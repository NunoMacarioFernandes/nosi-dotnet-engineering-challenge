using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace NOS.Engineering.Challenge.Cache
{
    public class MemoryCacheService<T> : ICacheService<T>
    {
        private readonly ConcurrentDictionary<Guid, T> _cache;

        public MemoryCacheService()
        {
            _cache = new ConcurrentDictionary<Guid, T>();
        }

        public Task<T?> GetAsync(Guid id)
        {
            _cache.TryGetValue(id, out var item);
            return Task.FromResult(item);
        }

        public Task SetAsync(Guid id, T item)
        {
            _cache[id] = item;
            return Task.CompletedTask;
        }

        public Task RemoveAsync(Guid id)
        {
            _cache.TryRemove(id, out _);
            return Task.CompletedTask;
        }
    }
}
