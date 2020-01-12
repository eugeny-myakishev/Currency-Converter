using System.Collections.Generic;
using CurrencyConverter.Enums;
using CurrencyConverter.Models;

namespace CurrencyConverter.Providers.Abstract
{
    /// <summary>
    /// Interface of converter provider.
    /// </summary>
    public interface ICurrencyConverterProvider
    {
        /// <summary>
        /// Returns all possible rates that provider represents for current currency.
        /// </summary>
        /// <param name="baseCurrency">Base currency.</param>
        /// <param name="destinationCurrencies">Rates that should be found for base currency.</param>
        /// <returns>Result of conversion.</returns>
        ConvertRateResult GetRates(CurrencyType baseCurrency, IEnumerable<CurrencyType> destinationCurrencies);
    }
}
