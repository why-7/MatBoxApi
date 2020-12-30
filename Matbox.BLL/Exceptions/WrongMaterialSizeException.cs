using System;

namespace Matbox.BLL.Exceptions
{
    public class WrongMaterialSizeException : Exception
    {
        public WrongMaterialSizeException(string message)
            : base(message)
        {}
    }
}