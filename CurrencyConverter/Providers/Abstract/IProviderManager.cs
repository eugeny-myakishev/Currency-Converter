using System.Collections.Generic;

namespace CurrencyConverter.Providers.Abstract
{
    /// <summary>
    /// Interface of provider manager.
    /// </summary>
    public interface IProviderManager
    {
        /// <summary>
        /// Gets currency provider.
        /// </summary>
        /// <typeparam name="T">Currency provider.</typeparam>
        /// <returns>Collection of providers.</returns>
        IEnumerable<T> GetProviders<T>() where T : ICurrencyConverterProvider;
        ICurrencyConverterProvider GetNextSuitableProvider();
        void ValidateProviders();
        void ClearProviders();
        void DeleteProviders<T>() where T : ICurrencyConverterProvider;
        void AddProvider(ICurrencyConverterProvider provider);
        void AddProvider(ICurrencyConverterProvider provider, int index);
    }
}
