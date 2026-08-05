
namespace YAP_middle_csharp_Booking.Domain.Exceptions
{
    public class EventExpiredException()
        : BaseApiException("Срок регистрации или проведения события уже истек", 400, "Event Registration Expired");
}
