using System;

namespace Matbox.BLL.Exceptions
{
    public class MaterialAlredyInDbException : Exception
    {
        public MaterialAlredyInDbException(string message)
            : base(message)
        {}
    }
}