using System;
using System.Threading;
using System.Threading.Tasks;

namespace Folke.Wa
{
    public class FileActionResult : IHttpActionResult
    {
        private readonly string path;
        private readonly string contentType;

        public FileActionResult(string path, string contentType)
        {
            this.path = path;
            this.contentType = contentType;
        }

        public async Task ExecuteAsync(ICurrentContext currentContext, CancellationToken cancellationToken)
        {
            var file = System.IO.File.ReadAllBytes(path);
            currentContext.Response.ContentType = contentType;
            currentContext.Response.Headers["Last-Modified"] = System.IO.File.GetLastWriteTimeUtc(path).ToString("R");
            currentContext.Response.Headers["Cache-Control"] = "max-age=86400";
            currentContext.Response.Expires = DateTimeOffset.Now.AddDays(7);
            await currentContext.Response.WriteAsync(file, cancellationToken);
        }
    }
}
