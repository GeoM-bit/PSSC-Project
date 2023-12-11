using System.Runtime.Serialization;

namespace Project.Domain.Exceptions
{
    [Serializable]
    public class InvalidOrderPrice : Exception
    {
        public InvalidOrderPrice()
        {
        }

        public InvalidOrderPrice(string? message) : base(message)
        {
        }

        public InvalidOrderPrice(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected InvalidOrderPrice(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
