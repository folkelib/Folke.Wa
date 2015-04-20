using Microsoft.Owin;
using System;

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
            get { return modelState ?? (modelState = new ModelState(Context.Model)); }
        }

        protected ApiController(ICurrentContext context)
        {
            Context = context;
        }

        public IHttpActionResult<T> Ok<T>(T model)
        {
            return new JsonActionResult<T>(model, Config.JsonSerializerSettings);
        }

        public IHttpActionResult Ok()
        {
            return new JsonActionResult();
        }

        public IHttpActionResult Created(Uri uri, object model)
        {
            return new JsonActionResult(model, Config.JsonSerializerSettings, 201, uri.AbsolutePath);
        }

        protected IHttpActionResult<T> Created<T>(string routeName, int id, T content)
        {
            return new JsonActionResult<T>(content, Config.JsonSerializerSettings, 201, new Uri(Link(routeName, new {id})).AbsolutePath);
        }

        public IHttpActionResult Unauthorized()
        {
            return new JsonActionResult(statusCode: 401);
        }

        public IHttpActionResult<T> Unauthorized<T>()
        {
            return new JsonActionResult<T>(default(T), Config.JsonSerializerSettings, 401);
        }

        public IHttpActionResult<T> Unauthorized<T>(string message)
        {
            return new JsonActionResult<string, T>(message, Config.JsonSerializerSettings, 401);
        }

        public IHttpActionResult<T> NotFound<T>()
        {
            return new JsonActionResult<string, T>("Not found", Config.JsonSerializerSettings, 404);
        }

        public IHttpActionResult NotFound()
        {
            return new JsonActionResult("Not found", Config.JsonSerializerSettings, 404);
        }

        public IHttpActionResult Unauthorized(string message)
        {
            return new JsonActionResult(message, Config.JsonSerializerSettings, 401);
        }

        public IHttpActionResult<T> BadRequest<T>(ModelState badModelState)
        {
            return new JsonActionResult<object, T>(new { modelState = badModelState.Messages}, Config.JsonSerializerSettings, 400);
        }

        public IHttpActionResult BadRequest(ModelState badModelState)
        {
            return new JsonActionResult(new { modelState = badModelState.Messages }, Config.JsonSerializerSettings, 400);
        }

        public IHttpActionResult<T> BadRequest<T>()
        {
            return new JsonActionResult<T>(default(T), Config.JsonSerializerSettings, 400);
        }

        public IHttpActionResult BadRequest()
        {
            return new JsonActionResult(statusCode: 400);
        }

        public IHttpActionResult<T> BadRequest<T>(string message)
        {
            return new JsonActionResult<dynamic, T>(new {message }, Config.JsonSerializerSettings, 400);
        }

        public IHttpActionResult BadRequest(string message)
        {
            return new JsonActionResult(new { message }, Config.JsonSerializerSettings, 400);
        }

        public string Link(string route, object parameters)
        {
            return Context.Request.Scheme + "://" + Context.Request.Host.Value + Config.GetRoute(route).CreateLink(parameters);
        }
    }
}
