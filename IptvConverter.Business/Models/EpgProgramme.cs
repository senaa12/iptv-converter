using System;
using System.Collections.Generic;

namespace IptvConverter.Business.Models
{
    public class EpgProgramme
    {
        public string Start { get; set; }

        public DateTime StartDate
        {
            get
            {
                var baseDate = $"{Start.Substring(0, 4)}-{Start.Substring(4, 2)}-{Start.Substring(6, 2)}";
                return DateTime.Parse($"{baseDate}T{Start.Substring(8, 2)}:{Start.Substring(10, 2)}:{Start.Substring(12, 2)}{Start.Split(' ')[1]}");
            }
        }

        public string End { get; set; }

        public DateTime EndDate
        {
            get
            {
                var baseDate = $"{End.Substring(0, 4)}-{End.Substring(4, 2)}-{End.Substring(6, 2)}";
                return DateTime.Parse($"{baseDate}T{End.Substring(8, 2)}:{End.Substring(10, 2)}:{End.Substring(12, 2)}{End.Split(' ')[1]}");
            }
        }

        public string ChannelId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public List<string> Directors { get; set; }

        public List<string> Actors { get; set; }

        public List<string> Category { get; set; }

        public string Icon { get; set; }

        public string ToXmlString()
        {
            var baseString = $"<programme start=\"{Start}\" stop=\"{End}\" channel=\"{ChannelId}\">\n";
            baseString = $"{baseString}<title lang=\"hr\">{Title}</title>\n";
            baseString = $"{baseString}<desc lang=\"hr\">{Description}</desc>~n";
            if((Actors != null && Actors.Count > 0) || (Directors != null && Directors.Count > 0))
            {
                baseString = $"{baseString}<credits>";
                if (Directors != null && Directors.Count > 0)
                {
                    Directors.ForEach(c =>
                    {
                        baseString = $"{baseString}<director>{c}</director>\n";
                    });
                }

                if(Actors != null && Actors.Count > 0)
                {
                    Actors.ForEach(c =>
                    {
                        baseString = $"{baseString}<actor>{c}</actor>\n";
                    });
                }


                baseString = $"{baseString}</credits>\n";
            }

            if(Category != null && Category.Count > 0)
            {
                Category.ForEach(c =>
                {
                    baseString = $"{baseString}<category lang=\"hr\">{c}</category>\n";
                });
            }

            if (!string.IsNullOrEmpty(Icon))
            {
                baseString = $"{baseString}<icon src=\"{Icon}\"/>";
            }

            baseString = $"{baseString}</programme>\n";

            return baseString;
        }

    }
}
