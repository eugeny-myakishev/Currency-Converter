using CurrencyConverter.Enums;
using CurrencyConverter.Models;
using CurrencyConverter.Providers.Abstract;
using CurrencyConverter.Providers.MemoryStreamCacheConverterProvider.Models;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CurrencyConverter.Providers.MemoryStreamCacheConverterProvider
{
    public class MemoryCacheConverterProvider : AbstractConverterProvider, ICurrencyCacheConverterProvider
    {
        private static IMemoryCache MemoryCache { get; }

        public MemoryCacheOption MemoryCacheOption { get; }

        static MemoryCacheConverterProvider()
        {
            MemoryCache = new MemoryCache(new MemoryCacheOptions()
            {
                ExpirationScanFrequency = new TimeSpan(0, 0, 0, 1)
            });
        }

        public MemoryCacheConverterProvider()
        {
            MemoryCacheOption = new MemoryCacheOption()
            {
                ExpirationCache = new TimeSpan(12, 0, 0)
            };
        }

        public MemoryCacheConverterProvider(Action<MemoryCacheOption> option) : this()
        {
            option?.Invoke(MemoryCacheOption);
        }

        public override ConvertRateResult Get(CurrencyType baseCurrency)
        {
            var rateResult = GetDefaultConvertRateResult(baseCurrency);
            rateResult.IsSuccess = true;
            rateResult.Result = new Dictionary<CurrencyType, decimal>(GetCachedRates(baseCurrency));

            return rateResult;
        }

        public void Clear() => MemoryCache.Set(MemoryCacheOption.MemoryCacheKey, new Dictionary<CurrencyType, decimal>(), TimeSpan.Zero);

        public void AddOrUpdate(IEnumerable<ConvertRateResult> items)
        {
            var newRates = GetNewRatesFromResult(items);
            var cachedRates = GetRatesFromMemory();
            if (cachedRates == null || !cachedRates.Any())
            {
                CacheRates(newRates);

                return;
            }

            foreach (var newRate in newRates)
            {
                if (!cachedRates.ContainsKey(newRate.Key))
                    cachedRates.Add(newRate.Key, newRate.Value);
            }

            CacheRates(cachedRates);
        }

        private void CacheRates(Dictionary<CurrencyType, decimal> rates)
        {
            MemoryCache.Set(MemoryCacheOption.MemoryCacheKey, rates, MemoryCacheOption.ExpirationCache);
        }

        private Dictionary<CurrencyType, decimal> GetRatesFromMemory() =>
            MemoryCache.Get<Dictionary<CurrencyType, decimal>>(MemoryCacheOption.MemoryCacheKey);

        private Dictionary<CurrencyType, decimal> GetCachedRates(CurrencyType baseCurrency)
        {
            var rates = GetRatesFromMemory();

            return GetTransformedCachedRates(rates, baseCurrency);
        }

        private Dictionary<CurrencyType, decimal> GetTransformedCachedRates(
            Dictionary<CurrencyType, decimal> cachedRates, CurrencyType baseCurrency)
        {
            if (cachedRates == null || !cachedRates.Any())
                throw new InvalidOperationException($"{nameof(cachedRates)} is empty or memory cache was cleared.");

            var transformRatesResult = TransformRatesToCurrency(MemoryCacheOption.BaseCachedType, baseCurrency, cachedRates);
            if (transformRatesResult == null || !transformRatesResult.Any())
                throw new InvalidOperationException($"The currency {baseCurrency.ToString()} was not found in cache.");

            return transformRatesResult;
        }

        private Dictionary<CurrencyType, decimal> GetNewRatesFromResult(IEnumerable<ConvertRateResult> results)
        {
            var newRates = new Dictionary<CurrencyType, decimal>();

            foreach (var convertRateResult in results)
            {
                var currencyRates = convertRateResult.BaseCurrency == MemoryCacheOption.BaseCachedType
                    ? convertRateResult.Result
                    : TransformRatesToCurrency(convertRateResult.BaseCurrency, MemoryCacheOption.BaseCachedType,
                        convertRateResult.Result);

                foreach (var result in currencyRates.Where(result => !newRates.ContainsKey(result.Key)))
                    newRates.Add(result.Key, result.Value);
            }

            return newRates;
        }
    }
}
