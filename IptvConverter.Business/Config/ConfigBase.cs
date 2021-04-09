using Newtonsoft.Json;
using System;
using System.IO;

namespace IptvConverter.Business.Config
{
    public abstract class ConfigBase<T>
    {
        public T Config;

        protected abstract string FileName { get; }

        public ConfigBase() 
        {
            Config = loadJson();
        }

        private T loadJson()
        {
            using (StreamReader r = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "Json", $"{FileName}")))
            {
                string json = r.ReadToEnd();
                return JsonConvert.DeserializeObject<T>(json);
            }
        }
    }
}
