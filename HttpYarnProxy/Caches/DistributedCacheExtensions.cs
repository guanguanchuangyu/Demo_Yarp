/// <summary>
/// 缓存扩展
/// </summary>
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace HttpYarnProxy.Caches
{
    /// <summary>
    /// 分布式缓存扩展类
    /// </summary>
    public static class DistributedCacheExtensions
    {
        // 默认的 JSON 序列化选项
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        /// <summary>
        /// 将对象存储到缓存中
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="cache">分布式缓存实例</param>
        /// <param name="key">缓存键</param>
        /// <param name="value">要缓存的对象</param>
        /// <param name="options">缓存选项</param>
        /// <param name="cancellationToken">取消令牌</param>
        public static async Task SetAsync<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions options = null, CancellationToken cancellationToken = default)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            // 将对象序列化为 JSON 字符串
            var json = JsonSerializer.Serialize(value, _jsonSerializerOptions);
            var bytes = Encoding.UTF8.GetBytes(json);

            // 存储到缓存中
            await cache.SetAsync(key, bytes, options ?? new DistributedCacheEntryOptions(), cancellationToken);
        }

        /// <summary>
        /// 从缓存中获取对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="cache">分布式缓存实例</param>
        /// <param name="key">缓存键</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>反序列化后的对象，如果缓存中不存在则返回默认值</returns>
        public static async Task<T> GetAsync<T>(this IDistributedCache cache, string key, CancellationToken cancellationToken = default)
        {
            // 从缓存中获取字节数组
            var bytes = await cache.GetAsync(key, cancellationToken);
            if (bytes == null || bytes.Length == 0)
            {
                return default;
            }

            // 将字节数组反序列化为对象
            var json = Encoding.UTF8.GetString(bytes);
            return JsonSerializer.Deserialize<T>(json, _jsonSerializerOptions);
        }

        /// <summary>
        /// 从缓存中获取对象，如果不存在则通过工厂方法生成并缓存
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="cache">分布式缓存实例</param>
        /// <param name="key">缓存键</param>
        /// <param name="factory">对象生成工厂方法</param>
        /// <param name="options">缓存选项</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>缓存或生成的对象</returns>
        public static async Task<T> GetOrCreateAsync<T>(this IDistributedCache cache, string key, Func<Task<T>> factory, DistributedCacheEntryOptions options = null, CancellationToken cancellationToken = default)
        {
            // 尝试从缓存中获取对象
            var cachedValue = await cache.GetAsync<T>(key, cancellationToken);
            if (cachedValue != null)
            {
                return cachedValue;
            }

            // 如果缓存中不存在，则调用工厂方法生成对象
            var value = await factory();
            if (value != null)
            {
                // 将生成的对象存储到缓存中
                await cache.SetAsync(key, value, options, cancellationToken);
            }

            return value;
        }
    }
}
