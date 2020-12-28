using System;

namespace Matbox.BLL.Exceptions
{
    public class WrongCategoryException : Exception
    {
        public WrongCategoryException(string message)
            : base(message)
        {}
    }
}