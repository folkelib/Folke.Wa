using Microsoft.Owin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Folke.Wa
{
    public abstract class ApiController : IController
    {
        public ICurrentContext Context { get; set; }
        public IWaConfig Config { get { return Context.Config; } }
        public IOwinRequest Request { get { return Context.Request; } }

        private ModelState modelState;
        public ModelState ModelState
        {
            get
            {
                if (modelState == null)
                    modelState = new ModelState(Context.Model);
                return modelState;
            }
        }

        public ApiController(ICurrentContext context)
        {
            Context = context;
        }

        /*public IHttpActionResult Ok(object model)
        {
            Context.Response.ContentType = "application/json";
            Context.Response.Write(JsonConvert.SerializeObject(model, Config.JsonSerializerSettings));
            return null;
        }*/

        public IActionResult<T> Ok<T>(T model)
        {
            Context.Response.ContentType = "application/json";
            Context.Response.Write(JsonConvert.SerializeObject(model, Config.JsonSerializerSettings));
            return null;
        }

        public IHttpActionResult Ok()
        {
            Context.Response.ContentType = "application/json";
            Context.Response.Write("null");
            return null;
        }

        public IHttpActionResult Created(Uri uri, object model)
        {
            Context.Response.StatusCode = 201;
            Context.Response.ContentType = "application/json";
            Context.Response.Headers["Location"] = uri.AbsolutePath;
            Context.Response.Write(JsonConvert.SerializeObject(model, Config.JsonSerializerSettings));
            return null;
        }

        protected IActionResult<T> Created<T>(string routeName, int id, T content)
        {
            Context.Response.StatusCode = 201;
            Context.Response.ContentType = "application/json";
            Context.Response.Headers["Location"] = new Uri(Link(routeName, new {id = id})).AbsolutePath;
            Context.Response.Write(JsonConvert.SerializeObject(content, Config.JsonSerializerSettings));
            return null;
        }

        public IHttpActionResult Unauthorized()
        {
            Context.Response.StatusCode = 401;
            Context.Response.Write("Unauthorized");
            return null;
        }

        public IActionResult<T> Unauthorized<T>()
        {
            Context.Response.StatusCode = 401;
            Context.Response.Write("Unauthorized");
            return null;
        }

        public IActionResult<T> BadRequest<T>(ModelState modelState)
        {
            Context.Response.StatusCode = 400;
            Context.Response.ContentType = "application/json";
            Context.Response.Write(JsonConvert.SerializeObject(new { modelState = modelState.Messages }, Config.JsonSerializerSettings));
            return null;
        }

        public IHttpActionResult BadRequest(ModelState modelState)
        {
            Context.Response.StatusCode = 400;
            Context.Response.ContentType = "application/json";
            Context.Response.Write(JsonConvert.SerializeObject(new { modelState = modelState.Messages }, Config.JsonSerializerSettings));
            return null;
        }

        public IActionResult<T> BadRequest<T>()
        {
            Context.Response.StatusCode = 400;
            return null;
        }

        public IHttpActionResult BadRequest()
        {
            Context.Response.StatusCode = 400;
            return null;
        }

        public IActionResult<T> BadRequest<T>(string message)
        {
            Context.Response.StatusCode = 400;
            Context.Response.ContentType = "application/json";
            Context.Response.Write(JsonConvert.SerializeObject(new { message = message }, Config.JsonSerializerSettings));
            return null;
        }

        public IHttpActionResult BadRequest(string message)
        {
            Context.Response.StatusCode = 400;
            Context.Response.ContentType = "application/json";
            Context.Response.Write(JsonConvert.SerializeObject(new { message = message }, Config.JsonSerializerSettings));
            return null;
        }

        public string Link(string route, object parameters)
        {
            return Context.Request.Scheme + "://" + Context.Request.Host.Value + Config.GetRoute(route).CreateLink(parameters);
        }
    }
}
