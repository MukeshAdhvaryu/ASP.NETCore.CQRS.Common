/* Licensed under the MIT/X11 license.
* This notice may not be removed from any source distribution.
 Author: Mukesh Adhvaryu.
*/

//-:cnd:noEmit
#if !TDD
//+:cnd:noEmit
using System.Net;
using System.Text.Json;

using CQRS.Common.Constants;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CQRS.Common.API.Middlewares
{
    public class HttpExceptionMiddleWare : IMiddleware
    {
        readonly ILogger<HttpExceptionMiddleWare> logger;
        public HttpExceptionMiddleWare(ILogger<HttpExceptionMiddleWare> _logger) 
        {
            logger = _logger;
        }

        async Task IMiddleware.InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                var problem = GetProblemDetails(context, next, e);
                var json = JsonSerializer.Serialize(problem);
                var response = context.Response;
                response.ContentType = Contents.JSON;
                await response.WriteAsync(json);
                await response.CompleteAsync();
            }
        }

        protected virtual ProblemDetails GetProblemDetails(HttpContext context, RequestDelegate next, Exception e)
        {
            return new ProblemDetails()
            {
                Type = "Error", // customize
                Title = "Error", //customize

                Status = (int)HttpStatusCode.ExpectationFailed, //customize
                Detail = e.Message,
            };
        }
    }
}
//-:cnd:noEmit
#endif
//+:cnd:noEmit
