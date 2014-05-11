using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace Folke.Wa
{
    public class ModelState
    {
        public Dictionary<string, List<string>> Messages { get; set; }

        public bool IsValid { get; set; }
        public object Model { get; set; }
        
        public ModelState(object model)
        {
            IsValid = true;
            var type = model.GetType();
            var validationContext = new ValidationContext(model);
            foreach (var propertyInfo in type.GetProperties())
            {
                var attributes = propertyInfo.GetCustomAttributes<ValidationAttribute>();
                foreach (var validation in attributes)
                {
                    var result = validation.GetValidationResult(propertyInfo.GetValue(model), validationContext);
                    if (result != ValidationResult.Success)
                    {
                        IsValid = false;
                        AddModelError(propertyInfo.Name, result.ErrorMessage);
                    }
                }
            }
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
