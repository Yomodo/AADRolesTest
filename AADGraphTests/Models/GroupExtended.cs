extern alias BetaLib;

using Newtonsoft.Json;
using Beta = BetaLib.Microsoft.Graph;

namespace AppRolesTesting
{
    public class GroupExtended : Beta.Group
    {
        [JsonProperty("owners@odata.bind", NullValueHandling = NullValueHandling.Ignore)]
        public string[] OwnersReference { get; set; }

        [JsonProperty("members@odata.bind", NullValueHandling = NullValueHandling.Ignore)]
        public string[] MembersReference { get; set; }
    }
}