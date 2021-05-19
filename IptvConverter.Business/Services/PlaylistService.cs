using IptvConverter.Business.Config;
using IptvConverter.Business.Helpers;
using IptvConverter.Business.Models;
using IptvConverter.Business.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IptvConverter.Business.Services
{
    public class PlaylistService : IPlaylistService
    {
        public async Task<List<IptvChannelExtended>> ReadPlaylist(IFormFile playlistFile, bool tryFillCustomData = true)
        {
            var result = new List<IptvChannelExtended>();

            using (StreamReader reader = new StreamReader(playlistFile.OpenReadStream()))
            {
                string line = reader.ReadLine();
                if (line.Contains("#EXTM3U"))
                {
                    line = reader.ReadLine();
                }

                while(line != null)
                {
                    if (line.Contains("#EXTINF"))
                    {
                        var channel = ParseIntoChannel(line, tryFillCustomData);
                        channel.Uri = reader.ReadLine();
                        result.Add(channel);
                    }

                    line = reader.ReadLine();
                }
            }

            return result;
        }

        public async Task<List<IptvChannelExtended>> ProcessPlaylist(IFormFile playlistFile, bool tryFillCustomData = true)
        {
            var readPlayList = await ReadPlaylist(playlistFile, tryFillCustomData);

            var processedChannels = new List<IptvChannelExtended>();
            foreach (var idToInsert in GeneralConfig.Instance.Config.ChannelsOrder)
            {
                var channelsToProcess = readPlayList.Where(x => x.ID == idToInsert).ToList();
                foreach (var channel in channelsToProcess)
                {
                    var config = Config.ChannelsConfig.Instance.GetById((int)idToInsert);
                    processedChannels.Add(new IptvChannelExtended
                    {
                        ID = channel.ID,
                        ExtInf = channel.ExtInf,
                        Group = channel.Group,
                        Name = channel.Name,
                        Uri = channel.Uri,
                        Recognized = channel.Recognized,
                        ShouldCollect = channel.ShouldCollect,
                        Country = channel.Country,
                        EpgId = config.EpgId,
                        Logo = config.Logo,
                        Pattern = config.Pattern
                    });

                    readPlayList.Remove(channel);
                }
            }

            var restOfTheChannels = readPlayList.Where(x => x.Recognized == true).OrderBy(x => x.ID).ToList();
            foreach (var channel in restOfTheChannels)
            {
                if (processedChannels.Any(x => x.ID == channel.ID) == true)
                {
                    continue;
                }

                var config = Config.ChannelsConfig.Instance.GetById((int)channel.ID);
                processedChannels.Add(new IptvChannelExtended
                {
                    ID = channel.ID,
                    ExtInf = channel.ExtInf,
                    Group = channel.Group,
                    Name = channel.Name,
                    Uri = channel.Uri,
                    Recognized = channel.Recognized,
                    ShouldCollect = channel.ShouldCollect,
                    Country = channel.Country,
                    EpgId = config.EpgId,
                    Logo = config.Logo,
                    Pattern = config.Pattern
                });

                readPlayList.Remove(channel);
            }

            foreach (var channel in readPlayList)
                processedChannels.Add(channel);

            return processedChannels;
        }

        public async Task<byte[]> BuildPlaylistFile(IFormFile playlist)
        {
            var channels = await ProcessPlaylist(playlist);
            return await BuildPlaylistFile(channels.Select(x => new IptvChannel(x)).ToList());
        }

        public async Task<byte[]> BuildPlaylistFile(List<IptvChannel> channels)
        {
            var rows = generatePlaylistRows(channels);

            var fs = new MemoryStream();
            using (var writer = new StreamWriter(fs))
            {
                foreach (string line in rows)
                    writer.WriteLine(line);
            }

            return fs.ToArray();
        }

        public IptvChannelExtended ParseIntoChannel(string line, bool includeCustomSettings = true)
        {
            var channel = new IptvChannelExtended();

            channel.Name = ChannelHelper.ExtractProgramName(line);
            channel.EpgId = ChannelHelper.ExtractEpgId(line);
            channel.ExtInf = ChannelHelper.ExtractExtInf(line);
            channel.Group = ChannelHelper.ExtractGroup(line);
            channel.Logo = ChannelHelper.ExtractGroup(line);

            if(!includeCustomSettings)
            {
                return channel;
            }

            var match = Config.ChannelsConfig.Instance.MatchChannelByName(channel.Name);
            if(match != null)
            {
                channel.Recognized = true;
                channel.Country = match.Country;
                channel.ID = match.ID;
                channel.ShouldCollect = match.ShouldCollect;
                channel.Logo = !string.IsNullOrEmpty(match.Logo) ? match.Logo : channel.Logo;
            }
            else
            {
                channel.Recognized = false;
            }

            return channel;
        }

        private List<string> generatePlaylistRows(List<IptvChannel> channels)
        {
            var lines = new List<string>();
            lines.Add("#EXTM3U");

            foreach (var ch in channels)
            {
                lines.Add(ch.GenerateChannelInfoRow());
                lines.Add(ch.Uri);
            }

            return lines;
        }
    }
}
