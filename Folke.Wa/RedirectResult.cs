using System.Threading;
using System.Threading.Tasks;

namespace Folke.Wa
{
    public class HttpRedirectResult : IHttpActionResult
    {
        private readonly string uri;

        public HttpRedirectResult(string uri)
        {
            this.uri = uri;
        }

        public Task ExecuteAsync(ICurrentContext currentContext, CancellationToken cancellationToken)
        {
            currentContext.Response.Redirect(uri);
            return Task.Delay(0, cancellationToken);
        }
    }
}
