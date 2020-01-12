using CurrencyConverter.Enums;
using CurrencyConverter.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CurrencyConverter.Providers.Abstract
{
    /// <inheritdoc/>
    public abstract class AbstractConverterProvider : ICurrencyConverterProvider
    {
        #region Properties

        /// <summary>
        /// Gets or sets child provider. Participates in building a dependency chain.
        /// Don't need to set manually if you use AbstractConverterProvider as a platform for conversion.
        /// </summary>
        public ICurrencyConverterProvider ChildProvider { get; set; }

        /// <summary>
        /// Gets or sets cache converter provider.
        /// </summary>
        public ICurrencyCacheConverterProvider CacheConverterProvider { get; }

        #endregion

        protected AbstractConverterProvider(ICurrencyCacheConverterProvider cacheConverterProvider = null)
        {
            CacheConverterProvider = cacheConverterProvider;
        }

        #region Public Methods

        /// <inheritdoc/>
        public ConvertRateResult GetRates(CurrencyType baseCurrency, IEnumerable<CurrencyType> destinationCurrencies)
        {
            var currencyTypes = destinationCurrencies as CurrencyType[] ?? destinationCurrencies.ToArray();

            return GetResult(baseCurrency,
                currencyTypes,
                GetCachedResult,
                Get,
                GetNextConverterResult);
        }

        /// <summary>
        /// Gets possible rates that provider represents.
        /// </summary>
        /// <param name="baseCurrency">Base currency.</param>
        /// <returns>Conversion result.</returns>
        public abstract ConvertRateResult Get(CurrencyType baseCurrency);

        #endregion

        #region Protected Methods

        /// <summary>
        /// Save positive result to cache.
        /// </summary>
        /// <param name="result">The result that should be cached.</param>
        protected virtual void SaveResultToCache(ConvertRateResult result)
        {
            if (result == null || !result.IsSuccess || CacheConverterProvider == null)
                return;

            CacheConverterProvider.AddOrUpdate(new[] { result });
        }

        /// <summary>
        /// Gets rates result from cache.
        /// </summary>
        /// <param name="baseCurrency">The base currency.</param>
        /// <returns>Rate result instance.</returns>
        protected virtual ConvertRateResult GetCachedResult(CurrencyType baseCurrency, IEnumerable<CurrencyType> destinationCurrencies)
        {
            if (CacheConverterProvider == null)
                return GetDefaultErrorConvertRateResult(new InvalidOperationException($"{nameof(CacheConverterProvider)} is not initialized."), baseCurrency);

            try
            {
                return CacheConverterProvider.GetRates(baseCurrency, destinationCurrencies);
            }
            catch (Exception e)
            {
                return GetDefaultErrorConvertRateResult(e, baseCurrency);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="baseCurrency"></param>
        /// <returns></returns>
        protected virtual ConvertRateResult GetDefaultErrorConvertRateResult(Exception e, CurrencyType baseCurrency)
        {
            var rateResult = GetDefaultConvertRateResult(baseCurrency);
            rateResult.IsSuccess = false;

            rateResult.WriteError(this, e.Message);

            return rateResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseCurrency"></param>
        /// <returns></returns>
        protected virtual ConvertRateResult GetDefaultConvertRateResult(CurrencyType baseCurrency)
        {
            return new ConvertRateResult()
            {
                BaseCurrency = baseCurrency,
                DateOfConversion = DateTime.Now.Date,
                IsFromCache = this is ICurrencyCacheConverterProvider
            };
        }

        /// <summary>
        /// Transforms current rate of inner currency to any currency.
        /// </summary>
        /// <param name="fromType">Current base currency of the rates.</param>
        /// <param name="toType">Currency to convert rates.</param>
        /// <param name="sourceRates">Rates that should be transformed.</param>
        /// <returns>New transformed collection of rates.</returns>
        protected virtual Dictionary<CurrencyType, decimal> TransformRatesToCurrency(CurrencyType fromType,
            CurrencyType toType, Dictionary<CurrencyType, decimal> sourceRates)
        {
            if (fromType == toType)
                return new Dictionary<CurrencyType, decimal>(sourceRates);

            if (!sourceRates.ContainsKey(toType))
                return new Dictionary<CurrencyType, decimal>();

            var toTypeRate = sourceRates[toType];
            var newDictionaryRates = sourceRates.Where(r => r.Key != toType).ToDictionary(k => k.Key, v => v.Value / toTypeRate);
            newDictionaryRates.Add(fromType, 1 / toTypeRate);

            return newDictionaryRates;
        }

        /// <summary>
        /// Gets result from child provider if exists one. 
        /// </summary>
        /// <param name="previousResult">Previous unsuccessful result.</param>
        /// <param name="baseCurrency">Base currency.</param>
        /// <returns>New ConvertRateResult instance.</returns>
        protected virtual ConvertRateResult GetNextConverterResult(ConvertRateResult previousResult, CurrencyType baseCurrency, ICurrencyConverterProvider provider, IEnumerable<CurrencyType> destinationCurrencies)
        {
            if (provider == null)
                return previousResult;

            var newResult = provider.GetRates(baseCurrency, destinationCurrencies);
            newResult.MergeResults(previousResult);

            return newResult;
        }

        /// <summary>
        /// Gets full result.
        /// </summary>
        /// <param name="baseCurrency"></param>
        /// <param name="currencyTypes"></param>
        /// <param name="getCachedResult"></param>
        /// <param name="getMain"></param>
        /// <param name="getChildResult"></param>
        /// <returns></returns>
        protected virtual ConvertRateResult GetResult(
            CurrencyType baseCurrency,
            IEnumerable<CurrencyType> currencyTypes,
            Func<CurrencyType, IEnumerable<CurrencyType>, ConvertRateResult> getCachedResult,
            Func<CurrencyType, ConvertRateResult> getMain,
            Func<ConvertRateResult, CurrencyType, ICurrencyConverterProvider, IEnumerable<CurrencyType>, ConvertRateResult> getChildResult)
        {
            var cachedResult = getCachedResult(baseCurrency, currencyTypes);
            if (ResultIsCompleted(currencyTypes, cachedResult))
                return cachedResult;

            try
            {
                var nextResult = getMain(baseCurrency);
                if (nextResult.IsSuccess)
                    SaveResultToCache(nextResult);

                nextResult.MergeResults(cachedResult);

                return ResultIsCompleted(currencyTypes, nextResult)
                    ? nextResult
                    : getChildResult(nextResult, baseCurrency, ChildProvider, currencyTypes);
            }
            catch (Exception e)
            {
                var defaultErrorResult = GetDefaultErrorConvertRateResult(e, baseCurrency);
                defaultErrorResult.MergeResults(cachedResult);

                return getChildResult(defaultErrorResult, baseCurrency, ChildProvider, currencyTypes);
            }
        }

        private bool ResultIsCompleted(IEnumerable<CurrencyType> currencyTypes, ConvertRateResult result) =>
            result.CheckResultCompleted(currencyTypes) && result.IsSuccess;

        #endregion
    }
}
