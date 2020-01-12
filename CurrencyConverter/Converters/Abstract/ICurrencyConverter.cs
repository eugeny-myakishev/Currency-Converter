using CurrencyConverter.Enums;
using CurrencyConverter.Models;
using System.Collections.Generic;

namespace CurrencyConverter.Converters.Abstract
{
    /// <summary>
    /// The interface of currency converter.
    /// </summary>
    public interface ICurrencyConverter
    {
        /// <summary>
        /// Gets rates by base currency.
        /// </summary>
        /// <param name="baseCurrency">The base currency.</param>
        /// <param name="destinationCurrencies">These are the currencies whose results must be returned</param>
        /// <returns>Result of ConvertRateResult instance.</returns>
        ConvertRateResult GetRates(CurrencyType baseCurrency, IEnumerable<CurrencyType> destinationCurrencies);
    }
}
