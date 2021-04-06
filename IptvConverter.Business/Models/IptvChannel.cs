
namespace IptvConverter.Business.Models
{
    public class IptvChannel : IptvChannelBaseDto
    {
        public string EpgId { get; set; }
        public string Group { get; set; }
        public string Logo { get; set; }
        public string ExtInf { get; set; }
        public string Uri { get; set; }

        public string GenerateChannelInfoRow()
        {
            var line = $"#EXTINF:{ExtInf}";
            if (!string.IsNullOrEmpty(EpgId))
                line = $"{line} tvg-id=\"{EpgId}\" tvg-name=\"{EpgId}\"";
            if (!string.IsNullOrEmpty(Logo))
                line = $"{line} tvg-logo=\"{Logo}\"";
            if (!string.IsNullOrEmpty(Group))
                line = $"{line} group-title=\"{Group}\"";
            line = $"{line}, {Name}";
            return line;
        }

        public IptvChannel() { }

        public IptvChannel(IptvChannelExtended extended)
        {
            ID = extended.ID;
            Group = extended.Group;
            ExtInf = extended.ExtInf;
            Logo = extended.Logo;
            Name = extended.Name;
            Uri = extended.Uri;
            EpgId = extended.EpgId;
        }
    }
}
