using System;
using System.Collections.Generic;
using System.Linq;

namespace StudentRegistration.Domain.Common
{
    public class Result<T>
    {
        public bool IsSuccess { get; private set; }
        public T Data { get; private set; }
        public string Message { get; private set; }
        public IEnumerable<string> Errors { get; private set; }

        protected Result(bool isSuccess, T data, string message, IEnumerable<string> errors)
        {
            IsSuccess = isSuccess;
            Data = data;
            Message = message;
            Errors = errors ?? new List<string>();
        }

        public static Result<T> Success(T data, string message = "Operación exitosa")
        {
            return new Result<T>(true, data, message, null);
        }

        public static Result<T> Failure(string message, IEnumerable<string> errors = null)
        {
            return new Result<T>(false, default(T), message, errors);
        }

        public static Result<T> Failure(IEnumerable<string> errors)
        {
            return new Result<T>(false, default(T), "Operación fallida", errors);
        }
    }

    public class Result : Result<object>
    {
        protected Result(bool isSuccess, string message, IEnumerable<string> errors)
            : base(isSuccess, null, message, errors) { }

        public static Result Success(string message = "Operación exitosa")
        {
            return new Result(true, message, null);
        }

        public static new Result Failure(string message, IEnumerable<string> errors = null)
        {
            return new Result(false, message, errors);
        }

        /// <summary>
        /// Permite retornar errores de validación sin necesidad de un mensaje explícito.
        /// Evita que el compilador resuelva la sobrecarga hacia Result&lt;object&gt;.Failure(IEnumerable&lt;string&gt;)
        /// (lo cual devolvería Result&lt;object&gt; y causaría CS0266).
        /// </summary>
        public static Result Failure(IEnumerable<string> errors)
        {
            return new Result(false, "Operación fallida", errors);
        }
    }
}
