extern alias BetaLib;

using Common;

using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Beta = BetaLib.Microsoft.Graph;

namespace AuthNMethodsTesting
{
    internal class Program
    {
        private static PublicClientApplicationOptions appConfiguration = null;
        private static IConfiguration configuration;
        private static string _authority;
        private const string tenant = "woodgrove.ms";

        private static async Task Main(string[] args)
        {
            string[] scopes = new string[] { "user.readbasic.all", "UserAuthenticationMethod.ReadWrite.All", "Policy.Read.All", "Policy.ReadWrite.AuthenticationMethod", "IdentityRiskyUser.ReadWrite.All", "IdentityRiskEvent.Read.All", "SecurityEvents.ReadWrite.All" };

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
            UserOperations userOperations = new UserOperations(betaClient, "woodgrove.ms");
            //GroupOperations groupOperations = new GroupOperations(betaClient);

            //IEnumerable<Beta.User> allUsersInTenant = await userOperations.GetUsersAsync();
            //IList<Beta.User> randomUsersFromTenant = GenericUtility<Beta.User>.GetaRandomNumberOfItemsFromList(allUsersInTenant, 5);

            // use app only beta client for a few operations


            #region Groups App only operations

            Beta.GraphServiceClient betaConfidentialClient = BuildGraphServiceClientForAppOnlyScenarios();
            UserOperations userAppOnlyOperations = new UserOperations(betaConfidentialClient, "woodgrove.ms");
            GroupOperations groupAppOnlyOperations = new GroupOperations(betaConfidentialClient);

            Beta.Group newGroup = null;
            bool groupCreated = false;

            try
            {
                IEnumerable<Beta.User> allUsersInTenant = await userAppOnlyOperations.GetUsersAsync();
                IEnumerable<Beta.User> allNonGuestUsersInTenant = await userAppOnlyOperations.GetNonGuestUsersAsync();

                IEnumerable<Beta.User> membersToAdd = GenericUtility<Beta.User>.GetaRandomNumberOfItemsFromList(allNonGuestUsersInTenant, 5);
                IList<Beta.User> ownersToAdd = GenericUtility<Beta.User>.GetaRandomNumberOfItemsFromList(allNonGuestUsersInTenant, 2);

                var signedInUser = await userOperations.GetMeAsync();
                if (ownersToAdd.Where(x => x.Id == signedInUser.Id).Count() == 0)
                {
                    ownersToAdd.Add(signedInUser);
                }

                IEnumerable<Beta.User> ownersToUpdate = allNonGuestUsersInTenant.Except(ownersToAdd).Take(2);
                IEnumerable<Beta.User> membersToUpdate = allNonGuestUsersInTenant.Except(membersToAdd).Take(5);

                newGroup = await groupAppOnlyOperations.CreateUnifiedGroupAsync(tenant, membersToAdd, ownersToAdd);

                // Wait for group to be created
                Beta.Group grp = null;

                while (grp == null)
                {
                    await Task.Delay(3000);
                    grp = await groupAppOnlyOperations.GetGroupByIdAsync(newGroup.Id, true);
                    if (grp == null)
                    {
                        ColorConsole.WriteLine(ConsoleColor.DarkGreen, $"Failed to pick details of the newly created dynamic group. Trying again.. ");
                    }
                }

                groupCreated = true;
                ColorConsole.WriteLine(ConsoleColor.Green, $"Printing details of the newly created Unified group ");
                Console.WriteLine(await groupAppOnlyOperations.PrintGroupDetails(newGroup, true, true));

                ColorConsole.WriteLine(ConsoleColor.Green, $"Updating group's owners and members");
                // Update List
                foreach (var owner in ownersToUpdate)
                {
                    await groupAppOnlyOperations.AddOwnerToGroupAsync(newGroup, owner);
                }

                foreach (var member in membersToUpdate)
                {
                    await groupAppOnlyOperations.AddMemberToGroup(newGroup, member);
                }

                await Task.Delay(3000);
                Beta.Group group = await groupAppOnlyOperations.GetGroupByIdAsync(newGroup.Id, true);

                ColorConsole.WriteLine(ConsoleColor.Green, $"Printing details of the newly created Unified group after updating group's owners and members.");
                Console.WriteLine(await groupAppOnlyOperations.PrintGroupDetails(group, true, true));

                //newGroup = await groupAppOnlyOperations.AllowExternalSendersAsync(newGroup);

                // Delete a bunch
                ColorConsole.WriteLine(ConsoleColor.Green, $"Deleting a few group's owners and members.");
                foreach (var owner in ownersToAdd)
                {
                    await groupAppOnlyOperations.RemoveGroupOwnerAsync(newGroup, owner);
                }

                foreach (var member in membersToAdd)
                {
                    await groupAppOnlyOperations.RemoveGroupMemberAsync(newGroup, member);
                }

                await Task.Delay(3000);
                group = await groupAppOnlyOperations.GetGroupByIdAsync(newGroup.Id, true);
                ColorConsole.WriteLine(ConsoleColor.Green, $"Printing details of the newly created Unified group after deleting a few group's owners and members.");
                Console.WriteLine(await groupAppOnlyOperations.PrintGroupDetails(newGroup, true, true));
            }
            catch (Exception ex)
            {
                ColorConsole.WriteLine(ConsoleColor.Red, $"{ex}");
            }
            finally
            {
                if (groupCreated)
                {
                    ColorConsole.WriteLine(ConsoleColor.Green, "Press any key to delete this group");
                    Console.ReadKey();
                    await groupAppOnlyOperations.DeleteGroupAsync(newGroup);
                }
            }

            #endregion Groups App only operations

            // use app only beta client for a few operations end

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
            //DeviceRegistrationPolicySettingsOperations deviceRegistrationPolicySettingsOperations = new DeviceRegistrationPolicySettingsOperations(betaClient);
            //var deviceregistrationpolicy = await deviceRegistrationPolicySettingsOperations.GetDeviceRegistrationPolicyAsync();
            //Console.WriteLine(deviceRegistrationPolicySettingsOperations.PrintDeviceRegistrationPolicy(deviceregistrationpolicy));

            //ColorConsole.WriteLine(ConsoleColor.Green, $"fetching all Alerts");

            //AlertsOperations alertOperations = new AlertsOperations(betaClient);

            //var alerts = await alertOperations.ListAlertsAsync();

            //alerts.ForEach(alert =>
            //{
            //    Console.WriteLine(alertOperations.PrintAlert(alert));
            //});

            //ColorConsole.WriteLine(ConsoleColor.Green, $"End of printing all alerts");

            //// SecureScore operations
            //ColorConsole.WriteLine(ConsoleColor.Green, $"fetching secure scores");

            //SecureScoresOperations secureScoresOperations = new SecureScoresOperations(betaClient);

            //var secureScores = await secureScoresOperations.ListSecureScoresAsync();

            //secureScores.ForEach(alert =>
            //{
            //    Console.WriteLine(secureScoresOperations.PrintSecureScore(alert, true));
            //    ColorConsole.WriteLine(ConsoleColor.Green, $"-------------------------------------------------");
            //});

            //ColorConsole.WriteLine(ConsoleColor.Green, $"End of printing secure scores");
            //// end of SecureScore operations

            //// Temporary Access Pass operations

            //AuthenticationMethodsOperations authenticationMethodsOperations = new AuthenticationMethodsOperations(betaClient);

            //ColorConsole.WriteLine(ConsoleColor.Green, $"fetching current TAP config");

            //var TAPPolicyConfig = await authenticationMethodsOperations.GetTemporaryAccessPassConfigurationAsync();

            //ColorConsole.WriteLine(ConsoleColor.White, authenticationMethodsOperations.PrintTemporaryAccessPassAuthenticationMethodConfiguration(TAPPolicyConfig));

            //// Update TAP config
            //TAPPolicyConfig.State = "enabled";
            //TAPPolicyConfig.MaximumLifetimeInMinutes = 1234;

            //ColorConsole.WriteLine(ConsoleColor.Green, $"Updating TAP config");
            //var serverresponse = await authenticationMethodsOperations.UpdateTemporaryAccessPassConfigurationAsync(TAPPolicyConfig);

            //ColorConsole.WriteLine(ConsoleColor.Yellow, $"Update call result-{serverresponse}");

            //ColorConsole.WriteLine(ConsoleColor.Green, $"fetching TAP config after update");

            //TAPPolicyConfig = await authenticationMethodsOperations.GetTemporaryAccessPassConfigurationAsync();

            //ColorConsole.WriteLine(ConsoleColor.White, authenticationMethodsOperations.PrintTemporaryAccessPassAuthenticationMethodConfiguration(TAPPolicyConfig));

            //// Temporary Access Pass operations end

            //// TAP user operations
            //AuthenticationMethodsOperations authenticationMethodsOperations = new AuthenticationMethodsOperations(betaClient);

            //// create a random user
            //RandomNames randomNames = new RandomNames(NameType.MaleName);

            //var user = await userOperations.CreateUserAsync(
            //    givenName: randomNames.GetRandom(),
            //    surname: randomNames.GetRandom());

            //ColorConsole.WriteLine(ConsoleColor.Green, $"Fetching newly created user after creation");
            //ColorConsole.WriteLine(ConsoleColor.Blue, userOperations.PrintBetaUserDetails(await userOperations.GetUserByIdAsync(user.Id)));

            //// Wait 10 seconds
            //await Task.Delay(5000);

            //try
            //{
            //    ColorConsole.WriteLine(ConsoleColor.Green, $"Checking for an existing TAP for this user");
            //    var tap = await authenticationMethodsOperations.GetExistingTemporaryAccessPassForUser(user);

            //    ColorConsole.WriteLine(ConsoleColor.Green, $"Printing any existing TAP for this user");
            //    ColorConsole.WriteLine(ConsoleColor.Blue, authenticationMethodsOperations.PrintTemporaryAccessPass(tap));

            //    ColorConsole.WriteLine(ConsoleColor.Green, $"Creating a new TAP for this user");
            //    var newtap = await authenticationMethodsOperations.GenerateTemporaryAccessPassForUser(user, 60);

            //    ColorConsole.WriteLine(ConsoleColor.Green, $"Printing the newly generated TAP for this user");
            //    ColorConsole.WriteLine(ConsoleColor.Blue, authenticationMethodsOperations.PrintTemporaryAccessPass(newtap));

            //    Console.WriteLine($"Try the new TAP after {DateTime.Now.AddMinutes(10)} and then press any key to continue..");
            //    var timer = new Timer();
            //    timer.Interval = 600000;

            //    // Hook up the Elapsed event for the timer.
            //    timer.Elapsed += new ElapsedEventHandler(delegate (Object o, ElapsedEventArgs a)
            //    {
            //        ColorConsole.WriteLine(ConsoleColor.Cyan, $"The new TAP can be tried now..");
            //        // Start the timer
            //        timer.Enabled = false;
            //    });

            //    Console.ReadKey();

            //    ColorConsole.WriteLine(ConsoleColor.Green, $"Deleting the newly generated TAP for this user");
            //    await authenticationMethodsOperations.DeleteExistingTemporaryAccessPassForUser(user, newtap.Id);

            //    ColorConsole.WriteLine(ConsoleColor.Green, $"Checking for an existing TAP for this user");
            //    tap = await authenticationMethodsOperations.GetExistingTemporaryAccessPassForUser(user);

            //    ColorConsole.WriteLine(ConsoleColor.Green, $"Printing any existing TAP for this user");
            //    ColorConsole.WriteLine(ConsoleColor.Blue, authenticationMethodsOperations.PrintTemporaryAccessPass(tap));
            //}
            //catch (Exception ex)
            //{
            //    ColorConsole.WriteLine(ConsoleColor.Red, $"{ex}");
            //}
            //finally
            //{
            //    await userOperations.DeleteUserAsync(user.Id);
            //}

            //// TAP user operations end

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private static void OnTimedEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            throw new NotImplementedException();
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

        public static Beta.GraphServiceClient BuildGraphServiceClientForAppOnlyScenarios()
        {

            // Using appsettings.json as our configuration settings
            var builder = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings2.local.json");

            configuration = builder.Build();
            ConfidentialClientApplicationOptions appConfiguration = configuration.Get<ConfidentialClientApplicationOptions>();

            string authority = string.Concat(appConfiguration.Instance, appConfiguration.TenantId);

            // Building a public client application
            var app = ConfidentialClientApplicationBuilder.Create(appConfiguration.ClientId)
                                                    .WithClientSecret(appConfiguration.ClientSecret)
                                                    .WithAuthority(authority)
                                                    .Build();

            // Initialize the Graph SDK authentication provider
            ClientCredentialProvider authenticationProvider = new ClientCredentialProvider(app, "https://graph.microsoft.com/.default");
            Beta.GraphServiceClient betaClient = new Beta.GraphServiceClient(authenticationProvider);

            return betaClient; 
        }
    }
}