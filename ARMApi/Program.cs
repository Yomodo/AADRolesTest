extern alias BetaLib;

using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Beta = BetaLib.Microsoft.Graph;

namespace ARMApi
{
    internal class Program
    {
        private static PublicClientApplicationOptions appConfiguration = null;
        private static IConfiguration configuration;
        private static string _authority;

        private static async Task Main(string[] args)
        {
            //AsyncHelper.RunSync(async ()=> await DoARMThings());

            //await DoARMThings();

            //await DoAADThings();

            await DoMSALAuthN();

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private static async Task DoMSALAuthN()
        {

            // Using appsettings.json as our configuration settings
            var builder = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            configuration = builder.Build();
            appConfiguration = configuration.Get<PublicClientApplicationOptions>();

            _authority = string.Concat(appConfiguration.Instance, appConfiguration.TenantId);
            string[] scopes = new string[] { "user.readbasic.all", "RoleManagement.ReadWrite.Directory", "AdministrativeUnit.Read.All", "PrivilegedAccess.ReadWrite.AzureResources"
                , "Directory.AccessAsUser.All", "Reports.Read.All" };

            IPublicClientApplication app = PublicClientApplicationBuilder.Create(appConfiguration.ClientId)
               .WithAuthority(new System.Uri(_authority))
               .WithRedirectUri(appConfiguration.RedirectUri)
               .Build();

            var accounts = (await app.GetAccountsAsync()).ToList();
            AuthenticationResult authResult = null;

            // Get an access token to call the To Do list service.
            try
            {
                authResult = await app.AcquireTokenSilent(scopes, accounts.FirstOrDefault())
                    .ExecuteAsync()
                    .ConfigureAwait(false);
            }
            catch (MsalUiRequiredException)
            {
                try
                {
                    // Force a sign-in (Prompt.SelectAccount), as the MSAL web browser might contain cookies for the current user
                    // and we don't necessarily want to re-sign-in the same user
                    authResult = await app.AcquireTokenInteractive(scopes)
                        .WithAccount(accounts.FirstOrDefault())
                        .WithPrompt(Microsoft.Identity.Client.Prompt.SelectAccount)
                        .ExecuteAsync()
                        .ConfigureAwait(false);
                }
                catch (MsalException ex)
                {
                    if (ex.ErrorCode == "access_denied")
                    {
                        // The user canceled sign in, take no action.
                    }
                    else
                    {
                        // An unexpected error occurred.
                        string message = ex.Message;
                        if (ex.InnerException != null)
                        {
                            message += "Error Code: " + ex.ErrorCode + "Inner Exception : " + ex.InnerException.Message;
                        }

                        Console.Write(message);
                    }
                }
            }

            if (authResult == null || string.IsNullOrEmpty(authResult.AccessToken) || string.IsNullOrEmpty(authResult.TenantId))
            {
                Console.WriteLine("Authentication result, token is null");
                throw new InvalidOperationException("Authentication result, token or tenant id returned are null");
            }

            string token = authResult.AccessToken;
            string tenantID = authResult.TenantId;

    }

    private async static Task DoARMThings()
    {
        AzureServiceManagement arm = new AzureServiceManagement();

        var subscriptions = await arm.GetAllSubscriptionsForServicePrincipalAsync();
        subscriptions.ToList().ForEach(sub => Console.WriteLine($"{sub.DisplayName}"));

        var tenants = await arm.GetAllTenantsForServicePrincipalAsync();
        tenants.ToList().ForEach(sub => Console.WriteLine($"{sub.TenantId}"));

        // TODO Research more
        //var roleAssignments = await arm.GetAllRoleAssignmentsForServicePrincipalAsync();
        //roleAssignments.ToList().ForEach(sub => Console.WriteLine($"{sub.Name}"));

        // ADAL device code flow
        // subscriptions = await arm.GetAllSubscriptionsForUserAsync();
        //subscriptions.ToList().ForEach(sub => Console.WriteLine($"{sub.DisplayName}"));

        // tenants = await arm.GetAllTenantsForUserAsync();
        //tenants.ToList().ForEach(sub => Console.WriteLine($"{sub.TenantId}"));

        // MSAL
        tenants = await arm.GetAllTenantsForUserUsingMsalAsync();
        tenants.ToList().ForEach(sub => Console.WriteLine($"{sub.TenantId}"));

        //var subscriptions = await arm.GetAllsubscriptionsForServicePrincipalUsingMsalAsync();
        //subscriptions.ToList().ForEach(sub => Console.WriteLine($"{sub.DisplayName}"));

        // arm.PrintSubscriptionsUsingMsal();
        // ArmCredentials armCredentials = new ArmCredentials();
        // Console.WriteLine(armCredentials.AuthenticateUsingMsalAsync().Result);
    }

    private async static Task DoAADThings()
    {
        //return;

        string[] scopes = new string[] { "user.readbasic.all", "RoleManagement.ReadWrite.Directory", "AdministrativeUnit.Read.All", "PrivilegedAccess.ReadWrite.AzureResources"
                , "Directory.AccessAsUser.All", "Reports.Read.All" };

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
        GraphServiceClient graphServiceClient = new GraphServiceClient(authenticationProvider);

        UserOperations userOperations = new UserOperations(betaClient);
        ServicePrincipalOperations servicePrincipalOperations = new ServicePrincipalOperations(betaClient);
        GroupOperations groupOperations = new GroupOperations(betaClient);

        DirectoryObjectOperations directoryObjectOperations = new DirectoryObjectOperations(betaClient, userOperations, groupOperations, servicePrincipalOperations);

        //RoleManagementOperations roleManagementOperations = new RoleManagementOperations(betaClient, userOperations);
        //DirectoryRolesOperations directoryRolesOperations = new DirectoryRolesOperations(betaClient, userOperations, servicePrincipalOperations);

        IEnumerable<Beta.User> allUsersInTenant = await userOperations.GetUsersAsync();
        IList<Beta.User> randomUsersFromTenant = GenericUtility<Beta.User>.GetaRandomNumberOfItemsFromList(allUsersInTenant, 5);

        //IEnumerable<Beta.ServicePrincipal> allServicePrincipals = await servicePrincipalOperations.GetAllServicePrincipalsAsync();
        //IList<Beta.ServicePrincipal> randomServicePrincipals = GenericUtility<Beta.ServicePrincipal>.GetaRandomNumberOfItemsFromList(allServicePrincipals, 3);

        #region Privileged Identity Management

        //PIMOperations pIMOperations = new PIMOperations(betaClient, userOperations, directoryObjectOperations);
        //RoleManagementOperations roleManagementOperations = new RoleManagementOperations(betaClient, userOperations);

        //// Create a custom role
        //roleManagementOperations.CreateRoleDefinition()

        //try
        //{

        //}
        //catch(Exception ex)
        //{
        //    ColorConsole.WriteLine(ConsoleColor.Red, $"{ex}");
        //}
        //finally
        //{

        //}

        // Delete a custom role

        //// GetMy assignments (DEFUNCT)
        ////var myassignments = await pIMOperations.GetMyPrivilegedRoleAssignmentsAsync();

        ////await myassignments.ForEachAsync(async asn =>
        ////{
        ////    Console.WriteLine(pIMOperations.PrintPrivilegedRoleAssignment(asn));
        ////});

        //Beta.User currentUser = await userOperations.GetMeAsync();
        //var myassignments = await pIMOperations.GetGovernanceRoleAssignmentsBySubjectIdAsync(currentUser.Id);

        //await myassignments.Where(p=> !p.ExternalId.Contains("providers")).ForEachAsync(async asn =>
        //{
        //    Console.WriteLine(await pIMOperations.PrintGovernanceRoleAssignmentAsync(asn));
        //});

        //// discover resources
        //var governanceResources = await pIMOperations.DiscoverGovernanceResourcesAsync();
        //ColorConsole.WriteLine(ConsoleColor.Green, $"Discovered a total of {governanceResources.Count} resources");

        ////governanceResources.ForEach(async resource =>
        ////{
        ////    ColorConsole.WriteLine(ConsoleColor.Green, $"----Printing details of Governance Resource-{resource.DisplayName}--");
        ////    Console.WriteLine(await pIMOperations.PrintGovernanceResourceAsync(resource));
        ////    ColorConsole.WriteLine(ConsoleColor.Green, "------------------------------------");
        ////});

        ////Console.WriteLine("All governance resource statuses");
        ////pIMOperations.AllStatuses.ForEach(x =>
        ////{
        ////    ColorConsole.WriteLine(ConsoleColor.Yellow, $"{x}");
        ////});

        ////Console.WriteLine("All governance resource types");
        ////pIMOperations.AllTypes.ForEach(x =>
        ////{
        ////    ColorConsole.WriteLine(ConsoleColor.Yellow, $"{x}");
        ////});

        //// Filter Registered resources
        //var registeredReources = governanceResources.Where(x => x.RegisteredDateTime != null).Take(1).ToList();

        //ColorConsole.WriteLine(ConsoleColor.Green, $"Discovered a total of {registeredReources.Count} registered resources");
        //registeredReources.ForEach(async resource =>
        //{
        //    ColorConsole.WriteLine(ConsoleColor.Green, $"----Printing details of a registered Governance Resource-{resource.DisplayName}--");
        //    var regiteredItem = await pIMOperations.GetGovernanceResourceByIdAsync(resource.Id);
        //    //Console.WriteLine(await pIMOperations.PrintGovernanceResourceAsync(resource, true, true));

        //    ////Roledassignmentsrequests
        //    //ColorConsole.WriteLine(ConsoleColor.Yellow, $"\tPrinting role assignment requests of governance resource -'{resource.DisplayName}'");
        //    //var roleassignmentRequests = await pIMOperations.ListGovernanceRoleAssignmentRequestsAsync(resource);
        //    //roleassignmentRequests.ForEach(async r =>
        //    //{
        //    //   ColorConsole.WriteLine(ConsoleColor.Cyan, $"\t\t{await pIMOperations.PrintGovernanceRoleAssignmentRequestAsync(r, true)}");
        //    //});

        //    ////Role settings
        //    //ColorConsole.WriteLine(ConsoleColor.Yellow, $"\tPrinting role settings of governance resource -'{resource.DisplayName}'");
        //    //var roleasettings = await pIMOperations.ListGovernanceRoleSettingsAsync(resource);
        //    //roleasettings.ForEach(async r =>
        //    //{
        //    //   ColorConsole.WriteLine(ConsoleColor.Red, $"\t\t{await pIMOperations.PrintGovernanceRoleSettingAsync(r, true)}");
        //    //});

        //    ColorConsole.WriteLine(ConsoleColor.Green, "--------------------------------------------------------------------------------");
        //    return;
        //});

        //// Register a new governed resource

        //// Unregistered resoruces
        //var unregisteredReources = governanceResources.Where(x => x.RegisteredDateTime is null && x.Type == "subscription").ToList();
        //var toregister = GenericUtility<Beta.GovernanceResource>.GetaRandomNumberOfItemsFromList(unregisteredReources, 1);

        //if (toregister.Count > 0)
        //{
        //    var toRegisterItem = await pIMOperations.GetGovernanceResourceByIdAsync(toregister[0].Id);
        //    Console.WriteLine($"----Printing details of a unregistered Governance Resource-{toRegisterItem.DisplayName} before registration--");
        //    Console.WriteLine(await pIMOperations.PrintGovernanceResourceAsync(toRegisterItem, true, true));

        //    ColorConsole.WriteLine(ConsoleColor.Red, $"Registering resource {toRegisterItem.DisplayName}");
        //    //await pIMOperations.RegisterGovernanceResourceAsync(toRegisterItem.ExternalId);

        //    var registeredItem = await pIMOperations.GetGovernanceResourceByIdAsync(toRegisterItem.Id);
        //    Console.WriteLine($"----Printing details of a  Governance Resource-{registeredItem.DisplayName} after registration--");
        //    Console.WriteLine(await pIMOperations.PrintGovernanceResourceAsync(registeredItem, true, true));
        //}

        #endregion Privileged Identity Management

        #region Directory roles and assignment

        //var directoryroles = await directoryRolesOperations.ListDirectoryRolesAsync();

        //// List
        //Console.WriteLine("Getting directory roles");

        //IList<Beta.DirectoryRole> randomDirectoryRoles = GenericUtility<Beta.DirectoryRole>.GetaRandomNumberOfItemsFromList(directoryroles, 2);

        //foreach (var directoryRole in randomDirectoryRoles)
        //{
        //    Console.WriteLine("Printing role details ");
        //    ColorConsole.WriteLine(ConsoleColor.Green, await roleManagementOperations.PrintDirectoryRoleAsync(directoryRole, true, true));

        //    Console.WriteLine("Adding users to role ");
        //    foreach (var user in randomUsersFromTenant)
        //    {
        //        Console.WriteLine($"Adding user '{user.DisplayName}' to role '{directoryRole.DisplayName}'");
        //        await roleManagementOperations.AddMemberToDirectoryRole(directoryRole, user);
        //    }

        //    Console.WriteLine("Adding service principals to role ");
        //    foreach (var servicePrincipal in randomServicePrincipals)
        //    {
        //        Console.WriteLine($"Adding service principal '{servicePrincipal.DisplayName}' to role '{directoryRole.DisplayName}'");
        //        await roleManagementOperations.AddMemberToDirectoryRole(directoryRole, servicePrincipal);
        //    }

        //    Console.WriteLine("Fetching updated role");
        //    var updatedrole = await roleManagementOperations.GetDirectoryRoleByIdAsync(directoryRole.Id);

        //    Console.WriteLine("Printing role details after update");
        //    ColorConsole.WriteLine(ConsoleColor.Green, await roleManagementOperations.PrintDirectoryRoleAsync(updatedrole, true, true));

        //    Console.WriteLine("Removing users from role ");
        //    foreach (var user in randomUsersFromTenant)
        //    {
        //        Console.WriteLine($"Removing user '{user.DisplayName}' to role '{directoryRole.DisplayName}'");
        //        await roleManagementOperations.RemoveMemberFromDirectoryRole(updatedrole, user);
        //    }

        //    Console.WriteLine("Removing service principal from role ");
        //    foreach (var servicePrincipal in randomServicePrincipals)
        //    {
        //        Console.WriteLine($"Removing service principal '{servicePrincipal.DisplayName}' from role '{directoryRole.DisplayName}'");
        //        await roleManagementOperations.RemoveMemberFromDirectoryRole(updatedrole, servicePrincipal);
        //    }

        //    Console.WriteLine("Fetching updated role");
        //    updatedrole = await roleManagementOperations.GetDirectoryRoleByIdAsync(updatedrole.Id);

        //    Console.WriteLine("Printing role details after update");
        //    ColorConsole.WriteLine(ConsoleColor.Green, await roleManagementOperations.PrintDirectoryRoleAsync(updatedrole, true, true));
        //}

        //// Print all directory roles and its members
        //Console.WriteLine("Printing all directory roles and assignments");

        //for (int i = 0; i < directoryroles.Count; i++)
        //{
        //    Console.WriteLine($"Printing role {i}/{directoryroles.Count}");

        //    var directoryRole = await directoryRolesOperations.GetDirectoryRoleByIdAsync(directoryroles[i].Id);
        //    Console.WriteLine(AsyncHelper.RunSync(async () => await directoryRolesOperations.PrintDirectoryRoleAsync(directoryroles[i], true, true)));
        //    //i++;

        //    //directoryRole = await directoryRolesOperations.GetDirectoryRoleByDisplayNameAsync(directoryroles[i].DisplayName);
        //    //Console.WriteLine(AsyncHelper.RunSync(async () => await directoryRolesOperations.PrintDirectoryRoleAsync(directoryroles[i], true, true)));
        //}

        #endregion Directory roles and assignment

        #region Unified role definition and assignment

        RoleManagementOperations roleManagementOperations = new RoleManagementOperations(betaClient, userOperations);
        DirectoryRolesOperations directoryRolesOperations = new DirectoryRolesOperations(betaClient, userOperations, servicePrincipalOperations);

        // Delete esiting custom role
        string customRoleName = "Application Registration Support Administrator";

        var customroles = await roleManagementOperations.GetRoleDefinitionByDisplayNameAsync(customRoleName);

        if (customroles.ToList().Count() > 0)
        {
            customroles.ForEach(async x =>
            {
                ColorConsole.WriteLine(ConsoleColor.Green, $"Deleting custom role '{x.DisplayName}'");
                await roleManagementOperations.DeleteRoleDefinitionAsync(x.Id);
            });
        }

        //// Create
        //Console.WriteLine("Creating role definition");
        //var roledefinition = await roleManagementOperations.CreateRoleDefinition();

        //try
        //{
        //    // Get
        //    Console.WriteLine("Getting role definition");
        //    roledefinition = await roleManagementOperations.GetRoleDefinitionByIdAsync(roledefinition.Id);
        //    await roleManagementOperations.PrintRoleDefinition(roledefinition, false);

        //    // Update
        //    Console.WriteLine("Updating role definition");
        //    await roleManagementOperations.UpdateRoleDefinitionAsync(roledefinition.Id, true);

        //    Console.WriteLine("Creating role assignments");
        //    IList<Beta.UnifiedRoleAssignment> roleAssignments = await roleManagementOperations.CreateRoleAssignment(roledefinition, randomUsersFromTenant);

        //    //Get roleAssignemnt
        //    Console.WriteLine("Getting newly created role assignments");
        //    foreach (var newroleAssignment in roleAssignments)
        //    {
        //        var roleAssignment = await roleManagementOperations.GetRoleAssignmentByIdAsync(newroleAssignment.Id);
        //        Console.WriteLine("\t" + await roleManagementOperations.PrintRoleAssignment(roleAssignment));
        //    }

        //    // Get
        //    Console.WriteLine("Getting role definition with assignments after update");
        //    roledefinition = await roleManagementOperations.GetRoleDefinitionByIdAsync(roledefinition.Id);
        //    await roleManagementOperations.PrintRoleDefinition(roledefinition, false);

        //    // removing role assignments
        //    Console.WriteLine("Removing role assignments");

        //    int assignmentsToRemove = roleAssignments.Count - 3;
        //    for (int i = 0; i < assignmentsToRemove; i++)
        //    {
        //        await roleManagementOperations.DeleteRoleAssignmentAsync(roleAssignments[i].Id);
        //    }
        //    await roleManagementOperations.PrintRoleDefinition(roledefinition, true);

        //    //List
        //Console.WriteLine("Listing all role definitions");
        //    var roleDefinitions = await roleManagementOperations.ListUnifiedRoleDefinitions();
        //    roleDefinitions.ForEach(y => ColorConsole.WriteLine(ConsoleColor.Green, $"Role:- DisplayName-{y.DisplayName}, IsBuiltIn-{y.IsBuiltIn},IsEnabled-{y.IsEnabled}Id-{y.Id},Description-{y.Description}"));
        //}
        //catch (Exception ex)
        //{
        //    ColorConsole.WriteLine(ConsoleColor.Red, $"{ex}");
        //}
        //finally
        //{
        //    // Delete role definition
        //    ColorConsole.WriteLine(ConsoleColor.DarkRed, "Deleting role definition");
        //    await roleManagementOperations.DeleteRoleDefinitionAsync(roledefinition.Id);

        //    roledefinition = await roleManagementOperations.GetRoleDefinitionByIdAsync(roledefinition.Id);

        //    if (roledefinition == null)
        //    {
        //        ColorConsole.WriteLine(ConsoleColor.Green, "Role definition successfully deleted");
        //    }

        //    IEnumerable<Beta.UnifiedRoleDefinition> roledefinitionstoDelete = await roleManagementOperations.GetRoleDefinitionByDisplayNameAsync("Application Registration Support Administrator");

        //    if (roledefinitionstoDelete.Count() > 0)
        //    {
        //        foreach (var roleDef in roledefinitionstoDelete)
        //        {
        //            await roleManagementOperations.DeleteRoleDefinitionAsync(roleDef.Id);
        //        }
        //    }
        //}

        #endregion Unified role definition and assignment

        #region AAD reports operations

        //AADReportsOperations reportsOperations = new AADReportsOperations(betaClient, userOperations);

        //ColorConsole.WriteLine(ConsoleColor.Green, $"fetching all users registered for SSPR");
        //var allusersregisteredforSSPR = await reportsOperations.ListCredentialUserRegistrationRegisteredForSSPRAsync();

        //allusersregisteredforSSPR.ForEach(line =>
        //{
        //    Console.WriteLine(reportsOperations.PrintCredentialUserRegistrationDetails(line));
        //});

        //ColorConsole.WriteLine(ConsoleColor.Green, $"End of users registered for SSPR");


        //ColorConsole.WriteLine(ConsoleColor.Green, $"Fetching all users registered for MFA");
        //var allusersregisteredforMFA = await reportsOperations.ListCredentialUserRegistrationRegisteredForMFAAsync();

        //allusersregisteredforMFA.ForEach(line =>
        //{
        //    Console.WriteLine(reportsOperations.PrintCredentialUserRegistrationDetails(line));
        //});

        //ColorConsole.WriteLine(ConsoleColor.Green, $"End of users registered for MFA");

        //IList<Beta.CredentialUserRegistrationDetails> randomUsersregisteredForMFA = GenericUtility<Beta.CredentialUserRegistrationDetails>.GetaRandomNumberOfItemsFromList(allusersregisteredforMFA, 2);

        //if(randomUsersregisteredForMFA.Count() > 0)
        //{
        //    ColorConsole.WriteLine(ConsoleColor.Green, $"Fetching users by userPrincipalName");
        //    randomUsersregisteredForMFA.ForEach(async user => {
        //        var data = await reportsOperations.ListCredentialUserRegistrationDetailsByUserPrincipalNameAsync(user.UserPrincipalName);
        //        Console.WriteLine(reportsOperations.PrintCredentialUserRegistrationDetails(data));
        //    });                                

        //    ColorConsole.WriteLine(ConsoleColor.Green, $"End of users registered for MFA");

        //}

        #endregion AAD reports operations
    }
}
}