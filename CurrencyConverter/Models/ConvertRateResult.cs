using CurrencyConverter.Enums;
using CurrencyConverter.Providers.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CurrencyConverter.Models
{
    /// <summary>
    /// Class that represents a result of conversion.
    /// </summary>
    public class ConvertRateResult
    {
        #region Properties

        #region Private Properties

        /// <summary>
        /// Gets errors of conversion.
        /// </summary>
        private Dictionary<string, string> Errors { get; } = new Dictionary<string, string>();

        #endregion

        /// <summary>
        /// Indicates that conversion is succeeded.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Indicates that all rates was received into result.
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// Gets or sets base currency.
        /// </summary>
        public CurrencyType BaseCurrency { get; set; }

        /// <summary>
        /// Gets or sets result of conversion.
        /// </summary>
        public Dictionary<CurrencyType, decimal> Result { get; set; } = new Dictionary<CurrencyType, decimal>();

        /// <summary>
        /// Gets or sets date of conversion.
        /// </summary>
        public DateTime DateOfConversion { get; set; }

        /// <summary>
        /// Indicates that the result was provided by cache provider.
        /// </summary>
        public bool IsFromCache { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Rounds the result.
        /// </summary>
        /// <param name="decimals">The number of characters after the period.</param>
        public void RoundDecimalResult(int decimals = 3) =>
            Result = Result.ToDictionary(x => x.Key, x => decimal.Round(x.Value, decimals));

        /// <summary>
        /// Merges errors.
        /// </summary>
        /// <param name="errors"></param>
        public virtual void MergeErrors(Dictionary<string, string> errors)
        {
            foreach (var error in errors)
            {
                if(!Errors.ContainsKey(error.Key))
                    Errors.Add(error.Key, error.Value);
            }
        }

        /// <summary>
        /// Merges results.
        /// </summary>
        /// <param name="previousResult">Previous results.</param>
        public virtual void MergeResults(ConvertRateResult previousResult)
        {
            MergeErrors(previousResult.Errors);
            foreach (var result in previousResult.Result)
            {
                if (!Result.ContainsKey(result.Key))
                    Result.Add(result.Key, result.Value);
            }
        }

        /// <summary>
        /// AddOrUpdate error to error dictionary.
        /// </summary>
        /// <param name="provider">Provider that throw an error.</param>
        /// <param name="message">String message with description of error.</param>
        public virtual void WriteError(ICurrencyConverterProvider provider, string message)
        {
            var key = provider.GetType().Name;

            if (!Errors.ContainsKey(key))
                Errors.Add(key, message);
        }

        /// <summary>
        /// Checks that result is completed.
        /// </summary>
        /// <param name="currencies">Currencies that should be contains in the result.</param>
        /// <returns>Flag that result is completed.</returns>
        public bool CheckResultCompleted(IEnumerable<CurrencyType> currencies)
        {
            if (currencies.All(r => Result.Any(c => c.Key == r)))
                this.IsCompleted = true;

            return IsCompleted;
        }

        /// <summary>
        /// Gets all errors.
        /// </summary>
        /// <returns>Collection of errors.</returns>
        public Dictionary<string, string> GetErrors() => Errors;

        #endregion
    }
}
