using System;

namespace CTRL.Middleware.Exceptions
{
    public class InvalidLoginAttemptException : Exception
    {
        public InvalidLoginAttemptException(string message): base(message)
        {
        }
    }
}
