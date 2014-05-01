using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Folke.Wa
{
    public interface IWaConfig
    {
        string MapPath(string path);

        Folke.Wa.WaConfig.PathMatch Match(IOwinContext context);
        Task Run(IOwinContext context, Folke.Wa.WaConfig.PathMatch match);
        Container Container { get; set; }

        AbstractRoute GetRoute(string route);
        IView GetView(string name);
        void Configure(Container container);
        bool SendStaticContent(IOwinContext context);
        void AddStaticDirectory(string name);
        JsonSerializerSettings JsonSerializerSettings { get; set; }

        Dictionary<string, object> GetSession(IOwinContext context);
    }
}
