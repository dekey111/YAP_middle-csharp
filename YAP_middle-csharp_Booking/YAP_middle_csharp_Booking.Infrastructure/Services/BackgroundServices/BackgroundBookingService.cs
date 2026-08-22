using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using YAP_middle_csharp.Contracts.BookingModel;
using YAP_middle_csharp_Booking.Application.Interfaces.IRepositories;
using YAP_middle_csharp_Booking.Application.Interfaces.IServices;
using YAP_middle_csharp_Booking.Domain.Models;

namespace YAP_middle_csharp_Booking.Infrastructure.Services.BackgroundServices
{
    /// <summary>
    /// Фоновый метод для обработки поступающих заказов
    /// </summary>
    /// <param name="serviceProvider">Принимает провайдер, чтобы найти BookingRepository</param>
    /// <param name="logger">Принимает логгер</param>
    public class BackgroundBookingService(IServiceScopeFactory serviceProvider, ILogger<BackgroundBookingService> logger) : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceProvider = serviceProvider;
        private readonly ILogger<BackgroundBookingService> _logger = logger;


        /// <summary>
        /// Метод ожидания новых необработанных заявок
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[BackgroundBookingService] Фоновый сервис запущен");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    List<Guid> pendingBooksIds = new();
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var repository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
                        var pendingBooks = await repository.FindPendingBookingsAsync();
                        pendingBooksIds = pendingBooks.Select(x => x.Id).ToList();
                    }

                    if (pendingBooksIds.Any())
                    {
                        var tasks = pendingBooksIds.Select(id => ProcessBookingAsync(id, stoppingToken));
                        await Task.WhenAll(tasks);
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogDebug("[BackgroundBookingService] Задача была отменена");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[BackgroundBookingService] Ошибка обработки задачи");
                }

                await Task.Delay(2000);
            }
        }

        /// <summary>
        /// Метод обработки необработанного заказа
        /// </summary>
        /// <param name="booking">Сущность бронирования</param>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        private async Task ProcessBookingAsync(Guid pendingBookId, CancellationToken stoppingToken)
        {
            await Task.Delay(2000, stoppingToken);
            using (var scope = _serviceProvider.CreateScope())
            {
                var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
                var eventProducer = scope.ServiceProvider.GetRequiredService<IKafkaEventProducer>();

                BookingModel? booking = null;

                try
                {
                    booking = await bookingRepository.FindByIdAsync(pendingBookId, stoppingToken);

                    if (booking == null || booking.Status != BookingStatusEnum.Pending)
                        return;


                    _logger.LogInformation("[BackgroundBookingService] Взяли в работу ID: {idBook}", pendingBookId);
                    booking.Status = BookingStatusEnum.Confirmed;
                    booking.ProcessedAt = DateTime.UtcNow;
                    await bookingRepository.UpdateAsync(booking, stoppingToken);
                    _logger.LogInformation("[BackgroundBookingService] Статус брони {idBook} изменен на Confirmed в БД", booking.Id);

                    var confirmedEvent = new BookingConfirmedEvent(booking.Id, booking.EventId, booking.UserId, booking.SeatsCount, booking.ProcessedAt.Value);

                    await eventProducer.ProduceBookingConfirmedAsync(confirmedEvent, stoppingToken);
                    _logger.LogInformation("[BackgroundBookingService] Событие BookingConfirmed успешно опубликовано для {idBook}", booking.Id);

                }
                catch (OperationCanceledException)
                {
                    _logger.LogDebug("[BackgroundBookingService] Обработка брони {Id} отменена.", pendingBookId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[BackgroundBookingService] Ошибка при обработке брони {Id}", pendingBookId);

                    if (booking != null && booking.Status == BookingStatusEnum.Pending)
                    {
                        booking.Status = BookingStatusEnum.Rejected;
                        booking.ProcessedAt = DateTime.UtcNow;
                        await bookingRepository.UpdateAsync(booking, stoppingToken);
                    }

                    throw;
                }
            }
        }
    }
}
