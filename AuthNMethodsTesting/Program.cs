extern alias BetaLib;

using AuthNMethodsTesting.Model;
using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
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
            string[] scopes = new string[] { "UserAuthenticationMethod.ReadWrite.All" };

            // Using appsettings.json as our configuration settings
            var builder = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            configuration = builder.Build();

            appConfiguration = configuration
                .Get<PublicClientApplicationOptions>();

            _authority = string.Concat(appConfiguration.Instance, appConfiguration.TenantId);

            // Building a public client application
            var app = PublicClientApplicationBuilder.Create(appConfiguration.ClientId)
                                                    .WithAuthority(_authority)
                                                    .WithRedirectUri(appConfiguration.RedirectUri)
                                                    .Build();

            // Initialize the Graph SDK authentication provider
            InteractiveAuthenticationProvider authenticationProvider = new InteractiveAuthenticationProvider(app, scopes);
            Beta.GraphServiceClient betaClient = new Beta.GraphServiceClient(authenticationProvider);

            //var approleassignments = await GetUsersAppRoleAssignmentsAsync(betaClient);

            //if (approleassignments?.Count > 0)
            //{
            //    Console.WriteLine("--------------------AppRole Assignments-------------------");
            //    foreach (var approleassignment in approleassignments)
            //    {
            //        Console.WriteLine($"PrincipalDisplayName - '{approleassignment.PrincipalDisplayName}'" +
            //            $", ResourceDisplayName- '{approleassignment.ResourceDisplayName}'" +
            //            $", PrincipalType- '{approleassignment.PrincipalType}'");
            //    }
            //    Console.WriteLine("----------------------------------------------------------");
            //}

            await GetUsersPhoneMethodsAsync(betaClient);

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private static async Task<List<Beta.AppRoleAssignment>> GetUsersAppRoleAssignmentsAsync(Beta.GraphServiceClient graphServiceClient)
        {
            List<Beta.AppRoleAssignment> allAssignments = new List<Beta.AppRoleAssignment>();

            try
            {
                var approleAssignedToPages = await graphServiceClient.Me.AppRoleAssignments.Request().GetAsync();

                if (approleAssignedToPages != null)
                {
                    do
                    {
                        // Page through results
                        foreach (var user in approleAssignedToPages.CurrentPage)
                        {
                            allAssignments.Add(user);
                        }

                        // are there more pages (Has a @odata.nextLink ?)
                        if (approleAssignedToPages.NextPageRequest != null)
                        {
                            approleAssignedToPages = await approleAssignedToPages.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            approleAssignedToPages = null;
                        }
                    } while (approleAssignedToPages != null);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not retrieve the roles a user is assigned to: {e}");
                return null;
            }

            return allAssignments;
        }

        private static async Task GetUsersPhoneMethodsAsync(Beta.GraphServiceClient graphServiceClient)
        {
            var requestUrl = "https://graph.microsoft.com/beta/me/authentication/phoneMethods";
            HttpHelper httpHelper = new HttpHelper(new ColorConsoleLogger());
            HttpClient httpClient = await GetHttpClientForMSGraphAsync(graphServiceClient);

            HttpResponseMessage rawResponse = await httpHelper.GetRawHttpResponseAsync(httpClient, async client => await client.GetAsync(requestUrl));

            string jsonresponse = ProcessHttpResponse(rawResponse);

            JObject callresults = JObject.Parse(jsonresponse);
            // get JSON result objects into a list
            IList<JToken> results = callresults["value"].Children().ToList();

            phoneAuthenticationMethod phoneMethod = results[0].ToObject<phoneAuthenticationMethod>();
            ColorConsole.WriteLine(ConsoleColor.Green, $"phoneType-{phoneMethod.phoneType}, phoneNumber-{phoneMethod.phoneNumber}, smsSignInState-{phoneMethod.smsSignInState}");

        }

        private static string ProcessHttpResponse(HttpResponseMessage httpResponseMessage)
        {
            using (httpResponseMessage)
            {
                string responseString = (httpResponseMessage.Content != null) ? httpResponseMessage.GetResponseString() : string.Empty;

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    Console.WriteLine($"HttpResponse -{HttpHelper.GetFormattedJson(responseString)}");
                    return responseString;
                }
                else
                {
                    ColorConsole.WriteLine(ConsoleColor.Red, $"Http call failed with response code {httpResponseMessage.StatusCode}. Http response is \n {HttpHelper.GetFormattedJson(responseString)}");
                }
            }

            return string.Empty;
        }

        private static async Task<HttpClient> GetHttpClientForMSGraphAsync(Beta.GraphServiceClient graphServiceClient)
        {
            HttpClient httpClient = new HttpClient();

            await graphServiceClient.AuthenticationProvider.AuthenticateClient(httpClient);

            return httpClient;
        }
    }
}