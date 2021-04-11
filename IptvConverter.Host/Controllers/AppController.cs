using Microsoft.AspNetCore.Mvc;
using IptvConverter.Business.Services.Interfaces;
using System.Net;
using IptvConverter.Business.Models;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace IptvConverter.Host.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("api")]
    public class AppController : ControllerBase
    {
        private readonly IPlaylistService _playlistService;
        private readonly IEpgService _epgService;

        public AppController(IPlaylistService playlistService, IEpgService epgService)
        {
            _playlistService = playlistService;
            _epgService = epgService;
        }

        [ProducesResponseType(typeof(FileContentResult), (int)HttpStatusCode.OK)]
        [HttpPost]
        [Route("playlist/from-file")]
        public async Task<IActionResult> GeneratePlaylist(IFormFile playlist)
        {
            var generatedFile = await _playlistService.BuildPlaylistFile(playlist);
            return File(generatedFile, "audio/x-mpegurl", "GeneratedPlaylist");
        }

        [ProducesResponseType(typeof(FileContentResult), (int)HttpStatusCode.OK)]
        [HttpPost]
        [Route("playlist/from-channels")]
        public async Task<IActionResult> GeneratePlaylist([FromBody] List<IptvChannel> channels)
        {
            var generatedFile = await _playlistService.BuildPlaylistFile(channels);
            return File(generatedFile, "audio/x-mpegurl", "GeneratedPlaylist");
        }

        [ProducesResponseType(typeof(AjaxResponse<List<IptvChannelExtended>>), (int)HttpStatusCode.OK)]
        [HttpPost]
        [Route("playlist/preview")]
        public async Task<IActionResult> ReadPlaylist(IFormFile playlist, bool fillData = true)
        {
            var channels = await _playlistService.ProcessPlaylist(playlist, fillData);
            return Ok(AjaxResponse<List<IptvChannelExtended>>.Success(channels));
        }

        [ProducesResponseType(typeof(FileContentResult), (int)HttpStatusCode.OK)]
        [HttpPost]
        [Route("epg/from-source")]
        public async Task<IActionResult> GeneratePlaylist(string source, bool fillData = true)
        {
            var channels = await _epgService.GetEpgServiceChannels(source, fillData);
            var ms = new MemoryStream();
            using (var sw = new StreamWriter(ms))
            {
                await sw.WriteAsync(JsonConvert.SerializeObject(channels));
            }

            return File(ms.ToArray(), "application/json", "Epg");
        }

        [ProducesResponseType(typeof(AjaxResponse<List<EpgChannelExtended>>), (int)HttpStatusCode.OK)]
        [HttpPost]
        [Route("epg/preview")]
        public async Task<IActionResult> ReadEpgChannels(string source, bool fillData = true)
        {
            return Ok(AjaxResponse<List<EpgChannelExtended>>.Success(await _epgService.GetEpgServiceChannels(source, fillData)));
        }
    }
}
