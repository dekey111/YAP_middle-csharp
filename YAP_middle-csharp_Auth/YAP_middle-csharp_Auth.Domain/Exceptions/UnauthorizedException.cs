
namespace YAP_middle_csharp_Auth.Domain.Exceptions
{
    public class UnauthorizedException() : BaseApiException("Ошибка авторизации", 401, "Unauthorized");
}
