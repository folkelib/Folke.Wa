using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Folke.Wa
{
    public class ModelState
    {
        public Dictionary<string, List<string>> Messages { get; set; }

        public bool IsValid { get; set; }
        public object Model { get; set; }
        
        public ModelState()
        {
            IsValid = true;
        }

        public void AddModelError(string field, string message)
        {
            if (Messages == null)
                Messages = new Dictionary<string, List<string>>();
            if (Messages.ContainsKey(field))
                Messages[field].Add(message);
            else
                Messages.Add(field, new List<string> { message });
            IsValid = false;
        }
    }
}
