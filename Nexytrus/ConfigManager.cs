using System.IO;
using System.Xml.Serialization;

namespace Nexytrus
{
    class ConfigManager
    {
        public static string CONFIG_NAME = "config.xml";
        public static ConfigData Config
        {
            get
            {
                if (!_configLoaded)
                    LoadConfig();
                return _config;
            }
            set => _config = value;
        }

        private static ConfigData _config;
        private static bool _configLoaded;
        public static void LoadConfig()
        {
            if (!File.Exists(CONFIG_NAME)) // create config file with default values
            {
                using FileStream fs = new FileStream(CONFIG_NAME, FileMode.Create);
                XmlSerializer xs = new XmlSerializer(typeof(ConfigData));
                ConfigData sxml = new ConfigData(true);
                xs.Serialize(fs, sxml);
                _config = sxml;
            }
            else // read configuration from file
            {
                using FileStream fs = new FileStream(CONFIG_NAME, FileMode.Open);
                XmlSerializer xs = new XmlSerializer(typeof(ConfigData));
                ConfigData sc = (ConfigData)xs.Deserialize(fs);
                _config = sc;
            }
            _configLoaded = true;
        }

        public static bool SaveConfigData(ConfigData config)
        {
            if (!File.Exists(CONFIG_NAME))
                return false; // don't do anything if file doesn't exist

            using FileStream fs = new FileStream(CONFIG_NAME, FileMode.Open);
            XmlSerializer xs = new XmlSerializer(typeof(ConfigData));
            xs.Serialize(fs, config);
            return true;
        }
    }
    public class ConfigData
    {
        public string CytrusUrl;
        public string BaseUrl;
        public int MaxConcurrency;

        public ConfigData()
        {
        }
        public ConfigData(bool d)
        {
            CytrusUrl = "https://launcher.cdn.ankama.com/cytrus.json";
            BaseUrl = "https://launcher.cdn.ankama.com/";
            MaxConcurrency = 20;
        }
    }
}
