extern alias BetaLib;

using Common;

using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System;
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
            string[] scopes = new string[] { "user.readbasic.all", "UserAuthenticationMethod.ReadWrite.All", "Policy.Read.All", "IdentityRiskyUser.ReadWrite.All", "IdentityRiskEvent.Read.All" };

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

            //ServicePrincipalOperations servicePrincipalOperations = new ServicePrincipalOperations(betaClient);
            //UserOperations userOperations = new UserOperations(betaClient, "woodgrove.ms");
            //GroupOperations groupOperations = new GroupOperations(betaClient);

            //IEnumerable<Beta.User> allUsersInTenant = await userOperations.GetUsersAsync();
            //IList<Beta.User> randomUsersFromTenant = GenericUtility<Beta.User>.GetaRandomNumberOfItemsFromList(allUsersInTenant, 5);

            //// Conditional Access operations
            //ConditionalAccessPolicyOperations conditionalAccessPolicyOperations = new ConditionalAccessPolicyOperations(betaClient, userOperations, servicePrincipalOperations, groupOperations);

            //// List
            //Console.WriteLine("Getting CA Policies");
            //IList<Beta.ConditionalAccessPolicy> conditionalAccessPolicies = await conditionalAccessPolicyOperations.ListConditionalAccessPoliciesAsync();

            //for (int i = 0; i < conditionalAccessPolicies.Count; i++)
            //{
            //    Console.WriteLine(await conditionalAccessPolicyOperations.PrintConditionalAccessPolicyAsync(conditionalAccessPolicies[i], true));
            //    Console.WriteLine("-------------------------------------------------------------------------------");
            //}

            //var policy = await conditionalAccessPolicyOperations.GetConditionalAccessPolicyByDisplayNameAsync("Kalyan test");
            //Console.WriteLine(await conditionalAccessPolicyOperations.PrintConditionalAccessPolicyAsync(policy, true));


            // Risk detection operations

            //RiskDetectionOperations riskDetectionOperations = new RiskDetectionOperations(betaClient);

            //// List
            //Console.WriteLine("Getting risk detections");
            //IList<Beta.RiskDetection> riskDetections = await riskDetectionOperations.ListRiskDetectionsAsync();
            //await riskDetections.ForEachAsync(async detection => Console.WriteLine(await riskDetectionOperations.PrintRiskDetectionAsync(detection)));

            // Risk detection end

            ////// Risky users operations
            ////// create five random users
            ////RandomNames randomNames = new RandomNames(NameType.MaleName);

            ////IList<Beta.User> randomUsersFromTenant = new List<Beta.User>();

            ////try
            ////{
            ////    for (int i = 0; i < 5; i++)
            ////    {
            ////        var user = await userOperations.CreateUserAsync(
            ////            givenName: randomNames.GetRandom(),
            ////            surname: randomNames.GetRandom());

            ////        randomUsersFromTenant.Add(user);
            ////    }

            ////    // Get newly created users
            ////    await randomUsersFromTenant.ForEachAsync(async user =>
            ////    {
            ////        ColorConsole.WriteLine(ConsoleColor.Blue, userOperations.PrintBetaUserDetails(await userOperations.GetUserByIdAsync(user.Id)));
            ////    });

            ////    // Wait 10 seconds
            ////    await Task.Delay(10000);

            ////    RiskDetectionOperations riskDetectionOperations = new RiskDetectionOperations(betaClient);
            ////    RiskyUserOperations riskyUserOperations = new RiskyUserOperations(betaClient, userOperations);

            ////    //var riskyUsers = await riskyUserOperations.ListRiskyUsersAsync();
            ////    //riskyUsers.ForEach(async user => Console.WriteLine(await riskyUserOperations.PrintRiskyUserAsync(user)));

            ////    ColorConsole.WriteLine(ConsoleColor.Green, "Marking a random number of users as compromised");
            ////    await randomUsersFromTenant.ForEachAsync(async user => await riskyUserOperations.ConfirmCompromisedAsync(user.Id));

            ////    // Wait 10 seconds
            ////    await Task.Delay(10000);

            ////    await randomUsersFromTenant.ForEachAsync(async user =>
            ////    {
            ////        var trialRslt = await Retry.WithExpBackoff_StopOn<IList<Beta.RiskyUser>>(
            ////            async () =>
            ////            {
            ////                return await riskyUserOperations.GetRiskyUsersByUPNUnsafeAsync(user.UserPrincipalName);
            ////            },
            ////            TestforMissingRiskEvent);

            ////        Console.WriteLine($"User {user.UserPrincipalName} is marked as a risky user now");

            ////        var userriskresults = trialRslt.Result;

            ////        await userriskresults.ForEachAsync(async r => { Console.WriteLine(await riskyUserOperations.PrintRiskyUsersAsync(r, true, true)); });

            ////        // If retries occurred, log this fact
            ////        if (trialRslt.Latencies.Count > 1)
            ////        {
            ////            ColorConsole.WriteLine(ConsoleColor.Yellow, $"For {nameof(riskyUserOperations.GetRiskyUsersByIdUnsafeAsync)}, {trialRslt.Latencies.Count - 1} retries needed");
            ////        }

            ////        //Console.WriteLine(await riskyUserOperations.PrintRiskyUsersAsync(trialRslt.Result, true, true));
            ////    });

            ////    // check risk detections
            ////    ColorConsole.WriteLine(ConsoleColor.Green, "Checking risk detection logs for compromised users ");
            ////    await randomUsersFromTenant.ForEachAsync(async user =>
            ////    {
            ////        IList<Beta.RiskDetection> riskDetections = await riskDetectionOperations.ListRiskDetectionsByUpnAsync(user.UserPrincipalName);
            ////        await riskDetections.ForEachAsync(async detection => Console.WriteLine(await riskDetectionOperations.PrintRiskDetectionAsync(detection)));
            ////    });

            ////    ColorConsole.WriteLine(ConsoleColor.Green, "Dismissing a random number of compromised users ");
            ////    await randomUsersFromTenant.ForEachAsync(async user => await riskyUserOperations.DismissAsync(user.Id));

            //    // TODO: re do

            //    //// Wait 5 seconds
            //    //await Task.Delay(10000);

            //    //await randomUsersFromTenant.ForEachAsync(async user =>
            //    //{
            //    //    Beta.RiskyUser riskyUser = null;

            //    //    do
            //    //    {
            //    //        // wait 10 secs
            //    //        await Task.Delay(10000);
            //    //        var riskyUserresults = await riskyUserOperations.GetRiskyUsersByUPNUnsafeAsync(user.UserPrincipalName);
            //    //    } while (riskyUser != null);

            //    //    Console.WriteLine($"User {user.UserPrincipalName} is no longer a risky user");
            //    //});
            //}
            //catch (Exception ex)
            //{
            //    ColorConsole.WriteLine(ConsoleColor.Red, $"{ex}");
            //}
            //finally
            //{
            //    await randomUsersFromTenant.ForEachAsync(async user =>
            //    {
            //        await userOperations.DeleteUserAsync(user.Id);
            //    });
            //}

            // Authenticated methods operations
            // await GetUsersAuthenticationMethodsAsync(betaClient);
            // await GetUsersPhoneMethodsAsync(betaClient);

            // Device registration policy operations
            DeviceRegistrationPolicySettingsOperations deviceRegistrationPolicySettingsOperations = new DeviceRegistrationPolicySettingsOperations(betaClient);
            var deviceregistrationpolicy = await deviceRegistrationPolicySettingsOperations.GetDeviceRegistrationPolicyAsync();
            Console.WriteLine(deviceRegistrationPolicySettingsOperations.PrintDeviceRegistrationPolicy(deviceregistrationpolicy));

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private static bool TestforMissingRiskEvent(Exception ex)
        {
            if (ex is ServiceException)
            {
                var dce = ex as ServiceException;
                if (dce.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    return true;
                }
            }

            return false;
        }

       
    }
}