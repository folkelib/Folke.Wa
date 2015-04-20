using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Folke.Wa
{
    /// <summary>
    /// A IHttpActionResult that returns JSON
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TU"></typeparam>
    public class JsonActionResult<T, TU> : IHttpActionResult<TU>
    {
        private readonly T model;
        private readonly JsonSerializerSettings jsonSerializerSettings;
        private readonly int statusCode;
        private readonly string location;

        public JsonActionResult(T model, JsonSerializerSettings jsonSerializerSettings, int statusCode = 200, string location = null)
        {
            this.model = model;
            this.jsonSerializerSettings = jsonSerializerSettings;
            this.statusCode = statusCode;
            this.location = location;
        }

        public async Task ExecuteAsync(ICurrentContext currentContext, CancellationToken cancellationToken)
        {
            if (location != null)
                currentContext.Response.Headers["Location"] = location;
            currentContext.Response.StatusCode = statusCode;
            currentContext.Response.ContentType = "application/json; charset=utf-8";
            await currentContext.Response.WriteAsync(JsonConvert.SerializeObject(model, jsonSerializerSettings), cancellationToken);
        }
    }

    public class JsonActionResult<T> : JsonActionResult<T, T>
    {
        public JsonActionResult(T model, JsonSerializerSettings jsonSerializerSettings, int statusCode = 200, string location = null) : base(model, jsonSerializerSettings, statusCode, location)
        {
        }
    }

    public class JsonActionResult : IHttpActionResult
    {
        private readonly object model;
        private readonly JsonSerializerSettings jsonSerializerSettings;
        private readonly int statusCode;
        private readonly string location;

        public JsonActionResult(object model = null, JsonSerializerSettings jsonSerializerSettings = null, int statusCode = 200, string location = null)
        {
            this.model = model;
            this.jsonSerializerSettings = jsonSerializerSettings;
            this.statusCode = statusCode;
            this.location = location;
        }

        public async Task ExecuteAsync(ICurrentContext currentContext, CancellationToken cancellationToken)
        {
            if (location != null)
                currentContext.Response.Headers["Location"] = location;
            currentContext.Response.StatusCode = statusCode;
            currentContext.Response.ContentType = "application/json; charset=utf-8";
            await currentContext.Response.WriteAsync(model == null? "null" : JsonConvert.SerializeObject(model, jsonSerializerSettings), cancellationToken);
        }
    }
}
