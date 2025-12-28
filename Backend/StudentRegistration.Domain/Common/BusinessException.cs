using System;
using System.Collections.Generic;

namespace StudentRegistration.Domain.Common
{
    public class BusinessException : Exception
    {
        public IEnumerable<string> Errors { get; }

        public BusinessException(string message) : base(message)
        {
            Errors = new List<string> { message };
        }

        public BusinessException(IEnumerable<string> errors) : base("Se encontraron errores de validación")
        {
            Errors = errors;
        }
    }
}
