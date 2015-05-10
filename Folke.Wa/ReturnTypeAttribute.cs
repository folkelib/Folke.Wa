using System;

namespace Folke.Wa
{
    public class ReturnTypeAttribute
    {
        public Type ReturnType { get; set; }

        public ReturnTypeAttribute(Type returnType)
        {
            ReturnType = returnType;
        }
    }
}
