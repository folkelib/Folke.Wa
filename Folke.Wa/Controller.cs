using Microsoft.Owin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Folke.Wa
{
    public class Controller : IController
    {
        public ICurrentContext Context { get; set; }
        public IWaConfig Config { get { return Context.Config; } }
        public Dictionary<string, object> Session { get { return Context.Session; } }

        public Controller(ICurrentContext context)
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

        public ActionResult View(string viewName, string controller, object model = null)
        {
           /* var file = System.IO.File.ReadAllText("Views/" +  controller + "/" + view + ".cshtml");
            file = Regex.Replace(file, @"@model\s+[\w\.]+\s*", "");*/
            var view = Context.Config.GetView(viewName);
            Context.Response.ContentType = "text/html; charset=utf-8";
            Context.Response.Write(view.Render(Context, model));
            return null;
        }

        public ActionResult View(string view, object model = null)
        {
            var name = this.GetType().Name;
            var end = name.IndexOf("Controller");
            name = name.Substring(0, end);
            return View(view, name, model);
        }

        public ActionResult Json(object value)
        {
            Context.Response.ContentType = "application/json";
            Context.Response.Write(JsonConvert.SerializeObject(value, Config.JsonSerializerSettings));
            return null;
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
