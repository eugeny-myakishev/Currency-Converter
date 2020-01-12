using System;
using System.Collections.Generic;
using CurrencyConverter.Models;

namespace CurrencyConverter.Providers.Abstract
{
    /// <summary>
    /// Interface for caching results. Extends ICurrencyConverterProvider.
    /// </summary>
    public interface ICurrencyCacheConverterProvider : ICurrencyConverterProvider
    {
        /// <summary>
        /// Clear cache manually.
        /// </summary>
        void Clear();

        /// <summary>
        /// Adds or updates new results from providers.
        /// </summary>
        /// <param name="items"></param>
        void AddOrUpdate(IEnumerable<ConvertRateResult> items);
    }
}
