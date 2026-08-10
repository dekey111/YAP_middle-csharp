using System;
using System.Collections.Generic;
using System.Text;

namespace YAP_middle_csharp_Auth.Application.Interfaces
{
    public interface IValidator<in T>
    {
        bool IsValid(T item);
        IEnumerable<string> GetErrors(T item);

    }
}
