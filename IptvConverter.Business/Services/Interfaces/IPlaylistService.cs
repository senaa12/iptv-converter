
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

        Task<List<IptvChannelExtended>> ProcessPlaylist(IFormFile playlistFile);

        Task<byte[]> BuildPlaylistFile(IFormFile playlist);

        Task<byte[]> BuildPlaylistFile(List<IptvChannel> channels);

        IptvChannelExtended ParseIntoChannel(string line);
    }
}
