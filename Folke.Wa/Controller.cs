using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Folke.Wa
{
    public abstract class Controller : IController
    {
        private ModelState modelState;
        public ModelState ModelState
        {
            get { return modelState ?? (modelState = new ModelState(Context.Model)); }
        }

        public ICurrentContext Context { get; set; }
        public IWaConfig Config { get { return Context.Config; } }
        public Dictionary<string, object> Session { get { return Context.Session; } }

        protected Controller(ICurrentContext context)
        {
            Context = context;
        }

        public IOwinRequest Request
        {
            get
            {
                return Context.Request;
            }
        }

        public ActionResult View(IView view)
        {
            return new ActionResult(view.Render(Context), 200);
        }


        public ActionResult View(string view)
        {
            return View(Context.Config.GetView(view));
        }

        public JsonActionResult Json(object value)
        {
            return new JsonActionResult(value, Config.JsonSerializerSettings);
        }

        public ActionResult BadRequestText(string message)
        {
            return new ActionResult(message, 400);
        }

        public ActionResult Redirect(string uri)
        {
            Context.Response.Redirect(uri);
            return null;
        }

        public ActionResult File(string path, string contentType)
        {
            var file = System.IO.File.ReadAllBytes(path);
            Context.Response.ContentType = contentType;
            Context.Response.Headers["Last-Modified"] = System.IO.File.GetLastWriteTimeUtc(path).ToString("R");
            Context.Response.Headers["Cache-Control"] = "max-age=86400";
            Context.Response.Expires = DateTimeOffset.Now.AddDays(7);
            Context.Response.Write(file);
            return null;
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
            return new JsonActionResult<T>(content, Config.JsonSerializerSettings, 201, new Uri(Link(routeName, new { id })).AbsolutePath);
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
            return new JsonActionResult<object, T>(new { modelState = badModelState.Messages }, Config.JsonSerializerSettings, 400);
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
            return new JsonActionResult<dynamic, T>(new { message }, Config.JsonSerializerSettings, 400);
        }

        public IHttpActionResult BadRequest(string message)
        {
            return new JsonActionResult(new { message }, Config.JsonSerializerSettings, 400);
        }

        public string Link(string route, object parameters)
        {
            return Context.Request.Scheme + "://" + Context.Request.Host.Value + Config.GetRoute(route).CreateLink(parameters);
        }

        protected Dictionary<string, FormPart> ParseMultipartFormData()
        {
           var request = Context.Request;
            var match = Regex.Match(request.ContentType, @"multipart/form-data;\s+boundary=(.*)", RegexOptions.IgnoreCase);
            if (!match.Success)
                throw new Exception("Not a multipart/form-data");
            var boundary = Encoding.ASCII.GetBytes(match.Groups[1].Value);
            var boundaryMarker = new byte[boundary.Length + 2];
            boundary.CopyTo(boundaryMarker, 2);
            var dash = Encoding.ASCII.GetBytes("-")[0];
            boundaryMarker[0] = dash;
            boundaryMarker[1] = dash;
            var body = request.Body;
            var bodyLength = int.Parse(request.Headers["content-length"]);
            var bytes = new byte[bodyLength];
            int offset = 0;
            while (offset < bytes.Length)
            {
                offset += request.Body.Read(bytes, offset, bytes.Length - offset);
            }
            var position = FindMarker(bytes, boundaryMarker, 0);
            var crlf = Encoding.ASCII.GetBytes("\r\n");
            var ret = new Dictionary<string, FormPart>();

            while (true)
            {
                if (position == -1)
                    break;
                if (bytes[position] == dash && bytes[position + 1] == dash)
                    break;
                if (bytes[position] != '\r' || bytes[position + 1] != '\n')
                    throw new Exception("malformed");
                position += 2;
                string name = null;
                var formPart = new FormPart();
                while (position < bytes.Length)
                {
                    if (bytes[position] == '\r' && bytes[position + 1] == '\n')
                        break;
                    var start = position;
                    position = FindMarker(bytes, crlf, position);
                    var header = Encoding.UTF8.GetString(bytes, start, position - start);
                    var contentMatch = Regex.Match(header, @"Content-Disposition:\s*form-data;\s*name=""([^""]+)""", RegexOptions.IgnoreCase);
                    if (contentMatch.Success)
                        name = contentMatch.Groups[1].Value;
                }
                position += 2;
                var startContent = position;
                position = FindMarker(bytes, boundaryMarker, position);
                var content = new byte[position - startContent];
                Buffer.BlockCopy(bytes, startContent, content, 0, position - startContent);
                formPart.Content = content;
                formPart.InputStream = new MemoryStream(content);
                ret[name] = formPart;
            }
            return ret;
        }  

        protected static int FindMarker(byte[] body, byte[] marker, int start)
        {
            for (var i = start; i< body.Length - marker.Length; i++)
            {
                var j = 0;
                while (body[i + j] == marker[j])
                {
                    j++;
                    if (j == marker.Length)
                        return i + j;
                }
            }
            return -1;
        }
    }
}
