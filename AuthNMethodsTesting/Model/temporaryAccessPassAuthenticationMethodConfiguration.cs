using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AuthNMethodsTesting.Model
{
    public class temporaryAccessPassAuthenticationMethodConfiguration : AuthenticationMethodConfiguration
    {
        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("defaultLifetimeInMinutes")]
        public int DefaultLifetimeInMinutes { get; set; }

        [JsonProperty("defaultLength")]
        public int DefaultLength { get; set; }

        [JsonProperty("minimumLifetimeInMinutes")]
        public int MinimumLifetimeInMinutes { get; set; }

        [JsonProperty("maximumLifetimeInMinutes")]
        public int MaximumLifetimeInMinutes { get; set; }

        [JsonProperty("isUsableOnce")]
        public bool IsUsableOnce { get; set; }

        [JsonProperty("includeTargets@odata.context")]
        public string IncludeTargetsOdataContext { get; set; }

        [JsonProperty("includeTargets")]
        public List<IncludeTarget> IncludeTargets { get; set; }
    }
}
