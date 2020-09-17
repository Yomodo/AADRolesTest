using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AuthNMethodsTesting.Model
{
    public class IncludeTarget
    {
        [JsonProperty("targetType")]
        public string TargetType { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("isRegistrationRequired")]
        public bool IsRegistrationRequired { get; set; }

        [JsonProperty("useForSignIn")]
        public bool UseForSignIn { get; set; }
    }
}
