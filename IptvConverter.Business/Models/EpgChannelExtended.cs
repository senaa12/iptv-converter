
namespace IptvConverter.Business.Models
{
    public class EpgChannelExtended : EpgChannel
    {
        public EpgChannelExtended(EpgChannel channel, int? id)
        {
            ChannelId = id;
            ChannelEpgId = channel.ChannelEpgId;
            Name = channel.Name;
            Logo = channel.Logo;
            Url = channel.Url;
        }

        public int? ChannelId { get; set; }
    }
}
