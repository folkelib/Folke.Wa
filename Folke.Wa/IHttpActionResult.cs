using System.Threading;
using System.Threading.Tasks;

namespace Folke.Wa
{
    public interface IHttpActionResult
    {
        Task ExecuteAsync(ICurrentContext currentContext, CancellationToken cancellationToken);
    }
}
