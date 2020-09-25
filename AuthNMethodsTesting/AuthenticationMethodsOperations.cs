extern alias BetaLib;

using AuthNMethodsTesting.Model;
using Common;
using Microsoft.Graph;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

        public async Task<List<Beta.AuthenticationMethod>> GetUsersAuthenticationMethodsAsync()
        {
            var methods = await _graphServiceClient.Me.Authentication.Methods.Request().GetAsync();

            return await ProcessIAuthenticationMethodsCollectionPage(methods);

            //var requestUrl = "https://graph.microsoft.com/beta/me/authentication/methods";
            //HttpHelper httpHelper = new HttpHelper(new ColorConsoleLogger());
            //HttpClient httpClient = await _graphServiceClient.GetHttpClientForMSGraphAsync();

            //HttpResponseMessage rawResponse = await httpHelper.GetRawHttpResponseAsync(httpClient, async client => await client.GetAsync(requestUrl));

            //string jsonresponse = rawResponse.ProcessHttpResponse();

            //JObject callresults = JObject.Parse(jsonresponse);
            //// get JSON result objects into a list
            //IList<JToken> results = callresults["value"].Children().ToList();

            //authenticationMethod authenticationMethod = results[0].ToObject<authenticationMethod>();
            //ColorConsole.WriteLine(ConsoleColor.Green, $"id-{authenticationMethod.id}, isUsable-{authenticationMethod.isUsable}, phoneNumber-{authenticationMethod.phoneNumber}");
        }

        private async Task<List<Beta.AuthenticationMethod>> ProcessIAuthenticationMethodsCollectionPage(Beta.IAuthenticationMethodsCollectionPage authenticationMethodsPage)
        {
            List<Beta.AuthenticationMethod> allMethods = new List<Beta.AuthenticationMethod>();

            try
            {
                if (authenticationMethodsPage != null)
                {
                    do
                    {
                        // Page through results
                        foreach (var authenticationMethod in authenticationMethodsPage.CurrentPage)
                        {
                            allMethods.Add(authenticationMethod);
                        }

                        // are there more pages (Has a @odata.nextLink ?)
                        if (authenticationMethodsPage.NextPageRequest != null)
                        {
                            authenticationMethodsPage = await authenticationMethodsPage.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            authenticationMethodsPage = null;
                        }
                    } while (authenticationMethodsPage != null);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not process the authentication methods list: {e}");
                return null;
            }

            return allMethods;
        }

        public string PrintAuthenticationMethod(Beta.AuthenticationMethod method)
        {
            StringBuilder toreturn = new StringBuilder();

            if (method == null)
            {
                toreturn.Append("The provided authN method object is null");
            }
            else
            {
                toreturn.AppendLine($"Id:{method.Id}");

                if (method is Beta.PhoneAuthenticationMethod)
                {
                    Beta.PhoneAuthenticationMethod phoneAuth = method as Beta.PhoneAuthenticationMethod;

                    toreturn.AppendLine($"PhoneNumber:{phoneAuth.PhoneNumber}");
                    toreturn.AppendLine($"PhoneType:{phoneAuth.PhoneType}");
                    toreturn.AppendLine($"SmsSignInState:{phoneAuth.SmsSignInState}");
                }
                else if (method is Beta.PasswordAuthenticationMethod)
                {
                    Beta.PasswordAuthenticationMethod passwordAuthenticationMethod = method as Beta.PasswordAuthenticationMethod;

                    toreturn.AppendLine($"Id:{passwordAuthenticationMethod.Id}");
                    toreturn.AppendLine($"Password:{passwordAuthenticationMethod.Password}");
                    toreturn.AppendLine($"CreationDateTime:{passwordAuthenticationMethod.CreationDateTime}");
                }
                else
                {
                    ColorConsole.WriteLine(ConsoleColor.Red, $"No provision in PrintAuthenticationMethod for type-{method.ODataType}");
                }
            }

            return toreturn.ToString();
        }

        public async Task<List<Beta.AuthenticationMethod>> ListMyPhoneAuthenticationMethodsAsync()
        {
            var phoneAuthMethods = await _graphServiceClient.Me.Authentication.PhoneMethods.Request().GetAsync();

            return await ProcessIAuthenticationMethodsCollectionPage(phoneAuthMethods as Beta.IAuthenticationMethodsCollectionPage);
            //return await ProcessIAuthenticationPhoneMethodsCollectionPage(phoneAuthMethods);

            //var requestUrl = "https://graph.microsoft.com/beta/me/authentication/phoneMethods";
            //HttpHelper httpHelper = new HttpHelper(new ColorConsoleLogger());
            //HttpClient httpClient = await _graphServiceClient.GetHttpClientForMSGraphAsync();

            //HttpResponseMessage rawResponse = await httpHelper.GetRawHttpResponseAsync(httpClient, async client => await client.GetAsync(requestUrl));

            //string jsonresponse = rawResponse.ProcessHttpResponse();

            //JObject callresults = JObject.Parse(jsonresponse);
            //// get JSON result objects into a list
            //IList<JToken> results = callresults["value"].Children().ToList();

            //phoneAuthenticationMethod phoneMethod = results[0].ToObject<phoneAuthenticationMethod>();
            //ColorConsole.WriteLine(ConsoleColor.Green, $"phoneType-{phoneMethod.phoneType}, phoneNumber-{phoneMethod.phoneNumber}, smsSignInState-{phoneMethod.smsSignInState}");
        }

        public async Task<Beta.AuthenticationMethod> ListMyPhoneAuthenticationMethodAsync(string phoneAuthenticationMethodId)
        {
            return await _graphServiceClient.Me.Authentication.PhoneMethods[phoneAuthenticationMethodId].Request().GetAsync();
        }

        public async Task<List<Beta.AuthenticationMethod>> ListUsersPhoneAuthenticationMethodsAsync(string userId)
        {
            var phoneAuthMethods = await _graphServiceClient.Users[userId].Authentication.PhoneMethods.Request().GetAsync();

            return await ProcessIAuthenticationMethodsCollectionPage(phoneAuthMethods as Beta.IAuthenticationMethodsCollectionPage);
        }

        public async Task<Beta.AuthenticationMethod> ListUsersPhoneAuthenticationMethodAsync(string userId, string phoneAuthenticationMethodId)
        {
            return await _graphServiceClient.Users[userId].Authentication.PhoneMethods[phoneAuthenticationMethodId].Request().GetAsync();
        }

        public async Task<List<Beta.AuthenticationMethod>> ListMyPasswordAuthenticationMethodsAsync()
        {
            var passwordAuthMethods = await _graphServiceClient.Me.Authentication.PasswordMethods.Request().GetAsync();

            return await ProcessIAuthenticationMethodsCollectionPage(passwordAuthMethods as Beta.IAuthenticationMethodsCollectionPage);
        }

        public async Task<Beta.AuthenticationMethod> ListMyPasswordAuthenticationMethodAsync(string passwordAuthenticationMethodId)
        {
            return await _graphServiceClient.Me.Authentication.PasswordMethods[passwordAuthenticationMethodId].Request().GetAsync();
        }

        public async Task<List<Beta.AuthenticationMethod>> ListUsersPasswordAuthenticationMethodsAsync(string userId)
        {
            var passwordAuthMethods = await _graphServiceClient.Users[userId].Authentication.PasswordMethods.Request().GetAsync();

            return await ProcessIAuthenticationMethodsCollectionPage(passwordAuthMethods as Beta.IAuthenticationMethodsCollectionPage);
        }

        public async Task<Beta.AuthenticationMethod> ListUsersPasswordAuthenticationMethodAsync(string userId, string passwordAuthenticationMethodId)
        {
            return await _graphServiceClient.Users[userId].Authentication.PasswordMethods[passwordAuthenticationMethodId].Request().GetAsync();
        }

        private async Task<List<Beta.AuthenticationMethod>> ProcessIAuthenticationPhoneMethodsCollectionPage(Beta.IAuthenticationPhoneMethodsCollectionPage authenticationPhoneMethodsPage)
        {
            List<Beta.AuthenticationMethod> allMethods = new List<Beta.AuthenticationMethod>();

            try
            {
                if (authenticationPhoneMethodsPage != null)
                {
                    do
                    {
                        // Page through results
                        foreach (var authenticationMethod in authenticationPhoneMethodsPage.CurrentPage)
                        {
                            allMethods.Add(authenticationMethod);
                        }

                        // are there more pages (Has a @odata.nextLink ?)
                        if (authenticationPhoneMethodsPage.NextPageRequest != null)
                        {
                            authenticationPhoneMethodsPage = await authenticationPhoneMethodsPage.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            authenticationPhoneMethodsPage = null;
                        }
                    } while (authenticationPhoneMethodsPage != null);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not process the phone authentication methods list: {e}");
                return null;
            }

            return allMethods;
        }
    }
}