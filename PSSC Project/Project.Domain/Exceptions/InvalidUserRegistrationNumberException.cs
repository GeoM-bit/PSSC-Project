
using System.Runtime.Serialization;

namespace Project.Domain.Exceptions
{
    [Serializable]

    internal class InvalidUserRegistrationNumberException : Exception
    {
        public InvalidUserRegistrationNumberException()
        {
        }

        public InvalidUserRegistrationNumberException(string? message) : base(message)
        {
        }

        public InvalidUserRegistrationNumberException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected InvalidUserRegistrationNumberException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
