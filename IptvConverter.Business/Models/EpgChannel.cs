using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace IptvConverter.Business.Models
{
    public class EpgChannel
    {
        public string Name { get; set; }

        public string ChannelEpgId { get; set; }

        public string Logo { get; set; }

        public string Url { get; set; }

        public string ToXmlString()
        {
            var baseString = $"<channel id=\"{ChannelEpgId}\">\n";
            baseString = $"{baseString}<display-name>{HttpUtility.HtmlEncode(Name)}</display-name>\n";
            if (!string.IsNullOrEmpty(Url))
            {
                baseString = $"{baseString}<url>{HttpUtility.HtmlEncode(Url)}</url>\n";
            }

            baseString = $"{baseString}</channel>\n";
            return baseString;
        }
    }
}
