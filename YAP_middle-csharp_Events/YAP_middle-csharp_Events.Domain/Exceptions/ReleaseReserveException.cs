using System;
using System.Collections.Generic;
using System.Text;

namespace YAP_middle_csharp_Events.Domain.Exceptions
{
    public class ReleaseReserveException(string message) : BaseApiException(message, 422, "Unprocessable Content ");
}
