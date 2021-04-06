using System.Collections.Generic;

namespace IptvConverter.Business.Models
{
    public class ConfigDto
    {
        public bool IncludeOnlyHd { get; set; }

        public List<int> ChannelsOrder { get; set; }
    }
}
