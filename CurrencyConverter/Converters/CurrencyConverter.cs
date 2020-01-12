using CurrencyConverter.Converters.Abstract;
using CurrencyConverter.Enums;
using CurrencyConverter.Models;
using CurrencyConverter.Providers.Abstract;
using CurrencyConverter.Providers.EuropeanCentralBankProvider;
using CurrencyConverter.Providers.MemoryStreamCacheConverterProvider;
using CurrencyConverter.Providers.ProviderManager;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CurrencyConverter.Converters
{
    /// <inheritdoc/>
    public class CurrencyConverter : ICurrencyConverter
    {
        #region Properties

        /// <summary>
        /// Gets provider manager.
        /// </summary>
        public IProviderManager ProviderManager { get; }

        #endregion

        #region Ctors

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrencyConverter"/> class.
        /// </summary>
        public CurrencyConverter()
        {
            ProviderManager = new ProviderManager()
            {
                Providers = new List<ICurrencyConverterProvider>()
                {
                    new EuropeanCentralBankProvider(new MemoryCacheConverterProvider())
                }
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrencyConverter"/> class.
        /// </summary>
        public CurrencyConverter(Action<IProviderManager> options) : this()
        {
            options?.Invoke(ProviderManager);
        }

        #endregion

        #region Public Methods

        /// <inheritdoc/>
        public ConvertRateResult GetRates(CurrencyType baseCurrency, IEnumerable<CurrencyType> destinationCurrencies)
        {
            Validate();

            var result = GetRatesFromProvider(baseCurrency, destinationCurrencies);

            if (!result.IsSuccess)
                return result;

            SplitUnnecessaryResults(result, destinationCurrencies);

            return result;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Gets rates by base currency.
        /// </summary>
        /// <param name="baseCurrency">The base currency.</param>
        /// <param name="destinationCurrencies">These are the currencies whose results must be returned</param>
        /// <returns>Result of ConvertRateResult instance.</returns>
        protected virtual ConvertRateResult GetRatesFromProvider(CurrencyType baseCurrency, IEnumerable<CurrencyType> destinationCurrencies) =>
            ProviderManager.GetNextSuitableProvider().GetRates(baseCurrency, destinationCurrencies);

        /// <summary>
        /// Validates required parameters.
        /// </summary>
        protected virtual void Validate()
        {
            if (ProviderManager == null)
                throw new InvalidOperationException($"{nameof(ProviderManager)} is not initialized.");

            ProviderManager.ValidateProviders();
        }

        #endregion

        #region Private Methods

        private void SplitUnnecessaryResults(ConvertRateResult result, IEnumerable<CurrencyType> destinationCurrencies)
        {
            result.Result = result.Result.Where(x => destinationCurrencies.Contains(x.Key))
                .ToDictionary(x => x.Key, x => x.Value);
        }

        #endregion
    }
}
