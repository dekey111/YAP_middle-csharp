using YAP_middle_csharp.Interfaces.IRepositories;
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
                        var pendingBook = (await repository.FindPendingBookings()).ToList();

                        foreach (var book in pendingBook)
                        {
                            _logger.LogInformation("[BackgroundBookingService] Взяли в работу ID: {idBook}", book.Id);

                            await Task.Delay(2000);

                            book.Status = BookingStatusEnum.Confirmed;
                            book.ProcessedAt = DateTime.UtcNow;
                            await repository.Update(book);
                            _logger.LogInformation("[BackgroundBookingService] Обработали ID: {idBook}", book.Id);

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
    }
}
