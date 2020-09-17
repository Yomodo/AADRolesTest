extern alias BetaLib;

using AuthNMethodsTesting.Model;
using Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Beta = BetaLib.Microsoft.Graph;

namespace AuthNMethodsTesting
{
    public class AuthenticationMethodsOperations
    {
        private Beta.GraphServiceClient _graphServiceClient;

        public AuthenticationMethodsOperations(Beta.GraphServiceClient graphServiceClient)
        {
            this._graphServiceClient = graphServiceClient;
        }

        public async Task<temporaryAccessPassAuthenticationMethodConfiguration> GetTemporaryAccessPassConfigurationAsync()
        {
            var requestUrl = "https://graph.microsoft.com/beta/policies/authenticationMethodsPolicy/authenticationMethodConfigurations/TemporaryAccessPass";
            HttpHelper httpHelper = new HttpHelper(new ColorConsoleLogger());
            HttpClient httpClient = await _graphServiceClient.GetHttpClientForMSGraphAsync();

            HttpResponseMessage rawResponse = await httpHelper.GetRawHttpResponseAsync(httpClient, async client => await client.GetAsync(requestUrl));

            string jsonresponse = rawResponse.ProcessHttpResponse();
            temporaryAccessPassAuthenticationMethodConfiguration authenticationMethodConfiguration = JsonConvert.DeserializeObject<temporaryAccessPassAuthenticationMethodConfiguration>(jsonresponse);

            return authenticationMethodConfiguration;
        }

        public async Task<string> UpdateTemporaryAccessPassConfigurationAsync(temporaryAccessPassAuthenticationMethodConfiguration config)
        {
            var requestUrl = "https://graph.microsoft.com/beta/policies/authenticationMethodsPolicy/authenticationMethodConfigurations/TemporaryAccessPass";

            HttpClient httpClient = await _graphServiceClient.GetHttpClientForMSGraphAsync();

            HttpContent content = new StringContent(JsonConvert.SerializeObject(config));
            content.Headers.Add("ContentType", "application/json");
            var postResponse = await httpClient.PatchAsync(requestUrl, content);
            string serverResponse = await postResponse.Content.ReadAsStringAsync();

            return serverResponse;
        }

        public async Task<temporaryAccesPass> GenerateTemporaryAccessPassForUser(Beta.User user, int lifetimeInMinutes)
        {
            var requestUrl = $"https://graph.microsoft.com/beta/users/{user.Id}/authentication/temporaryAccessPassMethods";            
            HttpClient httpClient = await _graphServiceClient.GetHttpClientForMSGraphAsync();

            HttpContent content = new StringContent(JsonConvert.SerializeObject(new temporaryAccessPassRequest() { lifetimeInMinutes = lifetimeInMinutes }));
            content.Headers.Add("ContentType", "application/json");
            HttpResponseMessage rawResponse = await httpClient.PostAsync(requestUrl, content);
            string jsonresponse = rawResponse.ProcessHttpResponse();

            temporaryAccesPass accesPass = JsonConvert.DeserializeObject<temporaryAccesPass>(jsonresponse);

            return accesPass;
        }

        public async Task<temporaryAccesPass> GetExistingTemporaryAccessPassForUser(Beta.User user)
        {
            var requestUrl = $"https://graph.microsoft.com/beta/users/{user.Id}/authentication/temporaryAccessPassMethods";
            HttpHelper httpHelper = new HttpHelper(new ColorConsoleLogger());
            HttpClient httpClient = await _graphServiceClient.GetHttpClientForMSGraphAsync();

            HttpResponseMessage rawResponse = await httpHelper.GetRawHttpResponseAsync(httpClient, async client => await client.GetAsync(requestUrl));

            string jsonresponse = rawResponse.ProcessHttpResponse();
            temporaryAccesPass accesPass = JsonConvert.DeserializeObject<temporaryAccesPass>(jsonresponse);

            return accesPass;
        }

        public async Task<string> DeleteExistingTemporaryAccessPassForUser(Beta.User user, string tapId)
        {
            var requestUrl = $"https://graph.microsoft.com/beta/users/{user.Id}/authentication/temporaryAccessPassMethods/{tapId}";
            HttpHelper httpHelper = new HttpHelper(new ColorConsoleLogger());
            HttpClient httpClient = await _graphServiceClient.GetHttpClientForMSGraphAsync();

            HttpResponseMessage rawResponse = await httpHelper.GetRawHttpResponseAsync(httpClient, async client => await client.DeleteAsync(requestUrl));

            string jsonresponse = rawResponse.ProcessHttpResponse();

            return jsonresponse;
        }

        public string PrintTemporaryAccessPass(temporaryAccesPass tap)
        {
            StringBuilder toreturn = new StringBuilder();

            if (tap == null)
            {
                toreturn.Append("The provided TAP object is null");
            }
            else
            {
                toreturn.AppendLine($"Id:{tap.Id}");
                toreturn.AppendLine($"TemporaryAccessPass:{tap.TemporaryAccessPass}");
                toreturn.AppendLine($"LifetimeInMinutes:{tap.LifetimeInMinutes}");
                toreturn.AppendLine($"StartDateTime:{tap.StartDateTime}");                
            }

            return toreturn.ToString();
        }

        public string PrintTemporaryAccessPassAuthenticationMethodConfiguration(temporaryAccessPassAuthenticationMethodConfiguration config)
        {
            StringBuilder toreturn = new StringBuilder();

            if (config == null)
            {
                toreturn.Append("the provided config object is null");
            }
            else
            {
                toreturn.AppendLine($"Id:{config.Id}");
                toreturn.AppendLine($"State:{config.State}");
                toreturn.AppendLine($"DefaultLifetimeInMinutes:{config.DefaultLifetimeInMinutes}");
                toreturn.AppendLine($"DefaultLength:{config.DefaultLength}");
                toreturn.AppendLine($"MinimumLifetimeInMinutes:{config.MinimumLifetimeInMinutes}");
                toreturn.AppendLine($"MaximumLifetimeInMinutes:{config.MaximumLifetimeInMinutes}");
                toreturn.AppendLine($"IsUsableOnce:{config.IsUsableOnce}");

                foreach (var target in config.IncludeTargets)
                {
                    toreturn.AppendLine($"\tTargetType: {target.TargetType}");
                    toreturn.AppendLine($"\tId: {target.Id}");
                    toreturn.AppendLine($"\tIsRegistrationRequired: {target.IsRegistrationRequired}");
                    toreturn.AppendLine($"\tUseForSignIn: {target.UseForSignIn}");
                    toreturn.AppendLine($"-------------------------------");
                }
            }

            return toreturn.ToString();
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