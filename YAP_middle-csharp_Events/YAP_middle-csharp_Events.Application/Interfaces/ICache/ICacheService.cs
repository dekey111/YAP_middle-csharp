using System;
using System.Collections.Generic;
using System.Text;

namespace YAP_middle_csharp_Events.Application.Interfaces.ICache
{
    public interface ICacheService
    {
        /// <summary>
        /// Метод получения кеша по ключу
        /// </summary>
        /// <param name="key">Ключ кеша</param>
        /// <param name="cancellationToken">Токен отменв</param>
        /// <returns></returns>
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Метод сохранения значения в кеше
        /// </summary>
        /// <param name="key">Ключ по которому мы сохраняем</param>
        /// <param name="value">ЧТо мы сохраняем</param>
        /// <param name="ttl">Сколько времени будет закешировано</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns></returns>
        Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default);


        /// <summary>
        /// Метод удаления значения из кеша
        /// </summary>
        /// <param name="key">Ключ кеша</param>
        /// <param name="cancellationToken">Отмена задачи</param>
        /// <returns></returns>
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    }
}
