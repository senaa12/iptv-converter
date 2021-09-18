using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IptvConverter.Business.Helpers;
using IptvConverter.Business.Models;
using Microsoft.AspNetCore.Http;

namespace IptvConverter.Business.Services.Interfaces
{
    public interface IEpgService
    {
        Task GenerateXmlEpgFile(bool overrideExisting = false);

        Task<XmlEpg> FetchEpgGzip(string url);

        Task<XmlEpg> FetchXmlEpg(string url);

        Task<List<EpgChannelExtended>> GetEpgServiceChannels(string serviceUrl, bool fillCustomData = true);

        Task<List<EpgChannelExtended>> GetEpgServiceChannelsFromFile(IFormFile file, bool fillCustomData = true);

        Task<DateTime?> GetLastGenerationTime();
    }
}
