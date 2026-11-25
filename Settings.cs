using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using VRage.FileSystem;
using VRage.Utils;

namespace AutoEatPulsar
{
    [Serializable]
    public sealed class AutoEatSettings
    {
        public bool Enabled = true;
        public int ConsumeAtPercent = 35;
        public bool WarnWhenNoFood = true;
        public int WarnAtPercent = 45;
        public List<string> PriorityIds = new List<string>();
    }

    internal static class SettingsStore
    {
        private const string FileName = "AutoEat.settings.xml";

        private static string SettingsPath
        {
            get
            {
                return System.IO.Path.Combine(MyFileSystem.UserDataPath, "Storage", "AutoEat", FileName);
            }
        }

        private static string LegacySettingsPath
        {
            get
            {
                return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Pulsar", "Legacy", "Local", "AutoEatPulsar.settings.xml");
            }
        }

        public static AutoEatSettings Load()
        {
            try
            {
                string path = File.Exists(SettingsPath) ? SettingsPath : LegacySettingsPath;
                if (!File.Exists(path)) return new AutoEatSettings();
                using (FileStream stream = File.OpenRead(path))
                    return (AutoEatSettings)new XmlSerializer(typeof(AutoEatSettings)).Deserialize(stream);
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLine("[AutoEat] Could not load settings: " + exception);
                return new AutoEatSettings();
            }
        }

        public static void Save(AutoEatSettings settings)
        {
            string temporaryPath = null;
            try
            {
                string directory = System.IO.Path.GetDirectoryName(SettingsPath);
                Directory.CreateDirectory(directory);
                temporaryPath = SettingsPath + ".tmp";
                using (FileStream stream = File.Create(temporaryPath))
                    new XmlSerializer(typeof(AutoEatSettings)).Serialize(stream, settings);
                File.Copy(temporaryPath, SettingsPath, true);
                File.Delete(temporaryPath);
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLine("[AutoEat] Could not save settings: " + exception);
                try
                {
                    if (temporaryPath != null && File.Exists(temporaryPath)) File.Delete(temporaryPath);
                }
                catch { }
            }
        }
    }
}
