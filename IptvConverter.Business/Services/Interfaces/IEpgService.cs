using IptvConverter.Business.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IptvConverter.Business.Services.Interfaces
{
    public interface IEpgService
    {
        Task<List<EpgChannelExtended>> GetEpgServiceChannels(string serviceUrl, bool fillCustomData = true);

    }
}
