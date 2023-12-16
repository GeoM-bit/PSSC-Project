using System.Runtime.Serialization;

namespace Project.Domain.Exceptions
{
    [Serializable]

    internal class InvalidOrderNumberException : Exception
    {
        public InvalidOrderNumberException()
        {
        }

        public InvalidOrderNumberException(string? message) : base(message)
        {
        }

        public InvalidOrderNumberException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected InvalidOrderNumberException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
