using CurrencyConverter.Enums;
using System;
using System.Collections.Generic;

namespace CurrencyConverter.Providers.MemoryStreamCacheConverterProvider.Models
{
    [Serializable]
    public class MemoryCacheModel
    {
        public DateTime CreationDate { get; set; }
        public Dictionary<CurrencyType, decimal> Rates { get; set; } = new Dictionary<CurrencyType, decimal>();
        public CurrencyType BaseCurrency { get; set; }
    }
}
