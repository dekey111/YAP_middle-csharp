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


        /// <summary>
        /// Попытка зарезервировать место на событие
        /// </summary>
        /// <param name="eventId">Уникальный идентификатор события</param>
        /// <param name="seatsCount">Количество мест (по умолчанию 1) </param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>true - при успехе, иначе false </returns>
        public async Task<bool> TryReserveSeatAsync(Guid eventId, int seatsCount = 1, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/events/{eventId}/reserve", new ReserveSeatRequest { SeatsCount = seatsCount }, cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[EventApiClient] Ошибка при попытке зарезервировать место на событие {EventId}", eventId);
                return false;
            }
        }


        /// <summary>
        /// Освободить место на событие (при отмене брони)
        /// </summary>
        /// <param name="eventId">Уникальный идентификатор события</param>
        /// <param name="seatsCount">Количество мест (по умолчанию 1) </param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>true - при успехе, иначе false </returns>
        public async Task<bool> ReleaseSeatAsync(Guid eventId, int seatsCount = 1, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/events/{eventId}/release", new ReserveSeatRequest { SeatsCount = seatsCount }, cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[EventApiClient] Ошибка при освобождении места для события {EventId}", eventId);
                return false;
            }
        }
    }
}
