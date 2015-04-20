using System.Threading;
using System.Threading.Tasks;

namespace Folke.Wa
{
    public class ActionResult<T, TU> : ActionResult, IHttpActionResult<T>
    {
        public ActionResult(string text, int statusCode) : base(text, statusCode)
        {
        }
    }

    public class ActionResult<T> : ActionResult<T, T>
    {
        public ActionResult(string text, int statusCode) : base(text, statusCode)
        {
        }
    }

    /// <summary>
    /// An IHttpActionResult that returns HTML
    /// </summary>
    public class ActionResult : IHttpActionResult
    {
        private readonly string text;
        private readonly int statusCode;

        public ActionResult(string text, int statusCode)
        {
            this.text = text;
            this.statusCode = statusCode;
        }

        public async Task ExecuteAsync(ICurrentContext currentContext, CancellationToken cancellationToken)
        {
            currentContext.Response.ContentType = "text/html; charset=utf-8";
            currentContext.Response.StatusCode = statusCode;
            await currentContext.Response.WriteAsync(text, cancellationToken);
        }
    }
}
