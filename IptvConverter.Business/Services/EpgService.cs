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
            if (overrideExisting || !shouldGenerateEpg())
                return;

            var zagrebTime = DateTimeUtils.GetZagrebCurrentDateTime();
            using (FileStream fileStream = File.Create(Path.Combine(_uploadFolder, "guide.xml")))
            {
                var epgXml = XmlEpg.Create();

                #region phoenix rebornbuild
                var phoenixEpg = await FetchEpgGzip("https://epg.phoenixrebornbuild.com.hr/");
                epgXml.AddChannels(phoenixEpg.Channels, phoenixEpg.Programe);

                #endregion

                #region serbian forum
                var serbianForum = await FetchEpgGzip("http://epg.serbianforum.org/losmij/epg.xml.gz");
                var configChannel = Config.ChannelsConfig.Instance.MatchChannelByName("RTS 1");
                var rts1 = serbianForum.Channels.FirstOrDefault(x => string.Equals(x.ChannelEpgId, configChannel.EpgId, StringComparison.OrdinalIgnoreCase));
                epgXml.AddChannel(rts1, serbianForum.GetProgrammeForChannel(rts1.ChannelEpgId));

                configChannel = Config.ChannelsConfig.Instance.MatchChannelByName("RTS 2");
                var rts2 = serbianForum.Channels.FirstOrDefault(x => string.Equals(x.ChannelEpgId, configChannel.EpgId, StringComparison.OrdinalIgnoreCase));
                epgXml.AddChannel(rts2, serbianForum.GetProgrammeForChannel(rts2.ChannelEpgId));

                configChannel = Config.ChannelsConfig.Instance.MatchChannelByName("BHT 1");
                var bht1 = serbianForum.Channels.FirstOrDefault(x => string.Equals(x.ChannelEpgId, configChannel.EpgId, StringComparison.OrdinalIgnoreCase));
                epgXml.AddChannel(bht1, serbianForum.GetProgrammeForChannel(bht1.ChannelEpgId));

                #endregion

                #region mojtv.net xmltv
                var sk3Epg = await FetchXmlEpg($"https://mojtv.net/xmltv/service.ashx?kanal_id=401&date={zagrebTime.ToString("d.M.yyyy.")}");
                var sk3EpgTom = await FetchXmlEpg($"https://mojtv.net/xmltv/service.ashx?kanal_id=401&date={zagrebTime.AddDays(1).ToString("d.M.yyyy.")}");
                var fullProgramme = new List<EpgProgramme>();
                fullProgramme.AddRange(sk3Epg.Programe);
                fullProgramme.AddRange(sk3EpgTom.Programe);
                epgXml.AddChannel(sk3EpgTom.Channels.First(), fullProgramme);
                epgXml.ChangeEpgIdForChannel("sk3.rs", "SportKlub 3");
                epgXml.AddHoursToProgrammeTimeForChannel("SportKlub 3", 1);


                var sk2Epg = await FetchXmlEpg($"https://mojtv.net/xmltv/service.ashx?kanal_id=400&date={zagrebTime.ToString("d.M.yyyy.")}");
                var sk2EpgTom = await FetchXmlEpg($"https://mojtv.net/xmltv/service.ashx?kanal_id=400&date={zagrebTime.AddDays(1).ToString("d.M.yyyy.")}");
                fullProgramme = new List<EpgProgramme>();
                fullProgramme.AddRange(sk2Epg.Programe);
                fullProgramme.AddRange(sk2EpgTom.Programe);
                epgXml.AddChannel(sk2EpgTom.Channels.First(), fullProgramme);
                epgXml.ChangeEpgIdForChannel("sk2.rs", "SportKlub 2");
                epgXml.AddHoursToProgrammeTimeForChannel("SportKlub 2", 1);

                var sk1Epg = await FetchXmlEpg($"https://mojtv.net/xmltv/service.ashx?kanal_id=399&date={zagrebTime.ToString("d.M.yyyy.")}");
                var sk1EpgTom = await FetchXmlEpg($"https://mojtv.net/xmltv/service.ashx?kanal_id=399&date={zagrebTime.AddDays(1).ToString("d.M.yyyy.")}");
                fullProgramme = new List<EpgProgramme>();
                fullProgramme.AddRange(sk1Epg.Programe);
                fullProgramme.AddRange(sk1EpgTom.Programe);
                epgXml.AddChannel(sk1EpgTom.Channels.First(), fullProgramme);
                epgXml.ChangeEpgIdForChannel("sk1.rs", "SportKlub 1");
                epgXml.AddHoursToProgrammeTimeForChannel("SportKlub 1", 1);

                var foodN = await FetchXmlEpg($"https://mojtv.net/xmltv/service.ashx?kanal_id=265&date={zagrebTime.ToString("d.M.yyyy.")}");
                var foodNTom = await FetchXmlEpg($"https://mojtv.net/xmltv/service.ashx?kanal_id=265&date={zagrebTime.AddDays(1).ToString("d.M.yyyy.")}");
                fullProgramme = new List<EpgProgramme>();
                fullProgramme.AddRange(foodN.Programe);
                fullProgramme.AddRange(foodNTom.Programe);
                epgXml.AddChannel(foodNTom.Channels.First(), fullProgramme);

                if (!epgXml.ExistsProgrammeForChannel("HBO"))
                {
                    var hbo1 = await FetchXmlEpg($"https://mojtv.net/xmltv/service.ashx?kanal_id=366&date={zagrebTime.ToString("d.M.yyyy.")}");
                    var hbo1Tom = await FetchXmlEpg($"https://mojtv.net/xmltv/service.ashx?kanal_id=366&date={zagrebTime.AddDays(1).ToString("d.M.yyyy.")}");
                    fullProgramme = new List<EpgProgramme>();
                    fullProgramme.AddRange(hbo1.Programe);
                    fullProgramme.AddRange(hbo1Tom.Programe);
                    epgXml.AddChannel(hbo1Tom.Channels.First(), fullProgramme);
                }

                if (!epgXml.ExistsProgrammeForChannel("HBO 2"))
                {
                    var hbo2 = await FetchXmlEpg($"https://mojtv.net/xmltv/service.ashx?kanal_id=367&date={zagrebTime.ToString("d.M.yyyy.")}");
                    var hbo2Tom = await FetchXmlEpg($"https://mojtv.net/xmltv/service.ashx?kanal_id=367&date={zagrebTime.AddDays(1).ToString("d.M.yyyy.")}");
                    fullProgramme = new List<EpgProgramme>();
                    fullProgramme.AddRange(hbo2.Programe);
                    fullProgramme.AddRange(hbo2Tom.Programe);
                    epgXml.AddChannel(hbo2Tom.Channels.First(), fullProgramme);
                }

                if (!epgXml.ExistsProgrammeForChannel("HBO 3"))
                {
                    var hbo3 = await FetchXmlEpg($"https://mojtv.net/xmltv/service.ashx?kanal_id=368&date={zagrebTime.ToString("d.M.yyyy.")}");
                    var hbo3Tom = await FetchXmlEpg($"https://mojtv.net/xmltv/service.ashx?kanal_id=368&date={zagrebTime.AddDays(1).ToString("d.M.yyyy.")}");
                    fullProgramme = new List<EpgProgramme>();
                    fullProgramme.AddRange(hbo3.Programe);
                    fullProgramme.AddRange(hbo3Tom.Programe);
                    epgXml.AddChannel(hbo3Tom.Channels.First(), fullProgramme);
                }

                if (!epgXml.ExistsProgrammeForChannel("EuroSport 1"))
                {
                    var es = await FetchXmlEpg($"https://mojtv.net/xmltv/service.ashx?kanal_id=493&date={zagrebTime.ToString("d.M.yyyy.")}");
                    var esTom = await FetchXmlEpg($"https://mojtv.net/xmltv/service.ashx?kanal_id=493&date={zagrebTime.AddDays(1).ToString("d.M.yyyy.")}");
                    fullProgramme = new List<EpgProgramme>();
                    fullProgramme.AddRange(es.Programe);
                    fullProgramme.AddRange(esTom.Programe);
                    epgXml.AddChannel(es.Channels.First(), fullProgramme);
                    epgXml.ChangeEpgIdForChannel("esp1.rs", "EuroSport 1");
                }

                if (!epgXml.ExistsProgrammeForChannel("EuroSport 2"))
                {
                    var es = await FetchXmlEpg($"https://mojtv.net/xmltv/service.ashx?kanal_id=494&date={zagrebTime.ToString("d.M.yyyy.")}");
                    var esTom = await FetchXmlEpg($"https://mojtv.net/xmltv/service.ashx?kanal_id=494&date={zagrebTime.AddDays(1).ToString("d.M.yyyy.")}");
                    fullProgramme = new List<EpgProgramme>();
                    fullProgramme.AddRange(es.Programe);
                    fullProgramme.AddRange(esTom.Programe);
                    epgXml.AddChannel(es.Channels.First(), fullProgramme);
                    epgXml.ChangeEpgIdForChannel("esp2.rs", "EuroSport 2");
                }

                #endregion

                await fileStream.WriteAsync(epgXml.GenerateEpgFile());
                await fileStream.FlushAsync();
            }

            writeLastGeneratedDateTime();
        }

        #endregion

        #region FETCHER FUNCTIONS
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

        public async Task<List<EpgChannelExtended>> GetEpgServiceChannelsFromFile(IFormFile file, bool fillCustomData = true)
        {
            var list = new XmlEpgParser(file.OpenReadStream()).Channels;

            if (!fillCustomData)
            {
                return list.Select(x => new EpgChannelExtended(x, null)).ToList();
            }

            return list.Select(x =>
            {
                var match = Config.ChannelsConfig.Instance.MatchChannelByName(x.Name);
                return new EpgChannelExtended(x, match?.ID);
            }).OrderBy(x => x.ChannelId).ThenBy(x => x.Name).ToList();

        }

        #endregion

        private bool shouldGenerateEpg()
        {
            var lastCheckedPath = Path.Combine(_uploadFolder, "last_checked");
            if (!File.Exists(lastCheckedPath))
                return true;

            var currentZgDate = DateTimeUtils.GetZagrebCurrentDateTime();
            using (StreamReader r = new StreamReader(lastCheckedPath))
            {
                var dateCheck = r.ReadToEndAsync().GetAwaiter().GetResult();
                return DateTime.Parse(dateCheck).Date != currentZgDate.Date;
            }
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
