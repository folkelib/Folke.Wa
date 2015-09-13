using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Folke.Wa
{
    public interface ICurrentContext
    {
        IOwinRequest Request { get; }
        IOwinResponse Response { get; }
        IWaConfig Config { get; }
        Dictionary<string, object> Session { get; }
        void Setup(IOwinContext context, IWaConfig config);
        object Model { get; set; }
        /// <summary>
        /// Used as a replacement of Response.Cookies, which is buggy in Microsoft.Owin.Host.HttpListener (as of now)
        /// </summary>
        /// <param name="key">The cookie name</param>
        /// <param name="value">Its value</param>
        /// <param name="options">Options (only path and expires are implemented)</param>
        void SetCookie(string key, string value, CookieOptions options);

        IOwinContext GetOwinContext();
    }
}
