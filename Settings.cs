using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

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
        private static readonly string Path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Pulsar", "Legacy", "Local", "AutoEatPulsar.settings.xml");

        public static AutoEatSettings Load()
        {
            try
            {
                if (!File.Exists(Path)) return new AutoEatSettings();
                using (FileStream stream = File.OpenRead(Path))
                    return (AutoEatSettings)new XmlSerializer(typeof(AutoEatSettings)).Deserialize(stream);
            }
            catch { return new AutoEatSettings(); }
        }

        public static void Save(AutoEatSettings settings)
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
            using (FileStream stream = File.Create(Path))
                new XmlSerializer(typeof(AutoEatSettings)).Serialize(stream, settings);
        }
    }
}
