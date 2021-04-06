using Microsoft.AspNetCore.Mvc;
using IptvConverter.Business.Services.Interfaces;
using System.Net;
using IptvConverter.Business.Models;

namespace IptvConverter.Host.Controllers
{
    [Produces("application/json")] // without this it does not work for now
    [ApiController]
    [Route("api")]
    public class AppController : ControllerBase
    {
        private readonly IAppService _appService;

        public AppController(IAppService appService)
        {
            _appService = appService;
        }

        /// <summary>
        /// Test endpoint 
        /// </summary>
        /// <returns>Dummy value</returns>
        [ProducesResponseType(typeof(AjaxResponse), (int)HttpStatusCode.OK)]
        [HttpGet]
        public IActionResult Init()
        {
            return Ok(AjaxResponse.Success());
        }
    }
}
