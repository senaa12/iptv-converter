using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IptvConverter.Business.Models;
using IptvConverter.Business.Utils;

namespace IptvConverter.Business.Helpers
{
    public class XmlEpg
    {
        private List<EpgProgramme> _programme;

        private List<EpgChannel> _channels;

        private Func<string, Func<EpgChannel, bool>> channelsMatchingFunction =
            (tester) => (x) => (!string.IsNullOrEmpty(x.ChannelEpgId) && tester.Equals(x.ChannelEpgId, System.StringComparison.OrdinalIgnoreCase))
                || (!string.IsNullOrEmpty(x.Name) && tester.Equals(x.Name, System.StringComparison.OrdinalIgnoreCase));

        public List<EpgProgramme> Programme
        {
            get
            {
                return _programme;
            }
        }

        public List<EpgChannel> Channels
        {
            get
            {
                return _channels;
            }
        }


        private XmlEpg(List<EpgProgramme> programmes = null, List<EpgChannel> channels = null)
        {
            _programme = programmes ?? new List<EpgProgramme>();
            _channels = channels ?? new List<EpgChannel>();
        }

        public static XmlEpg Create((List<EpgProgramme> Programmes, List<EpgChannel> Channels) data)
        {
            return new XmlEpg(data.Programmes, data.Channels);
        }

        public static XmlEpg Create()
        {
            return new XmlEpg();
        }

        public XmlEpg AddChannel(EpgChannel channel, List<EpgProgramme> programme, bool filterOutExisting = true, bool checkForToday = true)
        {
            if (filterOutExisting)
                filterOutExistingChannel(channel);

            if (checkForToday == true && !channelHasProgramme(channel, programme))
                return this;

            _channels.Add(channel);
            _programme.AddRange(programme);

            return this;
        }

        public XmlEpg AddChannels(List<EpgChannel> channels, List<EpgProgramme> programmes, bool filterOutExisting = true, bool checkForToday = true)
        {
            if (filterOutExisting)
            {
                foreach (var newChannel in channels)
                {
                    filterOutExistingChannel(newChannel);
                }
            }

            _channels.AddRange(channels.Where(x => (checkForToday == true && channelHasProgramme(x, programmes) == true) || checkForToday == false));
            _programme.AddRange(programmes);

            return this;
        }

        public List<EpgProgramme> GetProgrammeForChannel(string channelId)
        {
            return _programme.Where(x => x.ChannelId.Equals(channelId)).ToList();
        }

        public bool ExistsProgrammeForChannel(string channelId)
        {
            var channelsToProcess = _channels.Where(channelsMatchingFunction(channelId)).ToList();
            if (channelsToProcess == null || channelsToProcess.Count == 0)
                return false;

            foreach (var c in channelsToProcess)
            {
                if (channelHasProgramme(c, _programme))
                    return true;
            }

            return false;
        }

        private static bool channelHasProgramme(EpgChannel channel, List<EpgProgramme> programmes)
        {
            var zagrebToday = DateTimeUtils.GetZagrebCurrentDateTime();
            var existingProgramme = programmes
                .Where(x => x.ChannelId == channel.ChannelEpgId && x.StartDate.Date == zagrebToday.Date)
                .ToList();

            if (existingProgramme != null && existingProgramme.Count > 4)
                return true;

            return false;
        }

        private void filterOutExistingChannel(EpgChannel channelToFilterOut)
        {
            var existingChannel = _channels.Where(channelsMatchingFunction(channelToFilterOut.ChannelEpgId)).ToList();
            if (existingChannel == null || existingChannel.Count == 0)
                existingChannel = _channels.Where(channelsMatchingFunction(channelToFilterOut.Name)).ToList();

            if (existingChannel == null || existingChannel.Count == 0)
                return;

            _programme = _programme.Where(x => existingChannel.FirstOrDefault(y => y.ChannelEpgId == x.ChannelId) == null).ToList();
            _channels = _channels.Where(x => existingChannel.FirstOrDefault(y => y.ChannelEpgId == x.ChannelEpgId) == null).ToList();
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

            var programmeForChannel = _programme.Where(x => x.ChannelId == channel.ChannelEpgId);
            foreach (var prog in programmeForChannel)
            {
                prog.ChannelId = newValue;
            }

            channel.Name = newValue;
            channel.ChannelEpgId = newValue;
        }

        public bool ChannelExists(string channelId, out EpgChannel channel)
        {
            channel = _channels.FirstOrDefault(channelsMatchingFunction(channelId));
            return channel != null;
        }

        public byte[] GenerateEpgFile()
        {
            var fs = new MemoryStream();
            using (var writer = new StreamWriter(fs))
            {
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                writer.WriteLine("<tv generator-info-name=\"senny processed epg\" generator-info-url=\"https://iptvconverter.azurewebsites.net\">");

                foreach (var c in _channels.OrderBy(x => x.Name))
                {
                    writer.Write(c.ToXmlString());
                }

                foreach (var p in _programme)
                {
                    writer.Write(p.ToXmlString());
                }

                writer.WriteLine("</tv>");
            }

            return fs.ToArray();
        }
    }
}
