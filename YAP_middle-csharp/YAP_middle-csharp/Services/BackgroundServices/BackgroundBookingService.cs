using YAP_middle_csharp.Interfaces.IRepositories;
using YAP_middle_csharp.Interfaces.IServices;
using YAP_middle_csharp.Models;

namespace YAP_middle_csharp.Services.BackgroundServices
{
    /// <summary>
    /// Фоновый метод для обработки поступающих заказов
    /// </summary>
    /// <param name="serviceProvider">Принимает провайдер, чтобы найти BookingRepository</param>
    /// <param name="logger">Принимает логгер</param>
    public class BackgroundBookingService(
        IServiceProvider serviceProvider,
        ILogger<BackgroundBookingService> logger) : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILogger<BackgroundBookingService> _logger = logger;
        private readonly SemaphoreSlim _processingSemaphore = new(1, 1);


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
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var repository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
                        var pendingBooks = (await repository.FindPendingBookingsAsync()).ToList();
                        if(pendingBooks.Any())
                        {
                            var tasks = pendingBooks.Select(booking => ProcessBookingAsync(booking, stoppingToken));
                            await Task.WhenAll(tasks);
                        }
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
        private async Task ProcessBookingAsync(BookingModel booking, CancellationToken stoppingToken)
        {
            await Task.Delay(2000, stoppingToken);
            using (var scope = _serviceProvider.CreateScope())
            {
                var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
                var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

                await _processingSemaphore.WaitAsync(stoppingToken);
                EventModel? findEvent = null;

                try
                {
                    _logger.LogInformation("[BackgroundBookingService] Взяли в работу ID: {idBook}", booking.Id);
                    findEvent = await eventService.FindByIdAsync(booking.EventId);
                    if (findEvent == null)
                    {
                        _logger.LogWarning("[BackgroundBookingService] Событие {EventId} не найдено для брони {BookingId}. Отклонение.", booking.EventId, booking.Id);

                        booking.Status = BookingStatusEnum.Rejected;
                        booking.ProcessedAt = DateTime.UtcNow;

                        await bookingRepository.UpdateAsync(booking);
                        return;
                    }

                    booking.Status = BookingStatusEnum.Confirmed;
                    booking.ProcessedAt = DateTime.UtcNow;

                    await bookingRepository.UpdateAsync(booking);
                    _logger.LogInformation("[BackgroundBookingService] Обработали ID: {idBook}", booking.Id);

                }
                catch (OperationCanceledException)
                {
                    _logger.LogDebug("[BackgroundBookingService] Обработка брони {Id} отменена.", booking.Id);
                }
                catch
                {
                    booking.Status = BookingStatusEnum.Rejected;
                    booking.ProcessedAt = DateTime.UtcNow;
                    await bookingRepository.UpdateAsync(booking);

                    if (findEvent == null)
                    {
                        findEvent = await eventService.FindByIdAsync(booking.EventId);
                    }

                    if (findEvent != null)
                    {
                        findEvent.ReleaseSeats(1);
                        await eventService.UpdateAsync(findEvent);
                    }
                    throw;
                }
                finally
                {
                    _processingSemaphore.Release();
                }
            }
        }
    }
}
