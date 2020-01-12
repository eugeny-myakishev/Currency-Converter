using CurrencyConverter.Enums;
using CurrencyConverter.Models;
using CurrencyConverter.Providers.Abstract;
using CurrencyConverter.Providers.EuropeanCentralBankProvider.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Xml.Serialization;

namespace CurrencyConverter.Providers.EuropeanCentralBankProvider
{
    public class EuropeanCentralBankProvider : AbstractConverterProvider
    {
        public string Url { get; set; } = "https://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml";

        public EuropeanCentralBankProvider(ICurrencyCacheConverterProvider cacheConverterProvider = null) : base(cacheConverterProvider)
        {

        }

        public override ConvertRateResult Get(CurrencyType baseCurrency)
        {
            var envelop = GetEnvelop();
            ValidateEnvelop(envelop);

            return MapToConvertRateResult(envelop, baseCurrency);
        }

        protected virtual Envelope GetEnvelop()
        {
            using (var httpClient = new HttpClient())
            {
                var stringResponse = httpClient.GetStringAsync(Url);
                stringResponse.Wait();

                return DeserializeResult(stringResponse.Result);
            }
        }

        private ConvertRateResult MapToConvertRateResult(Envelope envelop, CurrencyType baseCurrency)
        {
            var rootCube = envelop.Cube.Cubes.FirstOrDefault();

            var result = GetBaseRateResult(rootCube, baseCurrency);

            var currencyRateResult = TransformRatesToCurrency(CurrencyType.EUR, baseCurrency,
                rootCube.Cubes.ToDictionary(k => ParseEnum<CurrencyType>(k.Currency), v => decimal.Parse(v.Rate, CultureInfo.InvariantCulture)));

            if (!currencyRateResult.Any())
                throw new InvalidOperationException($"{nameof(TransformRatesToCurrency)} returns empty result.");

            result.Result = new Dictionary<CurrencyType, decimal>(currencyRateResult);

            return result;
        }

        private ConvertRateResult GetBaseRateResult(TimeCube timeCube, CurrencyType baseCurrency)
        {
            if (!DateTime.TryParseExact(timeCube.Time, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                throw new InvalidCastException($"Can't parse extract {timeCube.Time} to format 'yyyy-MM-dd'");

            var rateResult = GetDefaultConvertRateResult(baseCurrency);
            rateResult.DateOfConversion = result;
            rateResult.IsSuccess = true;

            return rateResult;
        }

        private static T ParseEnum<T>(string value) where T : struct
        {
            if (!Enum.TryParse(value, out T result))
                throw new InvalidCastException($"Can't convert {value} to {typeof(T).FullName}");

            return result;
        }

        private void ValidateEnvelop(Envelope envelop)
        {
            if (envelop == null)
                throw new ArgumentNullException(nameof(envelop));

            var rootCube = envelop.Cube.Cubes.FirstOrDefault();
            if (rootCube?.Cubes == null || !rootCube.Cubes.Any())
                throw new InvalidOperationException($"Not valid result from {Url}");
        }

        private Envelope DeserializeResult(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Envelope));
            using (StringReader rdr = new StringReader(xml))
            {
                var envelop = (Envelope)serializer.Deserialize(rdr);
                if (envelop == null)
                    throw new InvalidCastException($"Can't deserialize result {xml} to {nameof(Envelope)}");

                return envelop;
            }
        }
    }
}
