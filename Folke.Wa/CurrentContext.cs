using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Folke.Wa
{
    public class CurrentContext : ICurrentContext
    {
        private IOwinContext context;
        private IOwinRequest request;
        private IOwinResponse response;
        private IWaConfig config;

        public void Setup(IOwinContext context, IWaConfig config)
        {
            this.context = context;
            this.request = context.Request;
            this.response = context.Response;
            this.config = config;
        }

        public IOwinRequest Request { get { return request; } }
        public IOwinResponse Response { get { return response; } }
        public IWaConfig Config { get { return config; } }
        public object Model { get; set; }

        private Dictionary<string, object> session;

        public Dictionary<string, object> Session
        {
            get
            {
                if (session == null)
                    session = Config.GetSession(this);
                return session;
            }
        }

        public void SetCookie(string key, string value, CookieOptions options)
        {
            var text = string.Format("{0}={1}", key, value);
            var path = options.Path ?? "/";
            text += "; Path=" + path;
            if (options.Expires != null)
                text += "; Expires=" + options.Expires.Value.ToString("R").Replace(',',' ');
            if (options.Domain != null)
                text += "; Domain=" + options.Domain;

            response.Headers.AppendValues("Set-Cookie", text);
        }
    }
}
