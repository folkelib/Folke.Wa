using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Folke.Wa
{
    public interface IActionResult<T>
    {
        Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken);
    }
}
