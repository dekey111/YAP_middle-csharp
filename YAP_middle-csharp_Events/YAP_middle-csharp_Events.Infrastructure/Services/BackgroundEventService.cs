using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using YAP_middle_csharp.Contracts.BookingModel;
using YAP_middle_csharp.Contracts.EventModels;
using YAP_middle_csharp_Events.Application.Interfaces.IRepositories;

namespace YAP_middle_csharp_Events.Infrastructure.Services
{
    public class BackgroundEventService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BackgroundEventService> _logger;
        private readonly ConsumerConfig _consumerConfig;

        public BackgroundEventService(IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<BackgroundEventService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;

            var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? throw new InvalidOperationException("Kafka:BootstrapServers не настроен");
            var groupId = configuration["Kafka:GroupId"] ?? "events-booking-group";

            _consumerConfig = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true 
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var consumer = new ConsumerBuilder<string, string>(_consumerConfig).Build();

            consumer.Subscribe(EventTopics.BookingConfirmed);
            _logger.LogInformation("[BackgroundEventService] Подписка на топик {Topic} оформлена. Ожидание сообщений...", EventTopics.BookingConfirmed);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(stoppingToken);

                    if (consumeResult?.Message?.Value == null)
                        continue;

                    _logger.LogInformation("[BackgroundEventService] Получено сообщение из Partition {Partition}, Offset {Offset}",   consumeResult.Partition.Value, consumeResult.Offset.Value);

                    await ProcessMessageAsync(consumeResult.Message.Value, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("[BackgroundEventService] Остановка подписчика Kafka");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[BackgroundEventService] Критическая ошибка при чтении из Kafka");
                    await Task.Delay(1000, stoppingToken); 
                }
            }

            consumer.Close();
        }

        private async Task ProcessMessageAsync(string messageJson, CancellationToken cancellationToken)
        {
            BookingConfirmedEvent? bookingConfirmedEvent;

            try
            {
                bookingConfirmedEvent = JsonSerializer.Deserialize<BookingConfirmedEvent>(messageJson);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "[BackgroundEventService] Ошибка десериализации сообщения: {Json}", messageJson);
                return; 
            }

            if (bookingConfirmedEvent == null) return;

            using var scope = _scopeFactory.CreateScope();
            var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();

            try
            {
                var eventModel = await eventRepository.FindByIdAsync(bookingConfirmedEvent.EventId);

                if (eventModel == null)
                {
                    _logger.LogWarning("[BackgroundEventService] Событие {EventId} не найдено для брони {BookingId}. Пропуск.",
                        bookingConfirmedEvent.EventId, bookingConfirmedEvent.BookingId);
                    return;
                }

                if (eventModel.AvailableSeats < bookingConfirmedEvent.SeatsCount)
                {
                    _logger.LogWarning("[BackgroundEventService] Недостаточно мест для события {EventId}. Доступно: {Available}, Запрошено: {Requested}. Пропуск.",
                        bookingConfirmedEvent.EventId, eventModel.AvailableSeats, bookingConfirmedEvent.SeatsCount);
                    return;
                }

                eventModel.AvailableSeats -= bookingConfirmedEvent.SeatsCount;
                await eventRepository.UpdateAsync(eventModel);

                _logger.LogInformation("[BackgroundEventService] Успешно списано {Seats} мест для события {EventId}. Осталось: {Remaining}",
                    bookingConfirmedEvent.SeatsCount, bookingConfirmedEvent.EventId, eventModel.AvailableSeats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[BackgroundEventService] Ошибка при обновлении мест для брони {BookingId}", bookingConfirmedEvent.BookingId);
                throw;
            }
        }
    }
}
