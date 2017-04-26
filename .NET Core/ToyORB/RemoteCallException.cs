using System;

namespace ToyORB
{
    public class RemoteCallException : Exception
    {
        public RemoteCallException()
        {
        }

        public RemoteCallException(string message) : base(message)
        {
        }

        public RemoteCallException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
