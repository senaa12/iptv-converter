
using IptvConverter.Business.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace IptvConverter.Business.Services.Interfaces
{
    public interface IPlaylistService
    {
        Task<List<IptvChannelExtended>> ReadPlaylist(IFormFile playlistFile);

        Task<byte[]> GeneratePlaylist(IFormFile basePlaylist, List<int> customization = null);

        IptvChannelExtended ParseIntoChannel(string line);
    }
}
