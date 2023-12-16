using System.Runtime.Serialization;


namespace Project.Domain.Exceptions
{
    [Serializable]
    internal class InvalidOrderDeliveryAddress:Exception
    {
        public InvalidOrderDeliveryAddress()
        {
        }

        public InvalidOrderDeliveryAddress(string? message) : base(message)
        {
        }

        public InvalidOrderDeliveryAddress(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected InvalidOrderDeliveryAddress(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
