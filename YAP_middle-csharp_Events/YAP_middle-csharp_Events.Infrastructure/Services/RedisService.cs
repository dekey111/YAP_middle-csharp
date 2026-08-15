using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;
using YAP_middle_csharp_Events.Application.Interfaces.ICache;

namespace YAP_middle_csharp_Events.Infrastructure.Services
{
    public class RedisService(IConnectionMultiplexer redis, ILogger<RedisService> logger) : ICacheService
    {
        private readonly IConnectionMultiplexer _redis = redis;
        private readonly ILogger<RedisService> _logger = logger;

        /// <summary>
        /// Получение базы данных
        /// </summary>
        /// <returns></returns>
        private IDatabase? GetDatabase()
        {
            try
            {
                return _redis.GetDatabase();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении базы данных Redis.");
                return null;
            }
        }


        /// <summary>
        /// Метод получения кеша по ключу
        /// </summary>
        /// <param name="key">Ключ кеша</param>
        /// <param name="cancellationToken">Токен отменв</param>
        /// <returns></returns>
        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var db = GetDatabase();
                if (db == null)
                {
                    return default;
                }

                RedisValue value = await db.StringGetAsync(key);

                if (value.IsNullOrEmpty)
                {
                    return default;
                }

                return JsonSerializer.Deserialize<T>(value.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Не удалось получить значение из кеша Redis по ключу: {Key}", key);
                return default;
            }
        }


        /// <summary>
        /// Метод сохранения значения в кеше
        /// </summary>
        /// <param name="key">Ключ по которому мы сохраняем</param>
        /// <param name="value">ЧТо мы сохраняем</param>
        /// <param name="ttl">Сколько времени будет закешировано</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns></returns>
        public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default)
        {
            try
            {
                var db = GetDatabase();
                if (db == null)
                {
                    return;
                }
                string serializedValue = JsonSerializer.Serialize(value);
                await db.StringSetAsync(key, serializedValue, ttl);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Не удалось записать значение в кеш Redis по ключу: {Key}", key);
            }
        }


        /// <summary>
        /// Метод удаления значения из кеша
        /// </summary>
        /// <param name="key">Ключ кеша</param>
        /// <param name="cancellationToken">Отмена задачи</param>
        /// <returns></returns>
        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var db = GetDatabase();
                if (db == null)
                {
                    return;
                }

                await db.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Не удалось удалить значение из кеша Redis по ключу: {Key}", key);
            }
        }
    }
}
