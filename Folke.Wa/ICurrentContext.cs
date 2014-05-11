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
    }
}
