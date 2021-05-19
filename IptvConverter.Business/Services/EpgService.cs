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

        public async Task GenerateXmlEpgFile()
        {
            if (!shouldGenerateEpg())
                return;

            using (FileStream fileStream = File.Create(Path.Combine(_uploadFolder, "guide.xml")))
            {
                   var phoenixEpg = await FetchEpgGzip("https://epg.phoenixrebornbuild.com.hr/");

                    var epgXml = XmlEpg.Create();
                    epgXml.AddChannels(phoenixEpg.Channels, phoenixEpg.Programe);

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
