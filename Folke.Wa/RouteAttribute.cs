using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Folke.Wa
{
    public class RouteAttribute : Attribute
    {
        public string Format { get; set; }
        public string Name { get; set; }

        public RouteAttribute(string format)
        {
            Format = format;
        }
    }
}
