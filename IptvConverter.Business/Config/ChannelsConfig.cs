using IptvConverter.Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace IptvConverter.Business.Config
{
    public class ChannelsConfig : ConfigBase<List<IptvChannelExtended>>
    {
        protected override string FileName => "channelsConfig.json";

        public ChannelsConfig() : base()
        {

        }

        private static ChannelsConfig _instance;
        public static ChannelsConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ChannelsConfig();
                }

                return _instance;
            }
        }

        public IptvChannelExtended GetById(int channelId)
        {
            return _instance.Config.FirstOrDefault(x => x.ID == channelId);
        }

        /// <summary>
        /// returns channel that are matched by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IptvChannelExtended MatchChannelByName(string name)
        {
            IptvChannelExtended channel = new IptvChannelExtended();
            var copy = _instance.Config;
            IEnumerable<IptvChannelExtended> test;

            var country = extractCountryFromString(name);
            if (!checkCollectingCountry(country))
            {
                return null;
            }

            var nameMatch = copy.FirstOrDefault(c =>
                removeHdFhdSigns(c.Name).ToLower() == removeHdFhdSigns(name).ToLower() ||
                removeHdFhdSigns(c.EpgId).ToLower() == removeHdFhdSigns(name).ToLower()
                );
            if (nameMatch != null)
            {
                return nameMatch;
            }

            var splittedValues = name.ToLower().Split(' ');
            foreach (var value in splittedValues)
            {
                test = copy.Where(c => c.Pattern.ToLower().Split(' ').Contains(value)).ToList();

                if (test.Count() == 0
                    && (copy.Count > 2 || value == "fhd" || value == "hd")) continue;
                else if ((test.Count() == 1 && test.First().ID != -1)
                    || (test.Count() == 2 && test.FirstOrDefault(x => x.ID == -1) != null))
                {
                    return test.First(x => x.ID != -1);
                }
                else
                {
                    copy = test.ToList();
                }
            }
            // nije se napravio nikakav filtar => ne zelimo taj kanal
            if (copy.Count > 2 ||
                copy.Count == 0 ||
                copy.FirstOrDefault(c => c.ID == 100) != null) return null;

            // vjv je izmedu 2 jezika dilema, po defaultu uzimamo srb
            //var find = copy.Find(c => c.Pattern.Split(' ').Contains("RS"));
            return copy.First();
        }

        private string extractCountryFromString(string name)
        {
            var reg = new Regex(@"\s(hr|rs|sr|srb|it|uk|de|slo|fr|tr)(\s)*");
            var match = reg.Match(name.ToLower());
            return match.Success ? match.Value.Trim() : null;
        }

        private bool checkCollectingCountry(string country)
        {
            if (country == "hr" || country == "srb" || country == "rs")
            {
                return true;
            }
            else if (country == "slo" || country == "de" || country == "uk"
                || country == "it" || country == "fr")
            {
                return false;
            }
            else
            {
                // nema nista, vrati true
                return true;
            }
        }

        private string removeHdFhdSigns(string toRemove)
        {
            return toRemove.Replace("FHD", "").Replace("fhd", "").Replace("HD", "").Replace("hd", "");
        }
    }
}
