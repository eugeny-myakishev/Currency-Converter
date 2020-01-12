using CurrencyConverter.Enums;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace CurrencyConverter.Providers.MemoryStreamCacheConverterProvider.Models
{
    public class MemoryCacheOption
    {
        public string MemoryCacheKey { get; set; } = $"{nameof(MemoryCacheOption)}";
        public CurrencyType BaseCachedType { get; set; } = CurrencyType.EUR;
        public TimeSpan ExpirationCache { get; set; } = new TimeSpan(12, 0, 0);
    }
}
