using System;

namespace Matbox.BLL.Exceptions
{
    public class MaterialAlreadyInDbException : Exception
    {
        public MaterialAlreadyInDbException(string message)
            : base(message)
        {}
    }
}