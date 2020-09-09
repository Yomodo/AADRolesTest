extern alias BetaLib;

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using AuthNMethodsTesting.Model;
using System.Linq;
using System.Net.Http;
using Beta = BetaLib.Microsoft.Graph;
using System.Threading.Tasks;
using Common;
using Newtonsoft.Json;

namespace AuthNMethodsTesting
{
    public class DeviceRegistrationPolicySettingsOperations
    {
        private Beta.GraphServiceClient _graphServiceClient;

        public DeviceRegistrationPolicySettingsOperations(Beta.GraphServiceClient graphServiceClient)
        {
            this._graphServiceClient = graphServiceClient;
        }

        public async Task<deviceRegistrationPolicy> GetDeviceRegistrationPolicyAsync()
        {
            var requestUrl = "https://graph.microsoft.com/beta/policies/deviceRegistrationPolicy";
            HttpHelper httpHelper = new HttpHelper(new ColorConsoleLogger());
            HttpClient httpClient = await _graphServiceClient.GetHttpClientForMSGraphAsync();

            HttpResponseMessage rawResponse = await httpHelper.GetRawHttpResponseAsync(httpClient, async client => await client.GetAsync(requestUrl));

            string jsonresponse = rawResponse.ProcessHttpResponse();

            deviceRegistrationPolicy policy = JsonConvert.DeserializeObject<deviceRegistrationPolicy>(jsonresponse);
            return policy;
        }

        public string PrintDeviceRegistrationPolicy(deviceRegistrationPolicy policy, bool verbose = false)
        {
            string toPrint = string.Empty;
            StringBuilder more = new StringBuilder();


            if (policy != null)
            {
                Console.WriteLine($"Printing Device registration policy '{policy.displayName}'");

                toPrint = $"DisplayName-'{policy.displayName}', State-[{policy.description}]";

                more.AppendLine($"userDeviceQuota-{policy.userDeviceQuota}, multiFactorAuthConfiguration- {policy.multiFactorAuthConfiguration} ");

                if(policy.azureADJoin != null)
                {
                    more.AppendLine("");
                    more.AppendLine($"\tisAdminConfigurable-{policy.azureADJoin.isAdminConfigurable}, appliesTo- {policy.azureADJoin.appliesTo} ");

                    //TODO users and groups
                }

                if (policy.azureADRegistration != null)
                {
                    more.AppendLine("");
                    more.AppendLine($"\tisAdminConfigurable-{policy.azureADRegistration.isAdminConfigurable}, appliesTo- {policy.azureADRegistration.appliesTo} ");

                    //TODO users and groups
                }
            }
            else
            {
                ColorConsole.WriteLine(ConsoleColor.Red, $"The provided Device registration policy is null");
            }

            return toPrint + more.ToString();
        }
    }
}
