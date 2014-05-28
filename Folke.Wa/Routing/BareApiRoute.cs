using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Folke.Wa.Routing
{
    /// <summary>
    /// A route to a method that returns an object
    /// </summary>
    public class BareApiRoute : AbstractRoute
    {
        public BareApiRoute(string pattern, MethodInfo methodInfo, IWaConfig config)
            : base(pattern, methodInfo, config)
        {

        }

        public override Task Invoke(string[] path, ICurrentContext context)
        {
            var parameters = new object[NumberOfParameters];
            FillPathParameters(path, parameters);

            if (HasBody)
                FillJsonBody(context, parameters);
            FillQueryParameters(context, parameters);

            try
            {
                var ret = Invoke(context, parameters);
                context.Response.ContentType = "application/json; charset=utf-8";
                //if (ret != null)
                context.Response.Write(JsonConvert.SerializeObject(ret, config.JsonSerializerSettings));
            }
            catch (Exception e)
            {
                context.Response.StatusCode = 500;
                context.Response.Write(JsonConvert.SerializeObject(new { message = e.Message, details = e.ToString() }, config.JsonSerializerSettings));
            }
            return Task.Delay(0);
        }
    }
}
