using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;
using YAP_middle_csharp.Contracts.EventModels;
using YAP_middle_csharp_Booking.Application.Interfaces.IApi;

namespace YAP_middle_csharp_Booking.Infrastructure.Api
{
    public class EventApiClient(HttpClient httpClient, ILogger<EventApiClient> logger) : IEventApiClient
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<EventApiClient> _logger = logger;


        /// <summary>
        /// Получить информацию о событии по его ID
        /// </summary>
        /// <param name="eventId">Уникальный идентификатор события</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Возвращает сущность события, иначе null</returns>
        public async Task<EventContract?> GetEventByIdAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/events/{eventId}", cancellationToken);
                if(response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<EventContract>(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[EventApiClient] [GetEventByIdAsync] Ошибка при получении события {EventId}", eventId);
                throw;
            }
        }
    }
}
