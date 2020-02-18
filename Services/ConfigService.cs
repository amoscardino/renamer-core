using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using static System.Environment;

namespace RenamerCore.Services
{
    public class ConfigService
    {
        private const string DIR_NAME = "renamer-core";
        private const string FILE_NAME = "renamer-core.json";

        public string GetValue(string key)
        {
            var config = GetConfig();

            if (config.ContainsKey(key))
                return config[key];

            return null;
        }

        public void SetValue(string key, string value)
        {
            var config = GetConfig();

            config[key] = value;

            SaveConfig(config);
        }

        private string GetDataPath()
        {
            // Use DoNotVerify in case LocalApplicationData doesn’t exist.
            var appData = Path.Combine(Environment.GetFolderPath(SpecialFolder.LocalApplicationData, SpecialFolderOption.DoNotVerify), DIR_NAME);

            // Ensure the directory and all its parents exist.
            var dir = Directory.CreateDirectory(appData);

            return Path.Combine(appData, FILE_NAME);
        }

        private Dictionary<string, string> GetConfig()
        {
            var dataPath = GetDataPath();

            try
            {
                var json = File.ReadAllText(dataPath);

                return JsonSerializer.Deserialize<Dictionary<string, string>>(json ?? "{}");
            }
            catch { }

            return new Dictionary<string, string>();
        }

        private void SaveConfig(Dictionary<string, string> config)
        {
            var dataPath = GetDataPath();
            var json = JsonSerializer.Serialize(config);

            try
            {
                File.WriteAllText(dataPath, json);
            }
            catch { }
        }
    }
}
