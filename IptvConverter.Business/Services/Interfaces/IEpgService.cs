using IptvConverter.Business.Helpers;
using IptvConverter.Business.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IptvConverter.Business.Services.Interfaces
{
    public interface IEpgService
    {
        Task<List<EpgChannelExtended>> GetEpgServiceChannels(string serviceUrl, bool fillCustomData = true);

        Task<List<EpgChannelExtended>> GetEpgServiceChannelsFromFile(IFormFile file, bool fillCustomData = true);

        Task<XmlEpgParser> FetchEpgGzip(string url);

        Task GenerateXmlEpgFile(bool overrideExisting = false);

        Task<DateTime?> GetLastGenerationTime();
    }
}
