using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.Gui;
using Sandbox.Game.World;
using Sandbox.Graphics.GUI;
using VRage.Game;
using VRage.Game.Definitions;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;

namespace AutoEatPulsar
{
    internal sealed class FoodEntry { public MyDefinitionId Id; public string Name; }
    internal sealed class PriorityRow
    {
        public MyGuiControlLabel Label;
        public MyGuiControlButton Up;
        public MyGuiControlButton Down;
    }

    internal static class AutoEatRuntime
    {
        private const string ControlName = "AutoEatPulsarControl";
        private static readonly FieldInfo TerminalInstance = typeof(MyGuiScreenTerminal).GetField("m_instance", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly FieldInfo TerminalTabs = typeof(MyGuiScreenTerminal).GetField("m_terminalTabs", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo TerminalInteractedEntity = typeof(MyGuiScreenTerminal).GetField("m_interactedEntity", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly List<FoodEntry> Foods = new List<FoodEntry>();
        private static readonly Dictionary<string, FoodEntry> FoodsById = new Dictionary<string, FoodEntry>();
        private static readonly List<PriorityRow> PriorityRows = new List<PriorityRow>();
        private static AutoEatSettings settings;
        private static MyGuiScreenTerminal configuredTerminal;
        private static MyGuiControlButton enabledButton, warningButton, previousButton, nextButton;
        private static MyGuiControlLabel consumeLabel, warningLabel, foodCountLabel;
        private static int tick, lastNoFoodWarning, priorityOffset;

        public static void Start()
        {
            settings = SettingsStore.Load();
            RebuildFoodCatalog();
            SettingsStore.Save(settings);
        }

        public static void Stop()
        {
            configuredTerminal = null;
            PriorityRows.Clear();
            enabledButton = warningButton = previousButton = nextButton = null;
            consumeLabel = warningLabel = foodCountLabel = null;
            settings = null;
            Foods.Clear();
            FoodsById.Clear();
        }

        public static void Update()
        {
            if (settings == null || MySession.Static == null || MySession.Static.LocalCharacter == null) return;
            TryAddControlPanelSection();
            if (++tick % 60 != 0 || !settings.Enabled) return;

            MyCharacter character = MySession.Static.LocalCharacter;
            if (character.StatComp == null || character.StatComp.FoodRatio * 100f > settings.ConsumeAtPercent) return;
            MyInventory inventory = character.GetInventoryBase() as MyInventory;
            if (inventory == null) return;

            bool hadFood = false;
            foreach (FoodEntry food in OrderedFood())
            {
                bool available;
                bool consumed = MyInventory.TryConsumeItem(character, inventory, food.Id, out available, false);
                hadFood |= available;
                if (consumed)
                {
                    MyHud.Chat.ShowMessage("[AutoEat]", AutoEatLocalization.Text("Consumiu: ", "Consumed: ") + food.Name);
                    return;
                }
            }

            if (!hadFood && settings.WarnWhenNoFood && character.StatComp.FoodRatio * 100f <= settings.WarnAtPercent && tick - lastNoFoodWarning >= 1800)
            {
                lastNoFoodWarning = tick;
                ShowNoFoodWarning();
            }
        }

        public static void ToggleEnabled()
        {
            if (settings == null) return;
            settings.Enabled = !settings.Enabled;
            Save();
            MyHud.Chat.ShowMessage("[AutoEat]", settings.Enabled
                ? AutoEatLocalization.Text("Consumo automático ativado.", "Automatic eating enabled.")
                : AutoEatLocalization.Text("Consumo automático desativado.", "Automatic eating disabled."));
        }

        private static void TryAddControlPanelSection()
        {
            MyGuiScreenTerminal terminal = TerminalInstance.GetValue(null) as MyGuiScreenTerminal;
            if (terminal == null)
            {
                configuredTerminal = null;
                return;
            }
            MyGuiControlTabControl tabs = TerminalTabs.GetValue(terminal) as MyGuiControlTabControl;
            if (tabs == null) return;
            MyGuiControlTabPage page;
            if (!tabs.Pages.TryGetValue((int)MyTerminalPageEnum.ControlPanel, out page)) return;

            // The Control Panel tab is shared by the player terminal and every grid/block terminal.
            // Only the former has the local character as its interacted entity.
            object interactedEntity = TerminalInteractedEntity == null ? null : TerminalInteractedEntity.GetValue(null);
            if (interactedEntity != null && !(interactedEntity is MyCharacter))
            {
                RemoveControlPanelSection(page);
                configuredTerminal = terminal;
                return;
            }
            if (ReferenceEquals(terminal, configuredTerminal)) return;
            foreach (MyGuiControlBase control in page.Controls)
                if (control.Name == ControlName)
                {
                    configuredTerminal = terminal;
                    return;
                }
            AddControlPanelSection(page);
            configuredTerminal = terminal;
        }

        private static void RemoveControlPanelSection(MyGuiControlTabPage page)
        {
            List<MyGuiControlBase> remove = new List<MyGuiControlBase>();
            foreach (MyGuiControlBase control in page.Controls)
                if (control.Name == ControlName) remove.Add(control);
            foreach (MyGuiControlBase control in remove)
                page.Controls.Remove(control);

            PriorityRows.Clear();
            enabledButton = warningButton = previousButton = nextButton = null;
            consumeLabel = warningLabel = foodCountLabel = null;
        }

        private static void AddControlPanelSection(MyGuiControlTabPage page)
        {
            PriorityRows.Clear();
            MyGuiControlLabel title = AddLabel(page, -0.37f, -0.32f, "AUTOEAT", 0.9f);
            title.Name = ControlName;
            AddLabel(page, -0.37f, -0.25f, AutoEatLocalization.Text("Consumo automático:", "Automatic eating:"));
            enabledButton = AddButton(page, 0.10f, -0.25f, "", ToggleButton);
            consumeLabel = AddLabel(page, -0.37f, -0.18f, "");
            AddButton(page, 0.02f, -0.18f, "-5%", LowerConsumeThreshold);
            AddButton(page, 0.19f, -0.18f, "+5%", RaiseConsumeThreshold);
            AddLabel(page, -0.37f, -0.11f, AutoEatLocalization.Text("Avisar sem comida:", "Warn when no food:"));
            warningButton = AddButton(page, 0.10f, -0.11f, "", ToggleWarning);
            warningLabel = AddLabel(page, -0.37f, -0.04f, "");
            AddButton(page, 0.02f, -0.04f, "-5%", LowerWarningThreshold);
            AddButton(page, 0.19f, -0.04f, "+5%", RaiseWarningThreshold);
            foodCountLabel = AddLabel(page, -0.37f, 0.05f, "");
            AddFoodPriorityControls(page);
            AddLabel(page, -0.37f, 0.46f, AutoEatLocalization.Text("Ctrl + Alt + U alterna o consumo automático.", "Ctrl + Alt + U toggles automatic eating."), 0.62f);
            UpdatePanelText();
        }

        private static MyGuiControlLabel AddLabel(MyGuiControlTabPage page, float x, float y, string text, float scale = 0.72f)
        {
            MyGuiControlLabel label = new MyGuiControlLabel(new Vector2(x, y), text: text, textScale: scale, originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            label.Name = ControlName;
            page.Controls.Add(label);
            return label;
        }

        private static MyGuiControlButton AddButton(MyGuiControlTabPage page, float x, float y, string text, Action<MyGuiControlButton> action)
        {
            MyGuiControlButton button = new MyGuiControlButton(new Vector2(x, y), text: new StringBuilder(text), onButtonClick: action);
            button.Name = ControlName;
            page.Controls.Add(button);
            return button;
        }

        private static void AddFoodPriorityControls(MyGuiControlTabPage page)
        {
            const int rows = 5;
            for (int row = 0; row < rows; row++)
            {
                float y = 0.11f + row * 0.06f;
                int capturedRow = row;
                PriorityRow controls = new PriorityRow();
                controls.Label = AddLabel(page, -0.37f, y, "");
                controls.Up = AddButton(page, 0.16f, y, AutoEatLocalization.Text("SUBIR", "UP"), delegate(MyGuiControlButton b) { MovePriority(priorityOffset + capturedRow, -1); });
                controls.Down = AddButton(page, 0.28f, y, AutoEatLocalization.Text("DESCER", "DOWN"), delegate(MyGuiControlButton b) { MovePriority(priorityOffset + capturedRow, 1); });
                PriorityRows.Add(controls);
            }
            previousButton = AddButton(page, -0.08f, 0.41f, AutoEatLocalization.Text("ANTERIOR", "PREVIOUS"), PreviousFoodPage);
            nextButton = AddButton(page, 0.13f, 0.41f, AutoEatLocalization.Text("PRÓXIMA", "NEXT"), NextFoodPage);
        }

        private static void ToggleButton(MyGuiControlButton button) { ToggleEnabled(); UpdatePanelText(); }
        private static void ToggleWarning(MyGuiControlButton button) { settings.WarnWhenNoFood = !settings.WarnWhenNoFood; Save(); UpdatePanelText(); }
        private static void LowerConsumeThreshold(MyGuiControlButton button) { settings.ConsumeAtPercent = Clamp(settings.ConsumeAtPercent - 5); Save(); UpdatePanelText(); }
        private static void RaiseConsumeThreshold(MyGuiControlButton button) { settings.ConsumeAtPercent = Clamp(settings.ConsumeAtPercent + 5); Save(); UpdatePanelText(); }
        private static void LowerWarningThreshold(MyGuiControlButton button) { settings.WarnAtPercent = Clamp(settings.WarnAtPercent - 5); Save(); UpdatePanelText(); }
        private static void RaiseWarningThreshold(MyGuiControlButton button) { settings.WarnAtPercent = Clamp(settings.WarnAtPercent + 5); Save(); UpdatePanelText(); }

        private static void UpdatePanelText()
        {
            if (settings == null || enabledButton == null) return;
            enabledButton.Text = settings.Enabled ? AutoEatLocalization.Text("ATIVADO", "ENABLED") : AutoEatLocalization.Text("DESATIVADO", "DISABLED");
            warningButton.Text = settings.WarnWhenNoFood ? AutoEatLocalization.Text("ATIVADO", "ENABLED") : AutoEatLocalization.Text("DESATIVADO", "DISABLED");
            consumeLabel.Text = AutoEatLocalization.Text("Comer abaixo de: ", "Eat below: ") + settings.ConsumeAtPercent + "%";
            warningLabel.Text = AutoEatLocalization.Text("Avisar abaixo de: ", "Warn below: ") + settings.WarnAtPercent + "%";
            foodCountLabel.Text = AutoEatLocalization.IsPortuguese
                ? "PRIORIDADE DE COMIDA (" + settings.PriorityIds.Count + " detectada" + (settings.PriorityIds.Count == 1 ? ")" : "s)")
                : "FOOD PRIORITY (" + settings.PriorityIds.Count + " detected)";
            if (priorityOffset > Math.Max(0, settings.PriorityIds.Count - PriorityRows.Count))
                priorityOffset = Math.Max(0, settings.PriorityIds.Count - PriorityRows.Count);

            for (int row = 0; row < PriorityRows.Count; row++)
            {
                int index = priorityOffset + row;
                FoodEntry food = null;
                bool visible = index < settings.PriorityIds.Count && FoodsById.TryGetValue(settings.PriorityIds[index], out food);
                PriorityRow controls = PriorityRows[row];
                controls.Label.Visible = visible;
                controls.Up.Visible = visible && index > 0;
                controls.Down.Visible = visible && index + 1 < settings.PriorityIds.Count;
                if (visible) controls.Label.Text = (index + 1) + ". " + food.Name;
            }
            previousButton.Visible = priorityOffset > 0;
            nextButton.Visible = priorityOffset + PriorityRows.Count < settings.PriorityIds.Count;
        }

        private static void MovePriority(int index, int direction)
        {
            int target = index + direction;
            if (target < 0 || target >= settings.PriorityIds.Count) return;
            string item = settings.PriorityIds[index];
            settings.PriorityIds[index] = settings.PriorityIds[target];
            settings.PriorityIds[target] = item;
            Save();
            UpdatePanelText();
        }

        private static void PreviousFoodPage(MyGuiControlButton button)
        {
            priorityOffset = Math.Max(0, priorityOffset - PriorityRows.Count);
            UpdatePanelText();
        }

        private static void NextFoodPage(MyGuiControlButton button)
        {
            priorityOffset = Math.Min(Math.Max(0, settings.PriorityIds.Count - PriorityRows.Count), priorityOffset + PriorityRows.Count);
            UpdatePanelText();
        }

        private static int Clamp(int value) { return Math.Max(5, Math.Min(95, value)); }
        private static void Save() { SettingsStore.Save(settings); }

        private static void ShowNoFoodWarning()
        {
            string text = "[AutoEat] " + AutoEatLocalization.Text("SEM COMIDA NO INVENTÁRIO PESSOAL", "NO FOOD IN PERSONAL INVENTORY");
            MyHud.Notifications.Add(new MyHudNotification(
                MyStringId.GetOrCompute(text),
                5000,
                MyFontEnum.Red,
                MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER,
                0,
                MyNotificationLevel.Important));
        }

        private static void RebuildFoodCatalog()
        {
            Foods.Clear();
            FoodsById.Clear();
            foreach (MyPhysicalItemDefinition physical in MyDefinitionManager.Static.GetConsumableDefinitions())
            {
                MyConsumableItemDefinition food = physical as MyConsumableItemDefinition;
                if (food == null || !RestoresFood(food)) continue;
                FoodEntry entry = new FoodEntry { Id = food.Id, Name = food.DisplayNameText ?? food.Id.SubtypeName };
                Foods.Add(entry);
                FoodsById[entry.Id.ToString()] = entry;
            }
            Foods.Sort(delegate(FoodEntry left, FoodEntry right) { return string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase); });
            settings.PriorityIds.RemoveAll(delegate(string id) { return !FoodsById.ContainsKey(id); });
            foreach (FoodEntry food in Foods)
                if (!settings.PriorityIds.Contains(food.Id.ToString())) settings.PriorityIds.Add(food.Id.ToString());
        }

        private static IEnumerable<FoodEntry> OrderedFood()
        {
            foreach (string id in settings.PriorityIds)
            {
                FoodEntry entry;
                if (FoodsById.TryGetValue(id, out entry)) yield return entry;
            }
        }

        private static bool RestoresFood(MyConsumableItemDefinition definition)
        {
            if (definition.Stats == null) return false;
            foreach (MyConsumableItemDefinition.StatValue stat in definition.Stats)
                if (stat.Name == "Food" && stat.Value > 0f) return true;
            return false;
        }
    }
}
