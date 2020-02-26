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

        /// <summary>
        /// Retrieves a config value for the given key. Returns null if no value exists.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetValue(string key)
        {
            var config = GetConfig();

            if (config.ContainsKey(key))
                return config[key];

            return null;
        }

        /// <summary>
        /// Sets a config value for the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue(string key, string value)
        {
            var config = GetConfig();

            config[key] = value;

            SaveConfig(config);
        }

        /// <summary>
        /// Gets the path to the config file
        /// </summary>
        /// <returns></returns>
        private string GetDataPath()
        {
            // Use DoNotVerify in case LocalApplicationData doesn’t exist.
            var appData = Path.Combine(Environment.GetFolderPath(SpecialFolder.LocalApplicationData, SpecialFolderOption.DoNotVerify), DIR_NAME);

            // Ensure the directory and all its parents exist.
            var dir = Directory.CreateDirectory(appData);

            return Path.Combine(appData, FILE_NAME);
        }

        /// <summary>
        /// Retrieves a dictionary containing all the config values.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Updates the config file with a new set of values from a dictionary.
        /// </summary>
        /// <param name="config"></param>
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
