using System.Runtime.Serialization;

namespace Project.Domain.Exceptions
{
    [Serializable]
    public class InvalidPrice : Exception
    {
        public InvalidPrice()
        {
        }

        public InvalidPrice(string? message) : base(message)
        {
        }

        public InvalidPrice(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected InvalidPrice(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
