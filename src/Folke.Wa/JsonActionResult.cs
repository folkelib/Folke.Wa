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
        public T Model { get; }
        private readonly JsonSerializerSettings jsonSerializerSettings;
        public int StatusCode { get; }
        public string Location { get; }

        public JsonActionResult(T model, JsonSerializerSettings jsonSerializerSettings, int statusCode = 200, string location = null)
        {
            this.Model = model;
            this.jsonSerializerSettings = jsonSerializerSettings;
            this.StatusCode = statusCode;
            this.Location = location;
        }

        public async Task ExecuteAsync(ICurrentContext currentContext, CancellationToken cancellationToken)
        {
            if (Location != null)
                currentContext.Response.Headers["Location"] = Location;
            currentContext.Response.StatusCode = StatusCode;
            currentContext.Response.ContentType = "application/json; charset=utf-8";
            await currentContext.Response.WriteAsync(JsonConvert.SerializeObject(Model, jsonSerializerSettings), cancellationToken);
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
        public object Model { get; }
        private readonly JsonSerializerSettings jsonSerializerSettings;
        public int StatusCode { get; }
        public string Location { get; }

        public JsonActionResult(object model = null, JsonSerializerSettings jsonSerializerSettings = null, int statusCode = 200, string location = null)
        {
            this.Model = model;
            this.jsonSerializerSettings = jsonSerializerSettings;
            this.StatusCode = statusCode;
            this.Location = location;
        }

        public async Task ExecuteAsync(ICurrentContext currentContext, CancellationToken cancellationToken)
        {
            if (Location != null)
                currentContext.Response.Headers["Location"] = Location;
            currentContext.Response.StatusCode = StatusCode;
            currentContext.Response.ContentType = "application/json; charset=utf-8";
            await currentContext.Response.WriteAsync(Model == null? "null" : JsonConvert.SerializeObject(Model, jsonSerializerSettings), cancellationToken);
        }
    }
}
