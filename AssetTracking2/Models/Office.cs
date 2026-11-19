using System;
using System.Collections.Generic;
using System.Text;

namespace AssetTracking2.Models
{
    public sealed class Office
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Country { get; set; } = "";
        public string Currency { get; set; } = "";
        public string CurrencySymbol { get; set; } = "";
        public decimal UsdExchangeRate { get; set; }
        public List<Asset> Assets { get; set; } = new();
    }

}
