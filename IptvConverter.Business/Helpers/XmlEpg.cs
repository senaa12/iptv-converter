using IptvConverter.Business.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IptvConverter.Business.Helpers
{
    public class XmlEpg
    {
        private List<EpgProgramme> _programme;

        private List<EpgChannel> _channels;

        private Func<string, Func<EpgChannel, bool>> channelsMatchingFunction =
            (tester) => (x) => (!string.IsNullOrEmpty(x.ChannelEpgId) && tester.Equals(x.ChannelEpgId, System.StringComparison.OrdinalIgnoreCase))
                || (!string.IsNullOrEmpty(x.Name) && tester.Equals(x.Name, System.StringComparison.OrdinalIgnoreCase));


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
            filterOutChannel(channel);

            _channels.Add(channel);
            _programme.AddRange(programme);

            return this;
        }

        public XmlEpg AddChannels(List<EpgChannel> channels, List<EpgProgramme> programmes)
        {
            foreach (var newChannel in channels)
            {
                filterOutChannel(newChannel);
            }

            _channels.AddRange(channels);
            _programme.AddRange(programmes);

            return this;
        }

        private void filterOutChannel(EpgChannel channelToFilterOut)
        {
            var existingChannel = _channels.FirstOrDefault(channelsMatchingFunction(channelToFilterOut.ChannelEpgId));
            if (existingChannel == null)
                existingChannel = _channels.FirstOrDefault(channelsMatchingFunction(channelToFilterOut.Name));

            if (existingChannel == null)
                return;

            _programme = _programme.Where(x => x.ChannelId != existingChannel.ChannelEpgId).ToList();
            _channels = _channels.Where(x => x.ChannelEpgId != existingChannel.ChannelEpgId).ToList();
        }

        public void AddHoursToProgrammeTimeForChannel(string channelId, int addHours)
        {
            var channel = _channels.FirstOrDefault(channelsMatchingFunction(channelId));
            if (channel == null)
                return;

            foreach (var prog in _programme.Where(x => x.ChannelId == channel.ChannelEpgId))
            {
                prog.StartDate = prog.StartDate.AddHours(addHours);
                prog.EndDate = prog.EndDate.AddHours(addHours);
            }
        }

        /// <summary>
        ///  filters out if there is existing channel with newValue
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        public void ChangeEpgIdForChannel(string oldValue, string newValue)
        {
            var channel = _channels.FirstOrDefault(channelsMatchingFunction(oldValue));
            if (channel == null)
                return;

            channel.Name = newValue;
        }

        public bool ChannelExists(string channelId)
        {
            return _channels.Any(channelsMatchingFunction(channelId));
        }

        public byte[] GenerateEpgFile()
        {
            var fs = new MemoryStream();
            using (var writer = new StreamWriter(fs))
            {
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                writer.WriteLine("<tv generator-info-name=\"senny processed epg\" generator-info-url=\"https://iptvconverter.azurewebsites.net\">");

                foreach(var c in _channels.OrderBy(x => x.Name))
                {
                    writer.Write(c.ToXmlString());
                }

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
