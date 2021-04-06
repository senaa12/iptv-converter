using Microsoft.AspNetCore.Mvc;
using IptvConverter.Business.Services.Interfaces;
using System.Net;
using IptvConverter.Business.Models;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace IptvConverter.Host.Controllers
{
    [Produces("application/json")] // without this it does not work for now
    [ApiController]
    [Route("api/playlist")]
    public class AppController : ControllerBase
    {
        private readonly IPlaylistService _playlistService;
        private readonly IAppService _appService;

        public AppController(IAppService appService, IPlaylistService playlistService)
        {
            _playlistService = playlistService;
            _appService = appService;
        }

        [ProducesResponseType(typeof(AjaxResponse<List<IptvChannelExtended>>), (int)HttpStatusCode.OK)]
        [HttpPost]
        [Route("generate")]
        public async Task<IActionResult> GeneratePlaylist(IFormFile playlist)
        {
            var generatedFile = await _playlistService.GeneratePlaylist(playlist, null);
            return File(generatedFile, "audio/x-mpegurl", "GeneratedPlaylist");
        }

        [ProducesResponseType(typeof(AjaxResponse<List<IptvChannelExtended>>), (int)HttpStatusCode.OK)]
        [HttpPost]
        [Route("read")]
        public async Task<IActionResult> ReadPlaylist(IFormFile playlist)
        {
            var channels = await _playlistService.ReadPlaylist(playlist);
            return Ok(channels);
        }
    }
}
