using IptvConverter.Business.Helpers;
using IptvConverter.Business.Models;
using IptvConverter.Business.Services.Interfaces;
using IptvConverter.Business.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace IptvConverter.Business.Services
{
    public class EpgService : IEpgService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly string _uploadFolder;

        public EpgService(IHostingEnvironment enviroment, IHttpClientFactory httpClientFactory)
        {
            _uploadFolder = FileService.GetEpgFolderPath(enviroment.IsDevelopment(), enviroment.ContentRootPath);
            _httpClientFactory = httpClientFactory;
        }

        #region MAIN GENERATION
        public async Task GenerateXmlEpgFile(bool overrideExisting = false)
        {
            if (!overrideExisting && !shouldGenerateEpg())
                return;

            var zagrebTime = DateTimeUtils.GetZagrebCurrentDateTime();
            using (FileStream fileStream = File.Create(Path.Combine(_uploadFolder, "guide.xml")))
            {
                var epgXml = XmlEpg.Create();

                #region phoenix rebornbuild - BASE
                var phoenixEpg = await FetchXmlEpg("http://cdn.iptvhr.net/tvdata/guide.xml");
                if(phoenixEpg.Programe.Count > 15000)
                {
                    phoenixEpg = await FetchEpgGzip("https://epg.phoenixrebornbuild.com.hr/");
                }

                epgXml.AddChannels(phoenixEpg.Channels, phoenixEpg.Programe);

                #endregion

                #region serbian forum
                var serbianForum = await FetchEpgGzip("http://epg.serbianforum.org/losmij/epg.xml.gz");
                var configChannel = Config.ChannelsConfig.Instance.MatchChannelByName("RTS 1");
                if (!epgXml.ExistsProgrammeForChannel(configChannel.EpgId))
                {
                    var rts1 = serbianForum.Channels.FirstOrDefault(x => string.Equals(x.ChannelEpgId, configChannel.EpgId, StringComparison.OrdinalIgnoreCase));
                    epgXml.AddChannel(rts1, serbianForum.GetProgrammeForChannel(rts1.ChannelEpgId));
                }

                configChannel = Config.ChannelsConfig.Instance.MatchChannelByName("RTS 2");
                if (!epgXml.ExistsProgrammeForChannel(configChannel.EpgId))
                {
                    var rts2 = serbianForum.Channels.FirstOrDefault(x => string.Equals(x.ChannelEpgId, configChannel.EpgId, StringComparison.OrdinalIgnoreCase));
                    epgXml.AddChannel(rts2, serbianForum.GetProgrammeForChannel(rts2.ChannelEpgId));
                }

                configChannel = Config.ChannelsConfig.Instance.MatchChannelByName("BHT 1");
                if (!epgXml.ExistsProgrammeForChannel(configChannel.EpgId))
                {
                    var bht1 = serbianForum.Channels.FirstOrDefault(x => string.Equals(x.ChannelEpgId, configChannel.EpgId, StringComparison.OrdinalIgnoreCase));
                    epgXml.AddChannel(bht1, serbianForum.GetProgrammeForChannel(bht1.ChannelEpgId));
                }

                #endregion

                #region mojtv.net xmltv
                string channelId;

                channelId = "SportKlub 1";
                if (!epgXml.ExistsProgrammeForChannel(channelId))
                {
                    var mojTvId = 399;
                    epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml, channelId, 1);
                }

                channelId = "SportKlub 2";
                if (!epgXml.ExistsProgrammeForChannel(channelId))
                {
                    var mojTvId = 400;
                    epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml, channelId, 1);
                }

                channelId = "SportKlub 3";
                if (!epgXml.ExistsProgrammeForChannel(channelId))
                {
                    var mojTvId = 401;
                    epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml, channelId, 1);
                }

                channelId = "SportKlub 4";
                if (!epgXml.ExistsProgrammeForChannel(channelId))
                {
                    var mojTvId = 240;
                    epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml, channelId, 1);
                }

                channelId = "SportKlub 5";
                if (!epgXml.ExistsProgrammeForChannel(channelId))
                {
                    var mojTvId = 241;
                    epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml, channelId, 1);
                }

                channelId = "SportKlub 6";
                if (!epgXml.ExistsProgrammeForChannel(channelId))
                {
                    var mojTvId = 242;
                    epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml, channelId, 1);
                }

                channelId = "Arena Sport 6";
                if (!epgXml.ExistsProgrammeForChannel(channelId))
                {
                    var mojTvId = 533;
                    epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml, channelId);
                }

                channelId = "Food Network";
                if (!epgXml.ExistsProgrammeForChannel(channelId))
                {
                    var mojTvId = 265;
                    epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml);
                }

                channelId = "HBO";
                if (!epgXml.ExistsProgrammeForChannel(channelId))
                {
                    var mojTvId = 366;
                    epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml);
                }

                channelId = "HBO 2";
                if (!epgXml.ExistsProgrammeForChannel(channelId))
                {
                    var mojTvId = 367;
                    epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml);
                }

                channelId = "HBO 3";
                if (!epgXml.ExistsProgrammeForChannel(channelId))
                {
                    var mojTvId = 368;
                    epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml);
                }

                channelId = "EuroSport 1";
                if (!epgXml.ExistsProgrammeForChannel(channelId))
                {
                    var mojTvId = 493;
                    epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml, channelId);
                }

                channelId = "EuroSport 2";
                if (!epgXml.ExistsProgrammeForChannel(channelId))
                {
                    var mojTvId = 494;
                    epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml, channelId);
                }

                channelId = "Jugoton";
                if (!epgXml.ExistsProgrammeForChannel(channelId))
                {
                    var mojTvId = 308;
                    epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml, channelId);
                }

                channelId = "National Geographic";
                if (!epgXml.ExistsProgrammeForChannel(channelId))
                {
                    var mojTvId = 48;
                    epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml, channelId);
                }

                channelId = "MTV Base";
                if (!epgXml.ExistsProgrammeForChannel(channelId))
                {
                    var mojTvId = 45;
                    epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml, channelId);
                }

                channelId = "MTV Hits";
                if (!epgXml.ExistsProgrammeForChannel(channelId))
                {
                    var mojTvId = 46;
                    epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml, channelId);
                }

                channelId = "MTV Adria";
                if (!epgXml.ExistsProgrammeForChannel(channelId))
                {
                    var mojTvId = 447;
                    epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml, channelId);
                }

                channelId = "VH1 Classic";
                if (!epgXml.ExistsProgrammeForChannel(channelId))
                {
                    var mojTvId = 133;
                    epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml, channelId);
                }
                #endregion

                await fileStream.WriteAsync(epgXml.GenerateEpgFile());
                await fileStream.FlushAsync();
            }

            writeLastGeneratedDateTime();
        }

        #endregion

        #region PROGRAMME FETCHER FUNCTIONS
        public async Task<XmlEpgParser> FetchEpgGzip(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            var client = _httpClientFactory.CreateClient();
            var response = await client.SendAsync(request);

            using (GZipStream decompressionStream = new GZipStream(await response.Content.ReadAsStreamAsync(), CompressionMode.Decompress))
            {
                return new XmlEpgParser(decompressionStream);
            }
        }

        public async Task<XmlEpgParser> FetchXmlEpg(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            var client = _httpClientFactory.CreateClient();
            var response = await client.SendAsync(request);

            return new XmlEpgParser(await response.Content.ReadAsStreamAsync());
        }

        private async Task<XmlEpg> fetchMojTvProgrammeForChannel(int channelId, XmlEpg baseProgrammeAddTo, string newEpgId = null, int? addHoursToProgrammeTime = null)
        {
            try
            {
                var zagrebTime = DateTimeUtils.GetZagrebCurrentDateTime();

                var todayScheduleTask = FetchXmlEpg($"https://mojtv.net/xmltv/service.ashx?kanal_id={channelId}&date={zagrebTime.ToString("d.M.yyyy.")}");
                var tommorowScheduleTask = FetchXmlEpg($"https://mojtv.net/xmltv/service.ashx?kanal_id={channelId}&date={zagrebTime.AddDays(1).ToString("d.M.yyyy.")}");
                await Task.WhenAll(todayScheduleTask, tommorowScheduleTask);

                var fullProgramme = new List<EpgProgramme>();

                fullProgramme.AddRange(todayScheduleTask.Result.Programe);
                fullProgramme.AddRange(tommorowScheduleTask.Result.Programe);

                baseProgrammeAddTo.AddChannel(todayScheduleTask.Result.Channels.First(), fullProgramme);
                if (addHoursToProgrammeTime != null)
                {
                    baseProgrammeAddTo.AddHoursToProgrammeTimeForChannel(todayScheduleTask.Result.Channels.First().ChannelEpgId, (int)addHoursToProgrammeTime);
                }

                if (!string.IsNullOrEmpty(newEpgId))
                {
                    var oldEpgId = todayScheduleTask.Result.Channels.First().ChannelEpgId;
                    baseProgrammeAddTo.ChangeEpgIdForChannel(oldEpgId, newEpgId);
                }

                await Task.Delay(500);

                return baseProgrammeAddTo;
            }
            catch
            {
                return baseProgrammeAddTo;
            }
        }

        #endregion

        #region Chanell Readers
        public async Task<List<EpgChannelExtended>> GetEpgServiceChannels(string serviceUrl, bool fillCustomData = true)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, serviceUrl);

            var client = _httpClientFactory.CreateClient();
            var response = await client.SendAsync(request);

            var list = new XmlEpgParser(await response.Content.ReadAsStreamAsync()).Channels;
            if(!fillCustomData)
            {
                return list.Select(x => new EpgChannelExtended(x, null)).ToList();
            }

            return list.Select(x =>
            {
                var match = Config.ChannelsConfig.Instance.MatchChannelByName(x.Name);
                return new EpgChannelExtended(x, match?.ID);
            }).OrderBy(x => x.ChannelId).ThenBy(x => x.Name).ToList();
        }

        public Task<List<EpgChannelExtended>> GetEpgServiceChannelsFromFile(IFormFile file, bool fillCustomData = true)
        {
            var list = new XmlEpgParser(file.OpenReadStream()).Channels;

            if (!fillCustomData)
            {
                return Task.FromResult(list.Select(x => new EpgChannelExtended(x, null)).ToList());
            }

            return Task.FromResult(list.Select(x =>
            {
                var match = Config.ChannelsConfig.Instance.MatchChannelByName(x.Name);
                return new EpgChannelExtended(x, match?.ID);
            }).OrderBy(x => x.ChannelId).ThenBy(x => x.Name).ToList());
        }

        #endregion

        public async Task<DateTime?> GetLastGenerationTime()
        {
            var lastCheckedPath = Path.Combine(_uploadFolder, "last_checked");
            if(!File.Exists(lastCheckedPath))
                return null;

            using (StreamReader r = new StreamReader(lastCheckedPath))
            {
                var dateCheck = await r.ReadToEndAsync();
                return DateTime.Parse(dateCheck);
            }
        }

        private bool shouldGenerateEpg()
        {
            var lastGenerationTime = GetLastGenerationTime().GetAwaiter().GetResult();
            if (lastGenerationTime == null)
                return true;

            var currentZgDate = DateTimeUtils.GetZagrebCurrentDateTime();
            return ((DateTime)lastGenerationTime).Date != currentZgDate.Date;
        }

        private void writeLastGeneratedDateTime()
        {
            var fs = new MemoryStream();
            using (var writer = new StreamWriter(fs))
            {
                writer.WriteLine(DateTimeUtils.GetZagrebCurrentDateTime().ToString());
            }

            using (FileStream fileStream = File.Create(Path.Combine(_uploadFolder, "last_checked")))
            {
                fileStream.Write(fs.ToArray());

                fileStream.FlushAsync().GetAwaiter().GetResult();
            }
        }
    }
}
