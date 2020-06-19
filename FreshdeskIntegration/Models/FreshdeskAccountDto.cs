using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FreshdeskIntegration.Models
{
    public partial class FreshdeskAccounts
    {
        [JsonProperty("results")]
        public List<FreshdeskAccountDto> Results { get; set; } = new List<FreshdeskAccountDto>();

        [JsonProperty("total")]
        public long Total { get; set; }
    }
    public partial class FreshdeskAccountDto
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("note")]
        public string Note { get; set; }

        [JsonProperty("account_tier")]
        public string AccountTier { get; set; }

        [JsonProperty("industry")]
        public string Industry { get; set; }
    }
}
