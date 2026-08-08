using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using YAP_middle_csharp.Contracts.EventModels;

namespace YAP_middle_csharp_Events.Infrastructure.Topic
{
    public class KafkaTopicInit(IConfiguration configuration, ILogger<KafkaTopicInit> logger) : IHostedService
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<KafkaTopicInit> _logger = logger;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var bootstrapServers = _configuration["Kafka:BootstrapServers"];
            if (string.IsNullOrEmpty(bootstrapServers))
            {
                _logger.LogWarning("[KafkaTopicInit] BootstrapServers не заданы");
                return;
            }

            var config = new AdminClientConfig { BootstrapServers = bootstrapServers };

            using var adminClient = new AdminClientBuilder(config).Build();

            try
            {
                var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(5));
                var topicExists = metadata.Topics.Any(t => t.Topic == EventTopics.BookingConfirmed);

                if (!topicExists)
                {
                    _logger.LogInformation("[KafkaTopicInit] Топик {Topic} не найден. Создание...", EventTopics.BookingConfirmed);

                    var topicSpec = new TopicSpecification
                    {
                        Name = EventTopics.BookingConfirmed,
                        NumPartitions = 3,
                        ReplicationFactor = 1
                    };

                    await adminClient.CreateTopicsAsync(new[] { topicSpec });
                    _logger.LogInformation("[KafkaTopicInit] Топик {Topic} успешно создан", EventTopics.BookingConfirmed);
                }
                else
                {
                    _logger.LogInformation("[KafkaTopicInit] Топик {Topic} уже существует", EventTopics.BookingConfirmed);
                }
            }
            catch (CreateTopicsException ex) when (ex.Results.Any(r => r.Error.Code == ErrorCode.TopicAlreadyExists))
            {
                _logger.LogInformation("[KafkaTopicInit] Топик {Topic} уже создан другим сервисом", EventTopics.BookingConfirmed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[KafkaTopicInit] Не удалось создать топик {Topic}", EventTopics.BookingConfirmed);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

}
