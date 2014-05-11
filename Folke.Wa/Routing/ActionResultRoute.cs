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
    /// A route to a method that returns an ActionResult (typically, returns HTML)
    /// </summary>
    public class ActionResultRoute : AbstractRoute
    {
        public ActionResultRoute(string pattern, MethodInfo methodInfo, WaConfig config)
            : base(pattern, methodInfo, config)
        {

        }

        public override Task Invoke(string[] path, ICurrentContext context)
        {
            var parameters = new object[NumberOfParameters];
            FillPathParameters(path, parameters);

            if (HasBody)
                FillRequestBody(context, parameters);

            FillQueryParameters(context, parameters);

            try
            {
                /*var ret =*/ Invoke(context, parameters);
            }
            catch (Exception e)
            {
                context.Response.StatusCode = 500;
                context.Response.Write(JsonConvert.SerializeObject(new { message = e.Message, details = e.ToString() }, config.JsonSerializerSettings));
            }
            return Task.Delay(0);
        }

        private void FillRequestBody(ICurrentContext context, object[] parameters)
        {
            throw new NotImplementedException();
        }
    }
}
