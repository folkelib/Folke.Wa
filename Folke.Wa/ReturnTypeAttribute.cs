using System;

namespace Folke.Wa
{
    public class ReturnTypeAttribute : Attribute
    {
        public Type ReturnType { get; set; }

        public ReturnTypeAttribute(Type returnType)
        {
            ReturnType = returnType;
        }
    }
}
