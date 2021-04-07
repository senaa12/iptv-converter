using Microsoft.AspNetCore.Mvc;
using IptvConverter.Business.Services.Interfaces;
using System.Net;
using IptvConverter.Business.Models;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace IptvConverter.Host.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("api/playlist")]
    public class PlaylistController : ControllerBase
    {
        private readonly IPlaylistService _playlistService;
        private readonly IAppService _appService;

        public PlaylistController(IAppService appService, IPlaylistService playlistService)
        {
            _playlistService = playlistService;
            _appService = appService;
        }

        [ProducesResponseType(typeof(FileContentResult), (int)HttpStatusCode.OK)]
        [HttpPost]
        [Route("generate/file")]
        public async Task<IActionResult> GeneratePlaylist(IFormFile playlist)
        {
            var generatedFile = await _playlistService.BuildPlaylistFile(playlist);
            return File(generatedFile, "audio/x-mpegurl", "GeneratedPlaylist");
        }

        [ProducesResponseType(typeof(FileContentResult), (int)HttpStatusCode.OK)]
        [HttpPost]
        [Route("generate/channels")]
        public async Task<IActionResult> GeneratePlaylist([FromBody] List<IptvChannel> channels)
        {
            var generatedFile = await _playlistService.BuildPlaylistFile(channels);
            return File(generatedFile, "audio/x-mpegurl", "GeneratedPlaylist");
        }

        [ProducesResponseType(typeof(AjaxResponse<List<IptvChannelExtended>>), (int)HttpStatusCode.OK)]
        [HttpPost]
        [Route("preview")]
        public async Task<IActionResult> ReadPlaylist(IFormFile playlist)
        {
            var channels = await _playlistService.ProcessPlaylist(playlist);
            return Ok(AjaxResponse<List<IptvChannelExtended>>.Success(channels));
        }
    }
}
