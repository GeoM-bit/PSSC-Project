using System.Runtime.Serialization;

namespace Project.Domain.Exceptions
{
    internal class InvalidOrderTelephoneException : Exception
    {
        public InvalidOrderTelephoneException()
        {
        }

        public InvalidOrderTelephoneException(string? message) : base(message)
        {
        }

        public InvalidOrderTelephoneException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected InvalidOrderTelephoneException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
