using System;
using System.Collections.Generic;
using System.Web;

namespace PaynearmeCallbacks
{
    public class RequestException : Exception
    {
        private string message;
        private int status;
        public int StatusCode { get { return status; } }
        public string Message { get { return message; } }

        public RequestException(string message, int statusCode)
        {
            status = statusCode;
            this.message = message;
        }


    }
}