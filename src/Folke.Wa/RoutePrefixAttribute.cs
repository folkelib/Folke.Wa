using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Folke.Wa
{
    public class RoutePrefixAttribute : Attribute
    {
        public string Name { get; set; }

        public RoutePrefixAttribute(string name)
        {
            Name = name;
        }
    }
}
