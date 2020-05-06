extern alias BetaLib;

using AADGraphTesting;
using AuthNMethodsTesting.Model;
using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Beta = BetaLib.Microsoft.Graph;

namespace AuthNMethodsTesting
{
    internal class Program
    {
        private static PublicClientApplicationOptions appConfiguration = null;
        private static IConfiguration configuration;
        private static string _authority;

        private static async Task Main(string[] args)
        {
            string[] scopes = new string[] { "user.readbasic.all", "UserAuthenticationMethod.ReadWrite.All", "Policy.Read.All" };

            // Using appsettings.json as our configuration settings
            var builder = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            configuration = builder.Build();
            appConfiguration = configuration.Get<PublicClientApplicationOptions>();

            _authority = string.Concat(appConfiguration.Instance, appConfiguration.TenantId);

            // Building a public client application
            var app = PublicClientApplicationBuilder.Create(appConfiguration.ClientId)
                                                    .WithAuthority(_authority)
                                                    .WithRedirectUri(appConfiguration.RedirectUri)
                                                    .Build();

            // Initialize the Graph SDK authentication provider
            InteractiveAuthenticationProvider authenticationProvider = new InteractiveAuthenticationProvider(app, scopes);
            Beta.GraphServiceClient betaClient = new Beta.GraphServiceClient(authenticationProvider);

            ServicePrincipalOperations servicePrincipalOperations = new ServicePrincipalOperations(betaClient);
            UserOperations userOperations = new UserOperations(betaClient);
            GroupOperations groupOperations = new GroupOperations(betaClient);

            ConditionalAccessPolicyOperations conditionalAccessPolicyOperations = new ConditionalAccessPolicyOperations(betaClient, userOperations, servicePrincipalOperations, groupOperations);

            // List
            Console.WriteLine("Getting CA Policies");
            IList<Beta.ConditionalAccessPolicy> conditionalAccessPolicies = await conditionalAccessPolicyOperations.ListConditionalAccessPoliciesAsync();

            for (int i = 0; i < conditionalAccessPolicies.Count; i++)
            {
                Console.WriteLine(await conditionalAccessPolicyOperations.PrintConditionalAccessPolicyAsync(conditionalAccessPolicies[i], true));
                Console.WriteLine("-------------------------------------------------------------------------------");
            }          

            // await GetUsersAuthenticationMethodsAsync(betaClient);
            // await GetUsersPhoneMethodsAsync(betaClient);

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private static async Task GetUsersAuthenticationMethodsAsync(Beta.GraphServiceClient graphServiceClient)
        {
            var requestUrl = "https://graph.microsoft.com/beta/me/authentication/methods";
            HttpHelper httpHelper = new HttpHelper(new ColorConsoleLogger());
            HttpClient httpClient = await graphServiceClient.GetHttpClientForMSGraphAsync();

            HttpResponseMessage rawResponse = await httpHelper.GetRawHttpResponseAsync(httpClient, async client => await client.GetAsync(requestUrl));

            string jsonresponse = rawResponse.ProcessHttpResponse();

            JObject callresults = JObject.Parse(jsonresponse);
            // get JSON result objects into a list
            IList<JToken> results = callresults["value"].Children().ToList();

            authenticationMethod authenticationMethod = results[0].ToObject<authenticationMethod>();
            ColorConsole.WriteLine(ConsoleColor.Green, $"id-{authenticationMethod.id}, isUsable-{authenticationMethod.isUsable}, phoneNumber-{authenticationMethod.phoneNumber}");
        }

        private static async Task GetUsersPhoneMethodsAsync(Beta.GraphServiceClient graphServiceClient)
        {
            var requestUrl = "https://graph.microsoft.com/beta/me/authentication/phoneMethods";
            HttpHelper httpHelper = new HttpHelper(new ColorConsoleLogger());
            HttpClient httpClient = await graphServiceClient.GetHttpClientForMSGraphAsync();

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