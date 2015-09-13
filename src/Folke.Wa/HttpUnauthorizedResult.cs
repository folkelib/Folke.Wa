using System.Threading;
using System.Threading.Tasks;

namespace Folke.Wa
{
    public class HttpUnauthorizedResult : IHttpActionResult
    {
        public Task ExecuteAsync(ICurrentContext currentContext, CancellationToken cancellationToken)
        {
            currentContext.Response.StatusCode = 401;
            return Task.Delay(0, cancellationToken);
        }
    }
}
