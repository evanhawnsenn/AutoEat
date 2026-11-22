using Sandbox.Game.Components;
using Sandbox.Game.World;
using VRage.Game.Components;
using VRage.Input;

namespace AutoEatPulsar
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public sealed class AutoEatSession : MySessionComponentBase
    {
        public override void LoadData()
        {
            AutoEatRuntime.Start();
        }

        public override void UpdateAfterSimulation()
        {
            if (MySession.Static == null || MySession.Static.LocalHumanPlayer == null)
                return;

            AutoEatRuntime.Update();

            if (MyInput.Static.IsAnyCtrlKeyPressed() && MyInput.Static.IsAnyAltKeyPressed() && MyInput.Static.IsNewKeyPressed(MyKeys.U))
                AutoEatRuntime.ToggleEnabled();
        }

        protected override void UnloadData()
        {
            AutoEatRuntime.Stop();
        }
    }
}
