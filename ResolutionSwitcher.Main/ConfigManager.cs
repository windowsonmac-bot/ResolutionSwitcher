using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

namespace ResolutionSwitcher.Main
{
    /// <summary>
    /// Manages application configuration with JSON persistence
    /// Handles monitor settings, game profiles, and user preferences
    /// </summary>
    public class ConfigManager
    {
        private const string ConfigFileName = "config.json";
        private readonly string _configPath;
        private static readonly Logger _logger = Logger.Instance;

        public class MonitorConfig
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; } = "";

            [JsonProperty("deviceName")]
            public string DeviceName { get; set; } = "";

            [JsonProperty("isPrimary")]
            public bool IsPrimary { get; set; }

            [JsonProperty("defaultResolution")]
            public ResolutionConfig DefaultResolution { get; set; } = new ResolutionConfig();
        }

        public class ResolutionConfig
        {
            [JsonProperty("width")]
            public uint Width { get; set; }

            [JsonProperty("height")]
            public uint Height { get; set; }

            [JsonProperty("refreshRate")]
            public uint RefreshRate { get; set; }
        }

        public class GameProfile
        {
            [JsonProperty("id")]
            public string Id { get; set; } = Guid.NewGuid().ToString();

            [JsonProperty("name")]
            public string Name { get; set; } = "";

            [JsonProperty("targetMonitorId")]
            public int TargetMonitorId { get; set; }

            [JsonProperty("targetResolution")]
            public ResolutionConfig TargetResolution { get; set; } = new ResolutionConfig();

            [JsonProperty("gameName")]
            public string GameName { get; set; } = "";

            [JsonProperty("launchMethod")]
            public string LaunchMethod { get; set; } = "Steam";

            [JsonProperty("launchPath")]
            public string LaunchPath { get; set; } = "";

            [JsonProperty("launchMode")]
            public string LaunchMode { get; set; } = "autoRestore";
        }

        public class ResolutionPreset
        {
            [JsonProperty("name")]
            public string Name { get; set; } = "";

            [JsonProperty("width")]
            public uint Width { get; set; }

            [JsonProperty("height")]
            public uint Height { get; set; }
        }

        public class Config
        {
            [JsonProperty("version")]
            public string Version { get; set; } = "1.0";

            [JsonProperty("monitors")]
            public List<MonitorConfig> Monitors { get; set; } = new();

            [JsonProperty("profiles")]
            public List<GameProfile> Profiles { get; set; } = new();

            [JsonProperty("resolutionPresets")]
            public List<ResolutionPreset> ResolutionPresets { get; set; } = new();

            [JsonProperty("behavior")]
            public BehaviorConfig Behavior { get; set; } = new();
        }

        public class BehaviorConfig
        {
            [JsonProperty("startMinimized")]
            public bool StartMinimized { get; set; } = false;

            [JsonProperty("runOnStartup")]
            public bool RunOnStartup { get; set; } = false;

            [JsonProperty("showStatusWindow")]
            public bool ShowStatusWindow { get; set; } = true;

            [JsonProperty("enableDebugLogging")]
            public bool EnableDebugLogging { get; set; } = false;
        }

        private Config _config;

        public ConfigManager()
        {
            _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);
            _config = Load();
            
            // Apply debug logging setting from config
            Logger.Instance.SetLoggingEnabled(_config.Behavior.EnableDebugLogging);
        }

        /// <summary>
        /// Loads config from JSON file, or creates default if not exists
        /// </summary>
        public Config Load()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    _logger.LogInfo($"Loading config from {_configPath}");
                    var json = File.ReadAllText(_configPath);
                    var config = JsonConvert.DeserializeObject<Config>(json);
                    
                    if (config != null && ValidateConfig(config))
                    {
                        _logger.LogSuccess("Config loaded and validated successfully");
                        return config;
                    }
                    else
                    {
                        _logger.LogWarning("Config validation failed, creating new config");
                        return new Config();
                    }
                }
                else
                {
                    _logger.LogInfo("No config file found, creating new config");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error loading config", ex);
            }

            return new Config();
        }

        /// <summary>
        /// Validates config file integrity
        /// </summary>
        private bool ValidateConfig(Config config)
        {
            try
            {
                if (config.Monitors == null) config.Monitors = new List<MonitorConfig>();
                if (config.Profiles == null) config.Profiles = new List<GameProfile>();
                if (config.ResolutionPresets == null) config.ResolutionPresets = new List<ResolutionPreset>();
                if (config.Behavior == null) config.Behavior = new BehaviorConfig();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Config validation error", ex);
                return false;
            }
        }

        /// <summary>
        /// Saves config to JSON file
        /// </summary>
        public void Save()
        {
            try
            {
                _logger.LogInfo("Saving config to file");
                
                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore
                };

                var json = JsonConvert.SerializeObject(_config, settings);
                File.WriteAllText(_configPath, json);
                
                _logger.LogSuccess("Config saved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error saving config", ex);
                throw new Exception($"Error saving config: {ex.Message}");
            }
        }

        /// <summary>
        /// Initializes config with detected monitors
        /// </summary>
        public void InitializeFromDetectedMonitors(List<DisplayManager.MonitorInfo> monitors)
        {
            if (_config.Monitors.Any())
            {
                _logger.LogInfo("Config already initialized, skipping monitor detection");
                return;
            
            }

            _logger.LogInfo("Initializing config from detected monitors");

            foreach (var monitor in monitors)
            {
                _config.Monitors.Add(new MonitorConfig
                {
                    Id = monitor.Id,
                    Name = monitor.FriendlyName,
                    DeviceName = monitor.DeviceName,
                    IsPrimary = monitor.IsPrimary,
                    DefaultResolution = new ResolutionConfig
                    {
                        Width = monitor.Width,
                        Height = monitor.Height,
                        RefreshRate = monitor.RefreshRate
                    }
                });
            }

            // Add default presets if not exists
            if (!_config.ResolutionPresets.Any())
            {
                _config.ResolutionPresets = new List<ResolutionPreset>
                {
                    new() { Name = "960x720 (4:3)", Width = 960, Height = 720 },
                    new() { Name = "1080x810 (4:3)", Width = 1080, Height = 810 },
                    new() { Name = "1280x960 (4:3)", Width = 1280, Height = 960 },
                    new() { Name = "1440x1080 (4:3)", Width = 1440, Height = 1080 },
                    new() { Name = "1600x1200 (4:3)", Width = 1600, Height = 1200 },
                    new() { Name = "1152x690 (5:3)", Width = 1152, Height = 690 },
                    new() { Name = "1440x864 (5:3)", Width = 1440, Height = 864 },
                    new() { Name = "1728x1036 (5:3)", Width = 1728, Height = 1036 }
                };
            }

            Save();
            _logger.LogSuccess("Config initialization complete");
        }

        /// <summary>
        /// Factory reset - deletes config file and clears memory
        /// </summary>
        public void FactoryReset()
        {
            try
            {
                _logger.LogInfo("Factory reset initiated");
                
                if (File.Exists(_configPath))
                {
                    File.Delete(_configPath);
                    _logger.LogSuccess("Config file deleted");
                }

                _config = new Config();
                Logger.Instance.ClearLog();
                
                _logger.LogSuccess("Factory reset complete");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error during factory reset", ex);
                throw new Exception($"Error during factory reset: {ex.Message}");
            }
        }

        public Config GetConfig() => _config;
        public void SetConfig(Config config) => _config = config;
    }
}
