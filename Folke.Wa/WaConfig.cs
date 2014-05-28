using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleInjector;
using SimpleInjector.Extensions.ExecutionContextScoping;
using System.IO;
using Newtonsoft.Json.Serialization;
using System.Globalization;
using Folke.Wa.Routing;

namespace Folke.Wa
{
    /// <summary>
    /// The Wa configuration. Stores all the routes.
    /// </summary>
    public class WaConfig : IWaConfig
    {
        private Dictionary<string, List<AbstractRoute>> routes = new Dictionary<string, List<AbstractRoute>>();
        private Dictionary<string, AbstractRoute> routesByName = new Dictionary<string, AbstractRoute>();
        private Dictionary<string, IView> views = new Dictionary<string, IView>();
        private Dictionary<string, bool> staticDirectory = new Dictionary<string, bool>();
        public Container Container { get; set; }
        public JsonSerializerSettings JsonSerializerSettings { get; set; }

        public WaConfig()
        {
        }

        public void Configure(Container container)
        {
            Container = container;
            foreach (var c in Assembly.GetEntryAssembly().DefinedTypes.Where(t => t.IsClass && !t.IsAbstract && typeof(IController).IsAssignableFrom(t)))
            {
                var prefix = c.GetCustomAttribute<RoutePrefixAttribute>();
                if (prefix == null)
                    continue;
                foreach (var method in c.GetMethods().Where(t => t.GetCustomAttribute<NonActionAttribute>() == null))
                {
                    var routeAttribute = method.GetCustomAttribute<RouteAttribute>();
                    if (routeAttribute == null)
                        continue;
                    var route = routeAttribute.Format ?? "";
                    if (route.IndexOf("~") == 0)
                        route = route.Substring(1);
                    else if (route.Length > 0)
                        route = prefix.Name + "/" + route;
                    else
                        route = prefix.Name;

                    string methodName;
                    if (method.GetCustomAttribute<HttpPostAttribute>() != null)
                        methodName = "POST";
                    else if (method.GetCustomAttribute<HttpPutAttribute>() != null)
                        methodName = "PUT";
                    else if (method.GetCustomAttribute<HttpDeleteAttribute>() != null)
                        methodName = "DELETE";
                    else
                        methodName = "GET";
                    if (!routes.ContainsKey(methodName))
                        routes.Add(methodName, new List<AbstractRoute>());

                    AbstractRoute newRoute;
                    if (typeof(ActionResult).IsAssignableFrom(method.ReturnType))
                        newRoute = new ActionResultRoute(route, method, this);
                    else if (typeof(IHttpActionResult).IsAssignableFrom(method.ReturnType) || (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(IActionResult<>)))
                        newRoute = new HttpActionResultApiRoute(route, method, this);
                    else
                        newRoute = new BareApiRoute(route, method, this);
                    if (routeAttribute.Name != null)
                        routesByName[routeAttribute.Name] = newRoute;
                    routes[methodName].Add(newRoute);
                }
            }

            foreach (var c in Assembly.GetEntryAssembly().DefinedTypes.Where(t => t.IsClass && !t.IsAbstract && typeof(IView).IsAssignableFrom(t)))
            {
                var name = c.Name;
                var suffix = name.IndexOf("View");
                if (suffix > 0)
                    name = name.Substring(0, suffix);
                var constructor = c.GetConstructor(Type.EmptyTypes);
                if (constructor != null)
                    views.Add(name, (IView) constructor.Invoke(null));
            }
        }

        public struct PathMatch
        {
            public string[] pathParts;
            public AbstractRoute path;
            public bool success;
        }

        public PathMatch Match(IOwinContext context)
        {
            if (!routes.ContainsKey(context.Request.Method))
                return new PathMatch { success = false };

            var requestPath = context.Request.Path.Value.Substring(1);
            if (requestPath.EndsWith("/"))
                requestPath = requestPath.TrimEnd('/');
            var urlPath = requestPath.Split(new []{'/'}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var path in routes[context.Request.Method])
            {
                if (path.Match(urlPath))
                {
                    return new PathMatch { success = true, path = path, pathParts = urlPath };
                }
            }
            return new PathMatch { success = false };
        }

        public async Task Run(IOwinContext context, PathMatch match)
        {
            using (var executionContextScope = Container.BeginExecutionContextScope())
            {
                var currentContext = Container.GetInstance<ICurrentContext>();
                //TODO CurrentContextFactory ?
                currentContext.Setup(context, this);
                await match.path.Invoke(match.pathParts, currentContext);
            }
        }

        public AbstractRoute GetRoute(string route)
        {
            return routesByName[route];
        }

        public IView GetView(string name)
        {
            return views[name];
        }

        public string MapPath(string path)
        {
            if (path[0] == '~')
                return path.Substring(2);
            return path;
        }

        public void AddStaticDirectory(string name)
        {
            staticDirectory[name] = true;
        }

        public bool SendStaticContent(IOwinContext context)
        {
            var path = context.Request.Path.Value;
            if (path[0] == '/')
                path = path.Substring(1);
            var slash = path.IndexOf('/');
            if (slash < 0)
                return false;

            var root = path.Substring(0, slash);
            if (!staticDirectory.ContainsKey(root))
                return false;

            try
            {
                var lastDot = path.LastIndexOf('.');
                var extension = path.Substring(lastDot + 1);
                string contentType;
                switch (extension)
                {
                    case "js":
                        contentType = "application/javascript";
                        break;
                    case "html":
                        contentType = "text/html";
                        break;
                    case "png":
                        contentType = "image/png";
                        break;
                    case "jpeg":
                    case "jpg":
                        contentType = "image/jpeg";
                        break;
                    case "css":
                        contentType = "text/css";
                        break;
                    default:
                        contentType = null;
                        break;
                }

                if (!File.Exists(path))
                {
                    context.Response.StatusCode = 404;
                    context.Response.Write("File not found");
                    return true;
                }

                var lastModified = File.GetLastWriteTimeUtc(path);
                var ifModified = context.Request.Headers["If-Modified-Since"];
                if (ifModified != null)
                {
                    var since = DateTime.ParseExact(ifModified, CultureInfo.InvariantCulture.DateTimeFormat.RFC1123Pattern, CultureInfo.InvariantCulture);
                    if (since.AddMinutes(1) >= lastModified)
                    {
                        context.Response.StatusCode = 304;
                        return true;
                    }
                }
                /*if (contentType == null)
                {
                    context.Response.StatusCode = 500;
                }
                else*/
                {
                    var file = File.ReadAllBytes(path);
                    context.Response.ContentLength = file.Length;
                    context.Response.ContentType = contentType;
                    context.Response.Expires = DateTimeOffset.Now.AddDays(7);
                    context.Response.Headers["Last-Modified"] = lastModified.ToString("R");
                    context.Response.Headers["Cache-Control"] = "max-age=600";
                    context.Response.Write(file);
                }
                
            }
            catch (FileNotFoundException)
            {
                context.Response.StatusCode = 404;
            }
            return true;
        }

        private class Session
        {
            public DateTime expires;
            public Dictionary<string, object> content;
        }

        private IDictionary<string, Session> sessions = new Dictionary<string, Session>();

        public Dictionary<string, object> GetSession(ICurrentContext context)
        {
            var sessionId = context.Request.Cookies["session"];
            Session session;
            var now = DateTime.Now;
            var expires = now.AddHours(1);
            if (sessionId == null  || !sessions.ContainsKey(sessionId))
            {
                lock (sessions)
                {
                    if (sessionId == null || !sessions.ContainsKey(sessionId))
                    {
                        //Prune expired sessions
                        foreach (var key in sessions.Where(s => s.Value.expires < now).Select(s => s.Key).ToList())
                        {
                            sessions.Remove(key);
                        }

                        do
                        {
                            var generate = new byte[10];
                            var rand = new Random();
                            rand.NextBytes(generate);
                            sessionId = Convert.ToBase64String(generate);
                        }
                        while (sessions.ContainsKey(sessionId));
                        session = new Session { content = new Dictionary<string, object>() };
                        sessions[sessionId] = session;
                    }
                }
            }
            
            context.SetCookie("session", sessionId, new CookieOptions { Expires = expires });
            
            session = sessions[sessionId];
            session.expires = expires;
            return session.content;
        }
    }
}
