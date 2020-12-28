using System;

namespace Matbox.BLL.Exceptions
{
    public class MaterialNotInDbException : Exception
    {
        public MaterialNotInDbException(string message)
            : base(message)
        {}
    }
}