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

namespace AuthNMethodsTesting
{
    public class AuthenticationMethodsOperations
    {
        private Beta.GraphServiceClient _graphServiceClient;

        public AuthenticationMethodsOperations(Beta.GraphServiceClient graphServiceClient)
        {
            this._graphServiceClient = graphServiceClient;
        }

        public async Task GetUsersAuthenticationMethodsAsync()
        {
            var requestUrl = "https://graph.microsoft.com/beta/me/authentication/methods";
            HttpHelper httpHelper = new HttpHelper(new ColorConsoleLogger());
            HttpClient httpClient = await _graphServiceClient.GetHttpClientForMSGraphAsync();

            HttpResponseMessage rawResponse = await httpHelper.GetRawHttpResponseAsync(httpClient, async client => await client.GetAsync(requestUrl));

            string jsonresponse = rawResponse.ProcessHttpResponse();

            JObject callresults = JObject.Parse(jsonresponse);
            // get JSON result objects into a list
            IList<JToken> results = callresults["value"].Children().ToList();

            authenticationMethod authenticationMethod = results[0].ToObject<authenticationMethod>();
            ColorConsole.WriteLine(ConsoleColor.Green, $"id-{authenticationMethod.id}, isUsable-{authenticationMethod.isUsable}, phoneNumber-{authenticationMethod.phoneNumber}");
        }

        public async Task GetUsersPhoneMethodsAsync()
        {
            var requestUrl = "https://graph.microsoft.com/beta/me/authentication/phoneMethods";
            HttpHelper httpHelper = new HttpHelper(new ColorConsoleLogger());
            HttpClient httpClient = await _graphServiceClient.GetHttpClientForMSGraphAsync();

            HttpResponseMessage rawResponse = await httpHelper.GetRawHttpResponseAsync(httpClient, async client => await client.GetAsync(requestUrl));

            string jsonresponse = rawResponse.ProcessHttpResponse();

            JObject callresults = JObject.Parse(jsonresponse);
            // get JSON result objects into a list
            IList<JToken> results = callresults["value"].Children().ToList();

            phoneAuthenticationMethod phoneMethod = results[0].ToObject<phoneAuthenticationMethod>();
            ColorConsole.WriteLine(ConsoleColor.Green, $"phoneType-{phoneMethod.phoneType}, phoneNumber-{phoneMethod.phoneNumber}, smsSignInState-{phoneMethod.smsSignInState}");
        }
    }
}
