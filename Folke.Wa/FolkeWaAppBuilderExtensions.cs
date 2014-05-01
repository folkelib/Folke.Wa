using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Folke.Wa
{
    public static class FolkeWaAppBuilderExtensions
    {
        public static IAppBuilder UseWa(this IAppBuilder builder, IWaConfig config)
        {
            builder.Use((context, next) =>
            {
                if (config.SendStaticContent(context))
                    return Task.Delay(0);

                var match = config.Match(context);
                if (!match.success)
                    return next();
                return config.Run(context, match);
            });
            return builder;
        }
    }
}
