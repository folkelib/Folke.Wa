using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Folke.Wa.Routing
{
    public class AsyncHttpActionResultApiRoute<T> : AbstractRoute where T : IHttpActionResult
    {
        public AsyncHttpActionResultApiRoute(string pattern, MethodInfo methodInfo, IWaConfig config)
            : base(pattern, methodInfo, config)
        {
        }

        public async override Task Invoke(string[] path, ICurrentContext context, CancellationToken cancellationToken)
        {
            var parameters = new object[NumberOfParameters];
            FillPathParameters(path, parameters);

            if (HasBody)
                await FillJsonBody(context, parameters);
            FillQueryParameters(context, parameters);

            try
            {
                var result = await InvokeAsync<T>(context, parameters);
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

    public class AsyncHttpActionResultApiRoute : AsyncHttpActionResultApiRoute<IHttpActionResult>
    {
        public AsyncHttpActionResultApiRoute(string pattern, MethodInfo methodInfo, IWaConfig config) : base(pattern, methodInfo, config)
        {
        }
    }
}
