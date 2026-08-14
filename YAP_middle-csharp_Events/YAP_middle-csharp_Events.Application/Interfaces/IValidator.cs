using System;
using System.Collections.Generic;
using System.Text;

namespace YAP_middle_csharp_Events.Application.Interfaces
{
    public interface IValidator<in T>
    {
        bool IsValid(T item);
        IEnumerable<string> GetErrors(T item);

    }
}
