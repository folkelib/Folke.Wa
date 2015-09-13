using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Folke.Wa
{
    public interface IView
    {
        string Render(ICurrentContext context);
    }
}
