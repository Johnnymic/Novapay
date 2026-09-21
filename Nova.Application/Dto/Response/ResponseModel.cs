using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Application.Dto.Response
{

    public class ResponseModel<T>
    {
        public bool IsSuccessful { get; set; }
        public string Message { get; set; }

        public string ResponseMessage { get; set; }

        public List<string> ErrorMessage { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public T Data { get; set; }


        public ResponseModel()
        {
        }


        public ResponseModel(bool isSuccessful, string message, List<string> erroMessage, HttpStatusCode statusCode, T data)
        {
            IsSuccessful = isSuccessful;
            Message = message;
            ErrorMessage = erroMessage;
            StatusCode = statusCode;
            Data = data;
        }

        public ResponseModel(bool isSuccessful, string message, string responseMessage, List<string> list, HttpStatusCode statusCode, T data)
        {
            IsSuccessful = isSuccessful;
            Message = message;
            ResponseMessage = responseMessage;
            StatusCode = statusCode;
            Data = data;
        }



        public static ResponseModel<T> Fail(string message, List<string> erroMessage, HttpStatusCode statusCode, T data = default(T))
        {
            return new ResponseModel<T>(false, "Failed", erroMessage, statusCode, data);
        }



        public static ResponseModel<T> Ok(string responseMessage, T data = default(T))
        {
            return new ResponseModel<T>(true, "Successful", responseMessage, new List<string>(), (HttpStatusCode.OK), data);
        }


    }

}
