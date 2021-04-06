
namespace IptvConverter.Business.Models
{
    public class IptvChannelExtended : IptvChannel
    {
        public bool ShouldCollect { get; set; }
        public bool Recognized { get; set; }
        public string Pattern { get; set; }
        public string Country { get; set; }
        public bool Hd => Pattern.Contains("hd", System.StringComparison.OrdinalIgnoreCase) || Name.Contains("hd", System.StringComparison.OrdinalIgnoreCase);

    }
}
