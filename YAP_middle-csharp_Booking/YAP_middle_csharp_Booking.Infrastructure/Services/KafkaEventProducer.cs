using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using YAP_middle_csharp.Contracts.BookingModel;
using YAP_middle_csharp.Contracts.EventModels;
using YAP_middle_csharp_Booking.Application.Interfaces.IServices;

namespace YAP_middle_csharp_Booking.Infrastructure.Services
{
    public class KafkaEventProducer : IKafkaEventProducer, IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<KafkaEventProducer> _logger;

        public KafkaEventProducer(IConfiguration configuration, ILogger<KafkaEventProducer> logger)
        {
            _logger = logger;

            var findBootstrapServers = configuration["Kafka:BootstrapServers"] ?? throw new InvalidOperationException("Конфигурация Kafka не найдена!");
            var config = new ProducerConfig
            {
                BootstrapServers = findBootstrapServers,
                Acks = Acks.All
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
        }


        public async Task ProduceBookingConfirmedAsync(BookingConfirmedEvent bookingConfirmedEvent, CancellationToken cancellationToken = default)
        {
            var jsonMessage = JsonSerializer.Serialize(bookingConfirmedEvent);
            var message = new Message<string, string>
            {
                Key = bookingConfirmedEvent.EventId.ToString(),
                Value = jsonMessage
            };


            try
            {
                var result = await _producer.ProduceAsync(EventTopics.BookingConfirmed, message, cancellationToken);
                _logger.LogInformation("[KafkaEventProducer] Сообщение BookingConfirmed отправлено в топик {Topic}, Partition: {Partition}, Offset: {Offset}",  result.Topic, result.Partition.Value, result.Offset.Value);
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(ex, "[KafkaEventProducer] Ошибка при отправке сообщения BookingConfirmed в Kafka. Reason: {Reason}", ex.Error.Reason);
                throw;
            }

        }


        public void Dispose()
        {
            _producer.Flush(TimeSpan.FromSeconds(10));
            _producer.Dispose();
        }
    }
}
