using IptvConverter.Business.Models;
using System.Collections.Generic;
using System.IO;

namespace IptvConverter.Business.Helpers
{
    public class XmlEpg
    {
        private List<EpgProgramme> _programme;

        private List<EpgChannel> _channels;

        private XmlEpg() 
        {
            _programme = new List<EpgProgramme>();
            _channels = new List<EpgChannel>();
        }

        public static XmlEpg Create()
        {
            return new XmlEpg();
        }

        public XmlEpg AddChannel(EpgChannel channel, List<EpgProgramme> programme)
        {
            _channels.Add(channel);
            _programme.AddRange(programme);

            return this;
        }

        public XmlEpg AddChannels(List<EpgChannel> channels, List<EpgProgramme> programmes)
        {
            _channels.AddRange(channels);
            _programme.AddRange(programmes);

            return this;
        }

        public byte[] GenerateEpgFile()
        {
            var fs = new MemoryStream();
            using (var writer = new StreamWriter(fs))
            {
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                writer.WriteLine("<tv generator-info-name=\"senny processed epg\" generator-info-url=\"https://iptvconverter.azurewebsites.net\">");

                _channels.ForEach(c =>
                {
                    writer.Write(c.ToXmlString());
                });

                _programme.ForEach(c =>
                {
                    writer.Write(c.ToXmlString());
                });

                writer.WriteLine("</tv>");
            }

            return fs.ToArray();
        }
    }
}
