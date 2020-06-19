using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreshdeskIntegration.Models
{
    public class FreshdeskAccountForCreation
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("note")]
        public string Note { get; set; }

        //[JsonProperty("account_tier")]
        //public string AccountTier { get; set; }

        //[JsonProperty("industry")]
        //public string Industry { get; set; }
    }
}
