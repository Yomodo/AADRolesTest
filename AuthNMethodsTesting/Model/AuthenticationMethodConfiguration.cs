using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AuthNMethodsTesting.Model
{
    public class AuthenticationMethodConfiguration
    {
        [JsonProperty("@odata.context")]
        public string OdataContext { get; set; }

        [JsonProperty("@odata.type")]
        public string OdataType { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        
    }
}
