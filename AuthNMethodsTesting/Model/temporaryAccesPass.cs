using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AuthNMethodsTesting.Model
{
    public class temporaryAccesPass
    {
        [JsonProperty("@odata.context")]
        public string OdataContext { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("temporaryAccessPass")]
        public string TemporaryAccessPass { get; set; }

        [JsonProperty("createdDateTime")]
        public DateTime CreatedDateTime { get; set; }

        [JsonProperty("startDateTime")]
        public DateTime StartDateTime { get; set; }

        [JsonProperty("lifetimeInMinutes")]
        public int LifetimeInMinutes { get; set; }

        [JsonProperty("isUsableOnce")]
        public bool isUsableOnce { get; set; } = true;
    }
}
