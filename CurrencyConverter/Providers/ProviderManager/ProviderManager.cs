using CurrencyConverter.Providers.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CurrencyConverter.Providers.ProviderManager
{
    public class ProviderManager : IProviderManager
    {
        public List<ICurrencyConverterProvider> Providers { get; set; }

        public virtual IEnumerable<T> GetProviders<T>() where T : ICurrencyConverterProvider
        {
            if (Providers == null)
                throw new InvalidOperationException($"{nameof(Providers)} is NULL.");

            return Providers.OfType<T>().ToList();
        }

        public virtual ICurrencyConverterProvider GetNextSuitableProvider()
        {
            int currentIndex = 0;
            var nextIndex = currentIndex + 1;

            for (; ; currentIndex++, nextIndex++)
            {
                if (Providers.ElementAtOrDefault(nextIndex) != null && Providers[currentIndex] is AbstractConverterProvider abstractConverterProvider)
                {
                    abstractConverterProvider.ChildProvider = Providers[nextIndex];
                }

                else
                    break;
            }

            return Providers.First();
        }

        public virtual void ValidateProviders()
        {
            if (Providers == null || !Providers.Any())
                throw new InvalidOperationException($"{nameof(Providers)} is empty.");

            if (Providers.All(p => p is ICurrencyCacheConverterProvider))
                throw new InvalidOperationException(
                    $"{nameof(Providers)} should contains at least one provider besides {nameof(ICurrencyCacheConverterProvider)}");
        }

        public void ClearProviders()
        {
            Providers = new List<ICurrencyConverterProvider>();
        }

        public void DeleteProviders<T>() where T : ICurrencyConverterProvider =>
            Providers = Providers.Where(x => x.GetType().FullName == typeof(T).FullName).ToList();

        public void AddProvider(ICurrencyConverterProvider provider) => Providers.Add(provider);

        public void AddProvider(ICurrencyConverterProvider provider, int index) => Providers.Insert(index, provider);

    }
}
