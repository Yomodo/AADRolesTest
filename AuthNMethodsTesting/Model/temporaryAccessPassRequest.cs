using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AuthNMethodsTesting.Model
{
    public class temporaryAccessPassRequest
    {
        [JsonProperty("temporaryAccessPass")]
        public int lifetimeInMinutes { get; set; }

        [JsonProperty("isUsableOnce")]
        public bool isUsableOnce { get; set; } = true;
    }
}
