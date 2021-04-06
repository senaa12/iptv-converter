using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;
using IptvConverter.Business.Models;

namespace IptvConverter.Host.Middleware
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, ILogger<ErrorHandlerMiddleware> logger)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                AjaxResponse response = null;
                if (ex.GetType() == typeof(EntryPointNotFoundException))
                {
                    response = AjaxResponse.Error("Not Found");

                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(response));

                    return;
                }

                logger.LogError(ex, $"ERROR: {ex.Message}");

                response = AjaxResponse.Error("Internal Server Error");

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(response));

                return;
            }
        }
    }
}
