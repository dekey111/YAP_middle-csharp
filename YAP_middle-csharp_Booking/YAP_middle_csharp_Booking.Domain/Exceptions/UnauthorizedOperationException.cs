

namespace YAP_middle_csharp_Booking.Domain.Exceptions
{
    public class UnauthorizedOperationException()
        : BaseApiException("Недостаточно прав для выполнение операции!", 403, "Access Denied");
}
