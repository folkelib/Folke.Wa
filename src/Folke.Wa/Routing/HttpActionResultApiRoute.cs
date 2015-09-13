using System.Threading;
using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Folke.Wa.Routing
{
    /// <summary>
    /// A route to a method that returns an IHttpActionResult or an IActionResult
    /// </summary>
    public class HttpActionResultApiRoute : AbstractRoute
    {
        public HttpActionResultApiRoute(string pattern, MethodInfo methodInfo, WaConfig config)
            : base(pattern, methodInfo, config)
        {

        }

        public override async Task Invoke(string[] path, ICurrentContext context, CancellationToken cancellationToken)
        {
            var parameters = new object[NumberOfParameters];
            FillPathParameters(path, parameters);

            if (HasBody)
                await FillJsonBody(context, parameters);
            FillQueryParameters(context, parameters);

            try
            {
                var result = (IHttpActionResult)Invoke(context, parameters);
                await result.ExecuteAsync(context, cancellationToken);
            }
            catch (Exception e)
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json; charset=utf-8";
                context.Response.Write(JsonConvert.SerializeObject(new { message = e.Message, details = e.ToString() }, config.JsonSerializerSettings));
            }
        }
    }
}
