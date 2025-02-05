#nullable enable

using Nett;
using System.IO;
using System;

namespace TechtrainExtension.Config
{
    public class ConfigManager
    {
        public static string ConfigDirectoryName = "techtrain-railway";
        public static string ConfigFileName = "config.toml";

        public Config Config;

        public static string ResolveConfigPath()
        {
            var basePath = GetConfigBasePath();
            return Path.Join(basePath, ConfigDirectoryName, ConfigFileName);
        }

        private static string GetConfigBasePath()
        {
#if UNITY_EDITOR_WIN
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
#elif UNITY_EDITOR_OSX
            return Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Application Support";
#elif UNITY_EDITOR_LINUX
            return Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".local", "share");
#endif
        }

        public ConfigManager()
        {
            var configPath = ResolveConfigPath();
            if (!File.Exists(configPath))
            {
                Config = new Config();
                Toml.WriteFile(Config, configPath);
            }
            else
            {
                Config = Toml.ReadFile<Config>(configPath);
            }
        }

        public void Save()
        {
            var configPath = ResolveConfigPath();
            Toml.WriteFile(Config, configPath);
        }

        public void Reset()
        {
            Config = new Config();
            Save();
        }

        public void Reload()
        {
            var configPath = ResolveConfigPath();
            Config = Toml.ReadFile<Config>(configPath);
        }

        public void SetApiToken(string token)
        {
            Config.auth.apiToken = token;
            Save();
        }
    }
}
