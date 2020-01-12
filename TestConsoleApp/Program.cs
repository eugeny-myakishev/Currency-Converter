using CurrencyConverter.Enums;
using CurrencyConverter.Providers.MemoryStreamCacheConverterProvider;
using System;
using System.Linq;
using System.Threading;
using CurrencyConverter.Providers.Abstract;
using CurrencyConverter.Providers.EuropeanCentralBankProvider;

namespace TestConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var converter = new CurrencyConverter.Converters.CurrencyConverter(manager =>
            {
                manager.ClearProviders();
                manager.AddProvider(new EuropeanCentralBankProvider(
                    new MemoryCacheConverterProvider(option => option.ExpirationCache = new TimeSpan(0, 0, 30))));
            });
            var result = converter.GetRates(CurrencyType.GBP, new[] { CurrencyType.RUB });

            converter = new CurrencyConverter.Converters.CurrencyConverter();

            Thread.Sleep(new TimeSpan(0,0,20));
            result = converter.GetRates(CurrencyType.RUB, new[] { CurrencyType.GBP, CurrencyType.IDR });
            result.RoundDecimalResult(2);

            Console.ReadKey();
        }
    }
}
