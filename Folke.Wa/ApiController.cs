using System;

namespace Folke.Wa
{
    public abstract class ApiController : Controller
    {
        protected ApiController(ICurrentContext context):base(context)
        {
        }
    }
}
