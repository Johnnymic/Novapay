using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Domain.Exceptions
{
    public class NotFoundException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }
        public List<string> Errors { get; }

        public NotFoundException()
        { }



        public NotFoundException(
            string message,
            HttpStatusCode statusCode,
            IEnumerable<string> errors = null)
            : base(message)
        {
            StatusCode = statusCode;
            Errors = errors?.ToList() ?? new List<string>();
        }

        public NotFoundException(
            string message,
            Exception innerException)
            : base(message, innerException)
        {
            Errors = new List<string>();
        }

        public NotFoundException(string message, HttpStatusCode statusCode = HttpStatusCode.NotFound, List<string> errors = null)
           : base(message)
        {
            StatusCode = statusCode;
            Errors = errors ?? new List<string>();
        }


        public NotFoundException(string message, HttpStatusCode statusCode)
            : base(message)
        {
            StatusCode = statusCode;
        }

        public class ForbiddenException : Exception
        {
            public ForbiddenException(string message) : base(message) { }
        }

        public class ConflictException : Exception
        {
            public ConflictException(string message) : base(message) { }
        }

        public class InvalidOperationException : Exception
        {
            public InvalidOperationException(string message) : base(message) { }
        }
        public class OperationFailedException : Exception
        {

            public HttpStatusCode StatusCode { get; set; }
            public List<string> Errors { get; }

            public OperationFailedException()
            { }



            public OperationFailedException(
                string message,
                HttpStatusCode statusCode,
                IEnumerable<string> errors = null)
                : base(message)
            {
                StatusCode = statusCode;
                Errors = errors?.ToList() ?? new List<string>();
            }

            public OperationFailedException(
                string message,
                Exception innerException)
                : base(message, innerException)
            {
                Errors = new List<string>();
            }

            public OperationFailedException(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest, List<string> errors = null)
               : base(message)
            {
                StatusCode = statusCode;
                Errors = errors ?? new List<string>();
            }


            public OperationFailedException(string message, HttpStatusCode statusCode)
                : base(message)
            {
                StatusCode = statusCode;
            }


        }
    }
}
