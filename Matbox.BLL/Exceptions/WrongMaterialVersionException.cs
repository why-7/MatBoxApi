using System;

namespace Matbox.BLL.Exceptions
{
    public class WrongMaterialVersionException : Exception
    {
        public WrongMaterialVersionException(string message)
            : base(message)
        {}
    }
}