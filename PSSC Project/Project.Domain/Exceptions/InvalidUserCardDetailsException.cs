using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Exceptions
{
    public class InvalidUserCardDetailsException : Exception
    {
        public InvalidUserCardDetailsException()
        {
        }

        public InvalidUserCardDetailsException(string? message) : base(message)
        {
        }

        public InvalidUserCardDetailsException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected InvalidUserCardDetailsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
