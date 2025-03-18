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
            return Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Application Support");
#elif UNITY_EDITOR_LINUX
            return Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".local", "share");
#endif
        }

        public ConfigManager()
        {
            Config = LoadOrCreateConfig();
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
            Config = LoadOrCreateConfig();
        }

        public void SetApiToken(string token)
        {
            Config.auth.apiToken = token;
            Save();
        }

        private Config LoadOrCreateConfig()
        {
            var configPath = ResolveConfigPath();
            if (!File.Exists(configPath))
            {
                var config = new Config();
                Toml.WriteFile(config, configPath);
                return config;
            }
            else
            {
                return Toml.ReadFile<Config>(configPath);
            }
        }
    }
}
