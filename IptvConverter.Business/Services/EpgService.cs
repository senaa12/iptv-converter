using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using IptvConverter.Business.Helpers;
using IptvConverter.Business.Models;
using IptvConverter.Business.Services.Interfaces;
using IptvConverter.Business.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace IptvConverter.Business.Services
{
    public class EpgService : IEpgService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _uploadFolder;

        public ILogger<EpgService> Logger;

        public EpgService(IHostingEnvironment enviroment, IHttpClientFactory httpClientFactory, ILogger<EpgService> logger)
        {
            _uploadFolder = FileService.GetEpgFolderPath(enviroment.IsDevelopment(), enviroment.ContentRootPath);
            _httpClientFactory = httpClientFactory;
            Logger = logger;
        }

        #region MAIN GENERATION
        public async Task GenerateXmlEpgFile(bool overrideExisting = false)
        {

            if (!overrideExisting && !shouldGenerateEpg())
                return;

            await EpgBuilderService.Configure(this, _uploadFolder).Build();

            writeLastGeneratedDateTime();
        }

        #endregion

        #region PROGRAMME FETCHER FUNCTIONS
        public async Task<XmlEpg> FetchEpgGzip(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            var client = _httpClientFactory.CreateClient();
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                Logger.LogInformation($"REQUEST FAILED WITH CODE {response.StatusCode}: {url}");
                return XmlEpg.Create();
            }

            using (GZipStream decompressionStream = new GZipStream(await response.Content.ReadAsStreamAsync(), CompressionMode.Decompress))
            {
                return new XmlEpgParser(decompressionStream).GetXmlEpg();
            }
        }

        public async Task<XmlEpg> FetchXmlEpg(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            var client = _httpClientFactory.CreateClient();
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                Logger.LogInformation($"REQUEST FAILED WITH CODE {response.StatusCode}: {url}");
                return XmlEpg.Create();
            }

            return new XmlEpgParser(await response.Content.ReadAsStreamAsync()).GetXmlEpg();
        }

        #endregion

        #region Chanell Readers
        public async Task<List<EpgChannelExtended>> GetEpgServiceChannels(string serviceUrl, bool fillCustomData = true)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, serviceUrl);

            var client = _httpClientFactory.CreateClient();
            var response = await client.SendAsync(request);

            var list = new XmlEpgParser(await response.Content.ReadAsStreamAsync()).Channels;
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
            if (!File.Exists(lastCheckedPath))
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
