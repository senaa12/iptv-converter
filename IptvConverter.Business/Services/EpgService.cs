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

        public async Task GenerateXmlEpgFile()
        {
            //if (!shouldGenerateEpg())
            //    return;

            var zagrebTime = DateTimeUtils.GetZagrebCurrentDateTime();
            using (FileStream fileStream = File.Create(Path.Combine(_uploadFolder, "guide.xml")))
            {
                var epgXml = XmlEpg.Create();

                #region phoenix rebornbuild
                var phoenixEpg = await FetchEpgGzip("https://epg.phoenixrebornbuild.com.hr/");
                epgXml.AddChannels(phoenixEpg.Channels, phoenixEpg.Programe);

                #endregion

                #region mojtv.net xmltv
                var sk3Epg = await FetchXmlEpg($"https://mojtv.net/xmltv/service.ashx?kanal_id=401&date={zagrebTime.ToString("d.M.yyyy.")}");
                epgXml.AddChannel(sk3Epg.Channels.First(), sk3Epg.Programe);
                epgXml.ChangeEpgIdForChannel("sk3.rs", "SportKlub 3");
                epgXml.AddHoursToProgrammeTimeForChannel("SportKlub 3", 1);


                var sk2Epg = await FetchXmlEpg($"https://mojtv.net/xmltv/service.ashx?kanal_id=400&date={zagrebTime.ToString("d.M.yyyy.")}");
                epgXml.AddChannel(sk2Epg.Channels.First(), sk2Epg.Programe);
                epgXml.ChangeEpgIdForChannel("sk2.rs", "SportKlub 2");
                epgXml.AddHoursToProgrammeTimeForChannel("SportKlub 2", 1);

                var sk1Epg = await FetchXmlEpg($"https://mojtv.net/xmltv/service.ashx?kanal_id=399&date={zagrebTime.ToString("d.M.yyyy.")}");
                epgXml.AddChannel(sk1Epg.Channels.First(), sk1Epg.Programe);
                epgXml.ChangeEpgIdForChannel("sk1.rs", "SportKlub 1");
                epgXml.AddHoursToProgrammeTimeForChannel("SportKlub 1", 1);

                var foodN = await FetchXmlEpg($"https://mojtv.net/xmltv/service.ashx?kanal_id=265&date={zagrebTime.ToString("d.M.yyyy.")}");
                epgXml.AddChannel(foodN.Channels.First(), foodN.Programe);
                epgXml.AddHoursToProgrammeTimeForChannel("Food Network", 1);

                var hbo1 = await FetchXmlEpg($"https://mojtv.net/xmltv/service.ashx?kanal_id=366&date={zagrebTime.ToString("d.M.yyyy.")}");
                epgXml.AddChannel(hbo1.Channels.First(), hbo1.Programe);
                epgXml.AddHoursToProgrammeTimeForChannel("HBO", 1);

                #endregion

                await fileStream.WriteAsync(epgXml.GenerateEpgFile());
                await fileStream.FlushAsync();
            }

            writeLastGeneratedDateTime();
        }

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
