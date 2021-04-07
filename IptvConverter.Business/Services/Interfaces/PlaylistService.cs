
using IptvConverter.Business.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IptvConverter.Business.Services.Interfaces
{
    public class PlaylistService : IPlaylistService
    {
        public async Task<List<IptvChannelExtended>> ReadPlaylist(IFormFile playlistFile)
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
                        var channel = ParseIntoChannel(line);
                        channel.Uri = reader.ReadLine();
                        result.Add(channel);
                    }

                    line = reader.ReadLine();
                }
            }

            return result;
        }

        public async Task<List<IptvChannelExtended>> ProcessPlaylist(IFormFile playlistFile)
        {
            var readPlayList = await ReadPlaylist(playlistFile);

            var processedChannels = new List<IptvChannelExtended>();
            foreach (var idToInsert in Config.Config.Instance.GetChannelsOrder())
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

        public IptvChannelExtended ParseIntoChannel(string line)
        {
            var channel = new IptvChannelExtended();

            channel.Name = ChannelHelper.ExtractProgramName(line);
            channel.EpgId = ChannelHelper.ExtractEpgId(line);
            channel.ExtInf = ChannelHelper.ExtractExtInf(line);
            channel.Group = ChannelHelper.ExtractGroup(line);
            channel.Logo = ChannelHelper.ExtractGroup(line);

            var match = Config.ChannelsConfig.Instance.MatchChannelByName(channel.Name);
            if(match != null)
            {
                channel.Recognized = true;
                channel.Country = match.Country;
                channel.ID = match.ID;
                channel.ShouldCollect = match.ShouldCollect;
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
