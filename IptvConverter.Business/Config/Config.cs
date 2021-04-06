using IptvConverter.Business.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace IptvConverter.Business.Config
{
    class Config
    {
        private static Config _instance;
        private ConfigDto _config;

        private Config()
        {
            _config = loadJson();
        }

        private ConfigDto loadJson()
        {
            ConfigDto items;
            using (StreamReader r = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "config.json")))
            {
                string json = r.ReadToEnd();
                items = JsonConvert.DeserializeObject<ConfigDto>(json);
            }

            return items;
        }

        public static Config Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Config();
                }

                return _instance;
            }
        }


        public List<int> GetChannelsOrder()
        {
            return _config.ChannelsOrder;
        }
    }
}
