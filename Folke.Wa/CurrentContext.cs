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

        private Dictionary<string, object> session;

        public Dictionary<string, object> Session
        {
            get
            {
                if (session == null)
                    session = Config.GetSession(context);
                return session;
            }
        }
    }
}
