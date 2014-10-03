using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Folke.Wa
{
    public enum JsonUsage
    {
        Data,
        Service
    }

    public class JsonAttribute : Attribute
    {
        public JsonUsage Usage { get; set; }
        public bool Observable { get; set; }
    }
}
