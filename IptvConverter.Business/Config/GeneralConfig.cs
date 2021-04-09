using IptvConverter.Business.Models;

namespace IptvConverter.Business.Config
{
    public class GeneralConfig : ConfigBase<ConfigDto>
    {
        protected override string FileName => "generalConfig.json";

        public GeneralConfig() : base()
        {

        }

        private static GeneralConfig _instance;
        public static GeneralConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GeneralConfig();
                }

                return _instance;
            }
        }
    }
}
