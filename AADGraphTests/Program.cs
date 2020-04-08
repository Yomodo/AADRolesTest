extern alias BetaLib;

using Common;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beta = BetaLib.Microsoft.Graph;

/***********************************************************
 * Purpose: Experiments with the App Api
 * Author: Kalyan Krishna
 * *********************************************************/

namespace AADGraphTesting
{
    internal class Program
    {
        private const string clientId = "5c1e701d-ebf8-439c-beee-4d5c58890c93";
        private const string tenant = "kkaad.onmicrosoft.com";
        private const string redirectUri = "msal5c1e701d-ebf8-439c-beee-4d5c58890c93://auth";

        // Change the following between each call to create/update user if not deleting the user
        private static string givenName = "test99";

        private static string surname = "user99";

        private static async Task Main(string[] args)
        {
            // Initialize and prepare MSAL
            string[] scopes = new string[] { "user.read", "user.readwrite.all", "Directory.AccessAsUser.All", "Directory.ReadWrite.All", "Contacts.ReadWrite", "AppRoleAssignment.ReadWrite.All" };

            IPublicClientApplication app = PublicClientApplicationBuilder.Create(clientId)
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{tenant}"))
                .WithRedirectUri(redirectUri)
                .Build();

            // Initialize the Graph SDK authentication provider
            InteractiveAuthenticationProvider authenticationProvider = new InteractiveAuthenticationProvider(app, scopes);
            // GraphServiceClient graphServiceClient = new GraphServiceClient(authenticationProvider);

            Beta.GraphServiceClient betaClient = new Beta.GraphServiceClient(authenticationProvider);
            //Beta.ServicePrincipal graphServicePrincipal = GetServicePrincipalByAppDisplayNameAsync(betaClient, "Microsoft Graph").Result;

            #region Application operations

            // List<Beta.Application> applications = GetAllApplicationsAsync(betaClient).Result;
            // applications.ForEach(async (u) => await PrintApplicationDetailsAsync(u, betaClient);
            // Beta.Application rolesapp = applications.FirstOrDefault(x => x.DisplayName == "WebApp-RolesClaims");

            //IEnumerable<Beta.User> allUsersInTenant = await UserOperations.GetUsersAsync(betaClient);

            //Beta.Application newApp = await CreateApplicationAsync(betaClient);

            //try
            //{
            //    await PrintApplicationDetailsAsync(newApp, betaClient);
            //    await AssignUsersToAppRoles(betaClient, newApp, allUsersInTenant.ToList());
            //    await PrintServicePrincipalDetailsAsync(newApp, betaClient);
            //    await UpdateServicePrincipalSettings(betaClient, newApp, allUsersInTenant);
            //    await PrintServicePrincipalDetailsAsync(newApp, betaClient);
            //}
            //catch (Exception ex)
            //{
            //    ColorConsole.WriteLine(ConsoleColor.Red, $"{ex}");
            //}
            //finally
            //{
            //    ColorConsole.WriteLine(ConsoleColor.Green, "Press any key to delete this app");
            //    Console.ReadKey();
            //    await DeleteApplicationAsync(newApp, betaClient);
            //}

            #endregion Application operations

            #region appRoleAssignments

            //List<Beta.AppRoleAssignment> usersApproleAssignments = GetUsersAppRoleAssignmentsAsync(betaClient).Result;
            //usersApproleAssignments.ForEach(u => PrintAppRoleAssignment(u);

            #endregion appRoleAssignments

            #region Unified Groups operations

            //GroupOperations groupOperations = new GroupOperations(betaClient);
            //UserOperations userOperations = new UserOperations(betaClient);
            //Beta.Group newGroup = null;
            //bool groupCreated = false;

            //try
            //{
            //    IEnumerable<Beta.User> allUsersInTenant = await userOperations.GetUsersAsync();
            //    IEnumerable<Beta.User> allNonGuestUsersInTenant = await userOperations.GetNonGuestUsersAsync();

            //    IEnumerable<Beta.User> membersToAdd = GenericUtility<Beta.User>.GetaRandomNumberOfItemsFromList(allUsersInTenant, 10);
            //    IEnumerable<Beta.User> ownersToAdd = GenericUtility<Beta.User>.GetaRandomNumberOfItemsFromList(allNonGuestUsersInTenant, 2);

            //    IEnumerable<Beta.User> ownersToUpdate = allNonGuestUsersInTenant.Except(ownersToAdd).Take(2);
            //    IEnumerable<Beta.User> membersToUpdate = allUsersInTenant.Except(membersToAdd);

            //    newGroup = await groupOperations.CreateUnifiedGroupAsync(tenant, membersToAdd, ownersToAdd);
            //    groupCreated = true;
            //    await groupOperations.PrintGroupDetails(newGroup, true);

            //    // Update List
            //    ownersToUpdate.ToList().ForEach(y => AsyncHelper.RunSync(async () =>
            //        await groupOperations.AddOwnerToGroupAsync(newGroup, y)));

            //    membersToUpdate.ToList().ForEach(y => AsyncHelper.RunSync(async () =>
            //        await groupOperations.AddMemberToGroup(newGroup, y)));

            //    await groupOperations.PrintGroupDetails(newGroup, true);

            //    //newGroup = await groupOperations.AllowExternalSendersAsync(newGroup);

            //    // Delete a bunch
            //    ownersToAdd.ToList().ForEach(y => AsyncHelper.RunSync(async () =>
            //        await groupOperations.RemoveGroupOwnerAsync(newGroup, y)));

            //    membersToAdd.ToList().ForEach(y => AsyncHelper.RunSync(async () =>
            //        await groupOperations.RemoveGroupMemberAsync(newGroup, y)));

            //    await groupOperations.PrintGroupDetails(newGroup, true);
            //}
            //catch (Exception ex)
            //{
            //    ColorConsole.WriteLine(ConsoleColor.Red, $"{ex}");
            //}
            //finally
            //{
            //    if (groupCreated)
            //    {
            //        ColorConsole.WriteLine(ConsoleColor.Green, "Press any key to delete this group");
            //        Console.ReadKey();
            //        await groupOperations.DeleteGroupAsync(newGroup);
            //    }
            //}

            #endregion Unified Groups operations

            #region Distribution Groups operations

            GroupOperations groupOperations = new GroupOperations(betaClient);
            UserOperations userOperations = new UserOperations(betaClient);
            Beta.Group newGroup = null;
            bool groupCreated = false;

            try
            {
                IEnumerable<Beta.User> allUsersInTenant = await userOperations.GetUsersAsync();
                IEnumerable<Beta.User> allNonGuestUsersInTenant = await userOperations.GetNonGuestUsersAsync();

                var signedInUser = await userOperations.GetMeAsync();

                IEnumerable<Beta.User> membersToAdd = GenericUtility<Beta.User>.GetaRandomNumberOfItemsFromList(allUsersInTenant, 5);
                // Remove the current user as they have been added as owner automatically
                IEnumerable<Beta.User> ownersToAdd = GenericUtility<Beta.User>.GetaRandomNumberOfItemsFromList(allNonGuestUsersInTenant.Except(new List<Beta.User> { signedInUser }), 2);

                newGroup = await groupOperations.CreateDistributionGroupAsync(tenant);
                groupCreated = true;

                await groupOperations.PrintGroupDetails(newGroup, true);

                // Add owners
                ownersToAdd.ToList().ForEach(y => AsyncHelper.RunSync(async () =>
                    await groupOperations.AddOwnerToGroupAsync(newGroup, y)));

                // Add members
                membersToAdd.ToList().ForEach(y => AsyncHelper.RunSync(async () =>
                    await groupOperations.AddMemberToGroup(newGroup, y)));

                await groupOperations.PrintGroupDetails(newGroup, true);
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
                    await groupOperations.DeleteGroupAsync(newGroup);
                }
            }

            #endregion Distribution Groups operations

            #region user operations

            //// Get information from Graph about the currently signed-In user
            //Console.WriteLine("--Fetching details of the currently signed-in user--");
            //GetMeAsync(graphServiceClient).GetAwaiter().GetResult();
            //Console.WriteLine("---------");

            //// Create a new user
            //Console.WriteLine($"--Creating a new user in the tenant '{tenant}'--");
            //User newUser = CreateUserAsync(graphServiceClient).Result;
            //PrintUserDetails(newUser);
            //Console.WriteLine("---------");

            //// Update an existing user
            //if (newUser != null)
            //{
            //    Console.WriteLine("--Updating the detail of an existing user--");
            //    User updatedUser = UpdateUserAsync(graphServiceClient, userId: newUser.Id, jobTitle: "Program Manager").Result;
            //    PrintUserDetails(updatedUser);
            //    Console.WriteLine("---------");
            //}

            //// List existing users
            //Console.WriteLine("--Listing all users in the tenant--");
            //List<User> users = GetUsersAsync(graphServiceClient).Result;
            //users.ForEach(u => PrintUserDetails(u));
            //Console.WriteLine("---------");

            //// Delete this user
            //Console.WriteLine("--Deleting a user in the tenant--");
            //if (newUser != null)
            //{
            //    DeleteUserAsync(graphServiceClient, newUser?.Id).GetAwaiter().GetResult(); ;
            //}

            //Console.WriteLine("---------");

            //// List existing users after deletion
            //Console.WriteLine("--Listing all users in the tenant after deleting a user.--");
            //users = GetUsersAsync(graphServiceClient).Result;
            //users.ForEach(u => PrintUserDetails(u));
            //Console.WriteLine("---------");

            #endregion user operations

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private static async Task AssignUsersToAppRoles(Beta.GraphServiceClient graphServiceClient, Beta.Application application,
            IList<Beta.User> users)
        {
            Beta.ServicePrincipal servicePrincipal = await GetServicePrincipalByAppIdAsync(graphServiceClient, application.AppId);

            try
            {
                List<Beta.AppRole> userassignableroles = servicePrincipal.AppRoles.ToList().Where(x => x.AllowedMemberTypes.ToList().Contains("User")).ToList();

                userassignableroles.ForEach(async (approle) =>
                {
                    ColorConsole.WriteLine($"Role name {approle.DisplayName}");

                    IList<Beta.User> usersToAssign = GenericUtility<Beta.User>.GetaRandomNumberOfItemsFromList(users, 6).ToList();

                    for (int i = 0; i < usersToAssign.Count(); i++)
                    {
                        var approleAssignment = new Beta.AppRoleAssignment();
                        approleAssignment.PrincipalId = new Guid(usersToAssign[i].Id);
                        approleAssignment.ResourceId = new Guid(servicePrincipal.Id);
                        approleAssignment.AppRoleId = approle.Id;

                        // await graphServiceClient.AppRoleAssignments.Request().AddAsync(approleAssignment);
                        var assignment = await graphServiceClient.ServicePrincipals[servicePrincipal.Id].AppRoleAssignedTo.Request().AddAsync(approleAssignment);

                        Console.WriteLine($"{assignment.PrincipalDisplayName} assigned to AppRole '{approle.DisplayName}' with id '{assignment.Id}' ");
                    }
                });
            }
            catch (Exception ex)
            {
                ColorConsole.WriteLine(ConsoleColor.Red, $"{ex}");
                throw;
            }

            ColorConsole.WriteLine(ConsoleColor.Green, "All app role assignments complete");
        }

        private static async Task UpdateServicePrincipalSettings(Beta.GraphServiceClient graphServiceClient, Beta.Application application,
            IEnumerable<Beta.User> allUsersInTenant)
        {
            Beta.ServicePrincipal servicePrincipal = await GetServicePrincipalByAppIdAsync(graphServiceClient, application.AppId);

            if (servicePrincipal == null)
            {
                ColorConsole.WriteLine(ConsoleColor.Red, $"No service principal for app '{application.DisplayName}' found! ");
            }

            servicePrincipal.AppRoleAssignmentRequired = true;

            IList<string> replyUrlsToAdd = new List<string>()
            {
                "https://www.kkaad.onmicrosoft.com/myotherapp/landingpage",
                "https://www.kkaad.onmicrosoft.com/myotherapp/landingpage2"
            };

            //servicePrincipal.ReplyUrls.ToList().Add("https://www.kkaad.onmicrosoft.com/myotherapp/landingpage");
            //servicePrincipal.ReplyUrls = GenericUtility<string>.BackupAddAndReplace(servicePrincipal.ReplyUrls, replyUrlsToAdd);

            IEnumerable<Beta.User> owners = GenericUtility<Beta.User>.GetaRandomNumberOfItemsFromList(allUsersInTenant, 3);
            //servicePrincipal.Owners = Beta.GraphServiceUsersCollectionPage();

            //Beta.GraphServiceUsersCollectionPage userpage = new Beta.GraphServiceUsersCollectionPage();
            //owners.ToList().ForEach(x => servicePrincipal.Owners.Add(x));

            // Remove a couple of role assignments
            Dictionary<Guid?, Beta.AppRole> approles = new Dictionary<Guid?, Beta.AppRole>();
            if (servicePrincipal?.AppRoles?.Count() > 0)
            {
                Console.WriteLine("--------------------------AppRoles-------------------");
                foreach (var appRole in servicePrincipal.AppRoles)
                {
                    approles.Add(appRole.Id, appRole);
                    Console.WriteLine($"Id-{appRole.Id}, IsEnabled- {appRole.IsEnabled}, UserConsentDisplayName-{appRole.Value}, " +
                        $"AllowedMemberTypes- {String.Join(",", appRole.AllowedMemberTypes)}");
                }
                Console.WriteLine("----------------------------------------------------------");
            }

            var approleassignments = await GetServicePrincipalsAppRoleAssignedToAsync(graphServiceClient, servicePrincipal);

            var assignmentsToDelete = GenericUtility<Beta.AppRoleAssignment>.GetaRandomNumberOfItemsFromList(approleassignments, 4).ToList();

            assignmentsToDelete.ForEach(async (assignment) =>
            {
                await graphServiceClient.ServicePrincipals[servicePrincipal.Id].AppRoleAssignedTo[assignment.Id].Request().DeleteAsync();
                Console.WriteLine($"'{approles[assignment.AppRoleId].DisplayName}' assigned to {assignment.PrincipalDisplayName} with id '{assignment.Id}' deleted");
            });

            await graphServiceClient.ServicePrincipals[servicePrincipal.Id].Request().UpdateAsync(servicePrincipal);
        }

        private static async Task RemoveUsersFromAppRoles(Beta.GraphServiceClient graphServiceClient, Beta.Application application,
            IList<Beta.User> users)
        {
            Beta.ServicePrincipal servicePrincipal = await GetServicePrincipalByAppIdAsync(graphServiceClient, application.AppId);

            try
            {
                List<Beta.AppRole> userassignableroles = servicePrincipal.AppRoles.ToList().Where(x => x.AllowedMemberTypes.ToList().Contains("User")).ToList();

                userassignableroles.ForEach(async (approle) =>
                {
                    ColorConsole.WriteLine($"Role name {approle.DisplayName}");

                    int end = users.Count() / 2;

                    for (int i = 0; i < end; i++)
                    {
                        var approleAssignment = new Beta.AppRoleAssignment();
                        approleAssignment.PrincipalId = new Guid(users[i].Id);
                        approleAssignment.ResourceId = new Guid(servicePrincipal.Id);
                        approleAssignment.AppRoleId = approle.Id;

                        // await graphServiceClient.AppRoleAssignments.Request().AddAsync(approleAssignment);
                        var assignment = await graphServiceClient.ServicePrincipals[servicePrincipal.Id].AppRoleAssignedTo.Request().AddAsync(approleAssignment);
                        Console.WriteLine($"{assignment.PrincipalDisplayName} assigned to '{approle.DisplayName}' with id '{assignment.Id}' ");
                    }
                });
            }
            catch (Exception ex)
            {
                ColorConsole.WriteLine(ConsoleColor.Red, $"{ex}");
                throw;
            }

            ColorConsole.WriteLine(ConsoleColor.Green, "All app role assignments complete");
        }

        private static async Task<Beta.Application> CreateApplicationAsync(Beta.GraphServiceClient graphServiceClient)
        {
            Beta.Application application = new Beta.Application() { };

            application.DisplayName = "My app roles demo";

            application.Web = new Beta.WebApplication();
            application.Web.HomePageUrl = "https://localhost:44321/";
            application.Web.LogoutUrl = "https://localhost:44321/signout-oidc";
            application.Web.ImplicitGrantSettings = new Beta.ImplicitGrantSettings()
            { EnableIdTokenIssuance = true };

            IList<String> redirectUris = new List<string>() { "https://localhost:44321/", "https://localhost:44321/signin-oidc" };
            application.Web.RedirectUris = redirectUris;

            application.SignInAudience = "AzureADMyOrg";
            //application.IsFallbackPublicClient = true;

            IList<String> identifierUris = new List<string>() { $"https://kkaad.onmicrosoft.com/{application.DisplayName.Replace(" ", "")}" };
            application.IdentifierUris = identifierUris;

            application.Api = new Beta.ApiApplication();
            application.Api.RequestedAccessTokenVersion = 2;

            IList<Beta.PermissionScope> oauth2PermissionScopes = new List<Beta.PermissionScope>();
            oauth2PermissionScopes.Add(new Beta.PermissionScope()
            {
                Id = Guid.NewGuid(),
                IsEnabled = true,
                Type = "User",
                Value = "access_as_user",
                AdminConsentDisplayName = $"Access {application.DisplayName}",
                AdminConsentDescription = $"Allows the app to have the same access to information in the directory on behalf of the signed-in user.",
                UserConsentDisplayName = $"Access {application.DisplayName}",
                UserConsentDescription = $"Allow the application to access {application.DisplayName} on your behalf."
            });

            oauth2PermissionScopes.Add(new Beta.PermissionScope()
            {
                Id = Guid.NewGuid(),
                IsEnabled = true,
                Type = "Admin",
                Value = "user_impersonation",
                AdminConsentDisplayName = $"Access {application.DisplayName} as the signed-in user",
                AdminConsentDescription = $"Allows the app to have the same access to information in the directory on behalf of the signed-in user.",
                UserConsentDisplayName = $"Access {application.DisplayName} as the signed-in user",
                UserConsentDescription = $"Allow the application to access {application.DisplayName} on your behalf."
            });

            application.Api.Oauth2PermissionScopes = oauth2PermissionScopes;

            // Pre-authorized Apps
            IList<Beta.PreAuthorizedApplication> preAuthorizedApplications = new List<Beta.PreAuthorizedApplication>();
            IList<String> permissionIds = new List<String>();
            oauth2PermissionScopes.ToList().ForEach(x => permissionIds.Add(x.Id.ToString()));

            List<Beta.Application> applications = await GetAllApplicationsAsync(graphServiceClient);
            preAuthorizedApplications.Add(new Beta.PreAuthorizedApplication()
            {
                AppId = applications.ElementAtOrDefault(new System.Random().Next() % applications.Count()).AppId,
                PermissionIds = permissionIds
            });

            application.Api.PreAuthorizedApplications = preAuthorizedApplications;
            application.Tags = new List<string>() { "HooHoo", "HaaHaa" };

            // App owners
            IList<IApplicationOwnersCollectionWithReferencesPage> owners = new List<IApplicationOwnersCollectionWithReferencesPage>();

            // Required resource Access
            IList<Beta.RequiredResourceAccess> requiredResourceAccesses = new List<Beta.RequiredResourceAccess>();

            // App permissions
            requiredResourceAccesses.Add(await GetApplicationRolesByValueAsync(graphServiceClient, "Microsoft Graph", new List<string>() { "Directory.AccessAsUser.All", "Directory.ReadWrite.All", "Contacts.ReadWrite", "AppRoleAssignment.ReadWrite.All" }));
            requiredResourceAccesses.Add(await GetApplicationRolesByValueAsync(graphServiceClient, "Microsoft Intune API", new List<string>() { "get_data_warehouse", "send_data_usage", "update_device_health" }));
            requiredResourceAccesses.Add(await GetApplicationRolesByValueAsync(graphServiceClient, "Skype for Business Online", new List<string>() { "Meetings.JoinManage", "Meetings.ScheduleOnDemand" }));

            // TODO: Scopes
            //requiredResourceAccesses.Add(await GetApplicationScopesByValueAsync(graphServiceClient, "Microsoft Graph", new List<string>() { "User.Read", "User.ReadWrite.All" }));
            //requiredResourceAccesses.Add(await GetApplicationScopesByValueAsync(graphServiceClient, "Azure Service Management", new List<string>() { "user_impersonation" }));

            application.RequiredResourceAccess = requiredResourceAccesses;

            // Create app roles to assign users to
            Beta.AppRole viewersrole = new Beta.AppRole() { AllowedMemberTypes = new List<String>() { "User" }, DisplayName = "Viewers", Value = "Viewers", Description = "Users in this role have the permission to read data", Id = Guid.NewGuid(), IsEnabled = true };
            Beta.AppRole adminRole = new Beta.AppRole() { AllowedMemberTypes = new List<String>() { "User" }, Value = "Admins", DisplayName = "Admins", Description = "Users in the admin role have the permission to both read and write data", Id = Guid.NewGuid(), IsEnabled = true };

            // Create application permission
            Beta.AppRole accessAsApplication = new Beta.AppRole() { AllowedMemberTypes = new List<String>() { "Application" }, Value = "access_As_Application", DisplayName = $"Access {application.DisplayName} as an application", Description = "Access {application.DisplayName} as an application", Id = Guid.NewGuid(), IsEnabled = true };

            IList<Beta.AppRole> approles = new List<Beta.AppRole>() { viewersrole, adminRole, accessAsApplication };
            application.AppRoles = approles;

            // Not allowed
            // IList<Beta.PasswordCredential> passwordCredentials = new List<Beta.PasswordCredential>() { CreateAppKey(DateTime.Now, 99, ComputePassword()) };
            // application.PasswordCredentials = passwordCredentials;

            Beta.Application createdApp = await graphServiceClient.Applications.Request().AddAsync(application);

            if (createdApp != null)
            {
                // Not supported
                // Beta.PasswordCredential credential = await graphServiceClient.Applications[createdApp.Id].AddPassword(CreateAppKey(DateTime.Now, 99, ComputePassword())).Request().PostAsync();
                Beta.PasswordCredential credential = await graphServiceClient.Applications[createdApp.Id].AddPassword().Request().PostAsync();

                if (credential != null)
                {
                    Console.WriteLine($"New Credential: DisplayName -{credential.DisplayName}, CustomKeyIdentifier-{credential.CustomKeyIdentifier}, " +
                        $"StartDateTime- {credential.StartDateTime}, EndDateTime-{credential.EndDateTime}, SecretText-{credential.SecretText}");

                    // Refresh the newly created app's instance
                    createdApp = await graphServiceClient.Applications[createdApp.Id].Request().GetAsync();
                }
            }

            // Create a service principal
            Beta.ServicePrincipal servicePrincipal = new Beta.ServicePrincipal()
            {
                AppId = createdApp.AppId,
                Tags = new List<string>() { "WindowsAzureActiveDirectoryIntegratedApp", "PooPoo" }
            };

            await graphServiceClient.ServicePrincipals.Request().AddAsync(servicePrincipal);

            return createdApp;
        }

        private static async Task DeleteApplicationAsync(Beta.Application application, Beta.GraphServiceClient graphServiceClient)
        {
            try
            {
                await graphServiceClient.Applications[application.Id].Request().DeleteAsync();
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not delete the application with Id-{application.Id}: {e}");
            }
        }

        private async static Task<Beta.RequiredResourceAccess> GetApplicationScopesByValueAsync(Beta.GraphServiceClient graphServiceClient, string apiDisplayName, IList<string> scopeValues)
        {
            Beta.RequiredResourceAccess requiredResourceAccess = null;

            // ResourceAppId of Microsoft Graph
            Beta.ServicePrincipal servicePrincipal = await GetServicePrincipalByAppDisplayNameAsync(graphServiceClient, apiDisplayName);

            if (servicePrincipal != null)
            {
                requiredResourceAccess = new Beta.RequiredResourceAccess() { ResourceAppId = servicePrincipal.AppId };
                IList<Beta.ResourceAccess> resourceAccesses = new List<Beta.ResourceAccess>();

                scopeValues.ToList().ForEach(scopeValue =>
                {
                    //Beta.AppRole appRole = servicePrincipal.oAuth2PermissionScopes.Where(x => x.Value == roleValue).FirstOrDefault();

                    //if (appRole != null)
                    //{
                    //    resourceAccesses.Add(new Beta.ResourceAccess() { Type = "Role", Id = appRole.Id });
                    //}

                    //resourceAccesses.Add(new Beta.ResourceAccess() { Type = "Scope", Id = appRole.Id });
                });

                requiredResourceAccess.ResourceAccess = resourceAccesses;
            }
            else
            {
                ColorConsole.WriteLine(ConsoleColor.Red, $"No service principal matching '{apiDisplayName}' found in the tenant");
            }

            return requiredResourceAccess;
        }

        private async static Task<Beta.RequiredResourceAccess> GetApplicationRolesByValueAsync(Beta.GraphServiceClient graphServiceClient, string apiDisplayName, IList<string> appRoleValues)
        {
            Beta.RequiredResourceAccess requiredResourceAccess = null;

            // ResourceAppId of Microsoft Graph
            Beta.ServicePrincipal servicePrincipal = await GetServicePrincipalByAppDisplayNameAsync(graphServiceClient, apiDisplayName);

            if (servicePrincipal != null)
            {
                requiredResourceAccess = new Beta.RequiredResourceAccess() { ResourceAppId = servicePrincipal.AppId };
                IList<Beta.ResourceAccess> resourceAccesses = new List<Beta.ResourceAccess>();

                appRoleValues.ToList().ForEach(roleValue =>
                {
                    Beta.AppRole appRole = servicePrincipal.AppRoles.Where(x => x.Value == roleValue).FirstOrDefault();

                    if (appRole != null)
                    {
                        resourceAccesses.Add(new Beta.ResourceAccess() { Type = "Role", Id = appRole.Id });
                    }
                });

                if (resourceAccesses.Count() > 0)
                {
                    requiredResourceAccess.ResourceAccess = resourceAccesses;
                }
            }
            else
            {
                ColorConsole.WriteLine(ConsoleColor.Red, $"No service principal matching '{apiDisplayName}' found in the tenant");
            }

            return requiredResourceAccess;
        }

        //private static async Task DeleteServicePrincipalAsync(Beta.GraphServiceClient graphServiceClient, Beta.Application application)
        //{
        //    try
        //    {
        //        await graphServiceClient.Users[userId].Request().DeleteAsync();
        //    }
        //    catch (ServiceException e)
        //    {
        //        Console.WriteLine($"We could not delete the user with Id-{userId}: {e}");
        //    }

        //}

        //private static async Task DeleteServicePrincipalAsync(Beta.GraphServiceClient graphServiceClient, Beta.Application application)
        //{
        //    try
        //    {
        //        await graphServiceClient.Users[userId].Request().DeleteAsync();
        //    }
        //    catch (ServiceException e)
        //    {
        //        Console.WriteLine($"We could not delete the user with Id-{userId}: {e}");
        //    }

        //}

        private static async Task<List<Beta.Application>> GetAllApplicationsAsync(Beta.GraphServiceClient graphServiceClient)
        {
            //// Supported and works
            //var a = await graphServiceClient.Me.AppRoleAssignments.Request().GetAsync();

            //// Not navigable today, thus no way to retrieve the assignments for a service principal or group
            //var b = await graphServiceClient.Groups.AppRoleAssignments.Request().GetAsync();
            //var c = await graphServiceClient.ServicePrincipals.AppRoleAssignments.Request().GetAsync();

            //// this throws the unsupported query error
            //var d = await graphServiceClient.AppRoleAssignments.Request().GetAsync()

            List<Beta.Application> allApplications = new List<Beta.Application>();

            try
            {
                Beta.IGraphServiceApplicationsCollectionPage applications = await graphServiceClient.Applications.Request().GetAsync();

                if (applications?.CurrentPage.Count > 0)
                {
                    foreach (Beta.Application application in applications)
                    {
                        allApplications.Add(application);
                    }
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not retrieve the applications: {e}");
                return null;
            }

            return allApplications;
        }

        private static async Task PrintApplicationDetailsAsync(Beta.Application application, Beta.GraphServiceClient graphServiceClient)
        {
            if (application != null)
            {
                UserOperations userOperations = new UserOperations(graphServiceClient);

                Console.WriteLine($"--------------------------------Application '{application.DisplayName}' start----------------------------------------");
                Console.WriteLine($"Id-{application.Id}, AppId- {application.AppId}, DisplayName-{application.DisplayName}, " +
                    $"SignInAudience-{application.SignInAudience}, " +
                    $"GroupMembershipClaims-{application?.GroupMembershipClaims}");

                if (application?.Owners?.Count > 0)
                {
                    Console.WriteLine("--------------------Owners-------------------");
                    foreach (var owner in application.Owners)
                    {
                        Beta.User userOwner = await userOperations.GetUserByIdAsync(owner);

                        PrintBetaUserDetails(userOwner);
                    }
                    Console.WriteLine("----------------------------------------------------------");
                }

                if (application?.AppRoles?.Count() > 0)
                {
                    Console.WriteLine("--------------------------AppRoles-------------------");
                    foreach (var appRole in application.AppRoles)
                    {
                        Console.WriteLine($"Id-{appRole.Id}, IsEnabled- {appRole.IsEnabled}, UserConsentDisplayName-{appRole.Value}, " +
                            $"AllowedMemberTypes- {String.Join(",", appRole.AllowedMemberTypes.ToList())}");
                    }
                    Console.WriteLine("----------------------------------------------------------");
                }

                if (application.Web != null)
                {
                    Console.WriteLine("--------------------------Web App-------------------");

                    Console.WriteLine("Redirect Uris");
                    if (application.Web.RedirectUris != null && application.Web.RedirectUris.Count() > 0)
                    {
                        application.Web.RedirectUris.ToList().ForEach(x => Console.WriteLine($"     {x}"));
                    }

                    Console.WriteLine($"    Oauth2AllowImplicitFlow-'{application.Web?.Oauth2AllowImplicitFlow}'");

                    if (application?.Web?.AdditionalData?.Count > 0)
                    {
                        Console.WriteLine("--------------------------Application.Web.AdditionalData start-------------------");
                        Console.WriteLine(application?.Web.AdditionalData.ToDebugString());
                        Console.WriteLine("--------------------------Application.Web.AdditionalData end-------------------");
                    }

                    Console.WriteLine("----------------------------------------------------------");
                }

                if (application.RequiredResourceAccess != null)
                {
                    if (application.RequiredResourceAccess.Count() > 0)
                    {
                        Console.WriteLine("--------------------------RequiredResourceAccess-------------------");

                        foreach (var requiredResourceAccess in application.RequiredResourceAccess)
                        {
                            string resourceappName = string.Empty;

                            // Search for service principal first
                            Beta.ServicePrincipal servicePrincipal = await GetServicePrincipalByAppIdAsync(graphServiceClient, requiredResourceAccess.ResourceAppId);

                            if (servicePrincipal == null)
                            {
                                Beta.Application resourceApplication = await GetApplicationByAppIdAsync(graphServiceClient, requiredResourceAccess.ResourceAppId);
                                resourceappName = resourceApplication.DisplayName;
                            }
                            else
                            {
                                resourceappName = servicePrincipal.DisplayName;
                            }
                            Console.WriteLine($"ResourceAppId-{requiredResourceAccess.ResourceAppId}, Resource-{resourceappName} ");

                            foreach (var resourceAccess in requiredResourceAccess.ResourceAccess)
                            {
                                Beta.AppRole role = servicePrincipal.AppRoles.FirstOrDefault(x => x.Id == resourceAccess.Id);

                                if (role != null)
                                {
                                    Console.WriteLine($"    Id-{resourceAccess.Id}, Value-{role.Value}, DisplayName-{role.DisplayName}, Origin-{role.Origin}, " +
                                        $"Type-{resourceAccess.Type} ");
                                }
                                else
                                {
                                    //Beta.OAuth2Permission oauth2Permission = servicePrincipal.Oauth2Permissions.FirstOrDefault(x => x.Id == resourceAccess.Id);

                                    //Console.WriteLine($"    Id-{resourceAccess.Id}, Value-{oauth2Permission.Value}, UserConsentDisplayName-{oauth2Permission.UserConsentDisplayName}, " +
                                    //    $"Origin-{oauth2Permission?.Origin}, Type-{oauth2Permission.Type} ");
                                }
                            }
                        }

                        Console.WriteLine("----------------------------------------------------------");
                    }
                }

                if (application?.IdentifierUris.ToList().Count > 0)
                {
                    Console.WriteLine("--------------------------Api-------------------");

                    foreach (var identifierUri in application.IdentifierUris)
                    {
                        Console.WriteLine($"    identifierUri-'{identifierUri}'");
                    }

                    Console.WriteLine($"    RequestedAccessTokenVersion-'{application?.Api?.RequestedAccessTokenVersion}', AcceptMappedClaims - {application?.Api?.AcceptMappedClaims}");

                    foreach (var oauth2PermissionScope in application.Api.Oauth2PermissionScopes)
                    {
                        Console.WriteLine($"    Id-{oauth2PermissionScope.Id}, Type- {oauth2PermissionScope.Type}, " +
                            $"UserConsentDisplayName-{oauth2PermissionScope.UserConsentDisplayName}, AdminConsentDisplayName-{oauth2PermissionScope.AdminConsentDisplayName}, " +
                            $"IsEnabled-{oauth2PermissionScope.IsEnabled}");
                    }

                    foreach (var item in application.Api.KnownClientApplications)
                    {
                        Console.WriteLine("--------------------------KnownClientApplications-------------------");
                        application.Api.KnownClientApplications.ToList().ForEach(pz => Console.WriteLine($"KCA-{pz}"));
                        Console.WriteLine("----------------------------------------------------------");
                    }

                    foreach (var item in application.Api.PreAuthorizedApplications)
                    {
                        Console.WriteLine("--------------------------PreAuthorizedApplications-------------------");
                        foreach (var preAuthorizedApplication in application.Api.PreAuthorizedApplications)
                        {
                            Console.WriteLine($"AppId-{preAuthorizedApplication.AppId}");

                            preAuthorizedApplication.PermissionIds.ToList().ForEach(pz => Console.WriteLine($"Pid-{pz}"));
                        }
                        Console.WriteLine("----------------------------------------------------------");
                    }

                    if (application?.Api?.AdditionalData.Count > 0)
                    {
                        Console.WriteLine("--------------------------Application.Api.AdditionalData start-------------------");
                        Console.WriteLine(application?.Api?.AdditionalData.ToDebugString());
                        Console.WriteLine("--------------------------Application.Api.AdditionalData end-------------------");
                    }

                    Console.WriteLine("----------------------------------------------------------");
                }

                if (application?.KeyCredentials?.Count() > 0)
                {
                    Console.WriteLine("--------------------------KeyCredentials-------------------");
                    foreach (var keyCredential in application.KeyCredentials)
                    {
                        Console.WriteLine($"DisplayName-{keyCredential?.DisplayName}, KeyId- {keyCredential.KeyId}, StartDateTime- {keyCredential.StartDateTime}, EndDateTime- {keyCredential.EndDateTime} "
                            + $"Key-{keyCredential.Key}, Type-{keyCredential.Type}, Usage-{keyCredential.Usage}");
                    }
                    Console.WriteLine("----------------------------------------------------------");
                }

                if (application?.PasswordCredentials?.Count() > 0)
                {
                    Console.WriteLine("--------------------------PasswordCredentials-------------------");
                    foreach (var passwordCredential in application.PasswordCredentials)
                    {
                        Console.WriteLine($"DisplayName-{passwordCredential?.DisplayName}, KeyId- {passwordCredential.KeyId}, StartDateTime- {passwordCredential.StartDateTime}, EndDateTime- {passwordCredential.EndDateTime} "
                            + $"Hint-{passwordCredential.Hint}, SecretText-{passwordCredential.SecretText}, Hint-{passwordCredential?.Hint}");
                    }
                    Console.WriteLine("----------------------------------------------------------");
                }

                if (application?.OptionalClaims?.AccessToken.Count() > 0)
                {
                    Console.WriteLine("--------------------------OptionalClaims.AccessToken-------------------");
                    foreach (var optionalClaim in application.OptionalClaims.AccessToken)
                    {
                        Console.WriteLine($"Name-{optionalClaim.Name}, Source- {optionalClaim.Source}, Essential- {optionalClaim.Essential}");
                    }
                    Console.WriteLine("----------------------------------------------------------");
                }

                if (application?.OptionalClaims?.IdToken.Count() > 0)
                {
                    Console.WriteLine("--------------------------OptionalClaims.IdToken-------------------");
                    foreach (var optionalClaim in application.OptionalClaims.IdToken)
                    {
                        Console.WriteLine($"Name-{optionalClaim.Name}, Source- {optionalClaim.Source}, Essential- {optionalClaim.Essential}");
                    }
                    Console.WriteLine("----------------------------------------------------------");
                }

                if (application.Tags != null && application.Tags.Count() > 0)
                {
                    Console.WriteLine("--------------------------Tags-------------------");

                    application.Tags.ToList().ForEach(x => Console.WriteLine($"{x}"));

                    Console.WriteLine("----------------------------------------------------------");
                }

                if (application?.AdditionalData.Count > 0)
                {
                    Console.WriteLine("--------------------------Application.AdditionalData start-------------------");
                    Console.WriteLine(application?.AdditionalData.ToDebugString());
                    Console.WriteLine("--------------------------Application.AdditionalData end-------------------");
                }

                Console.WriteLine($"--------------------------------Application '{application.DisplayName}' end----------------------------------------");
                Console.WriteLine("");
            }
            else
            {
                Console.WriteLine("The provided Application is null!");
            }
        }

        private static async Task PrintServicePrincipalDetailsAsync(Beta.Application application, Beta.GraphServiceClient graphServiceClient)
        {
            Console.WriteLine("");
            Console.WriteLine($"--------------------------------ServicePrincipal '{application.DisplayName}' start----------------------------------------");

            Beta.ServicePrincipal servicePrincipal = await GetServicePrincipalByAppIdAsync(graphServiceClient, application.AppId);

            if (servicePrincipal == null)
            {
                ColorConsole.WriteLine(ConsoleColor.Red, $"NO SERVICE PRINCIPAL FOR '{application.DisplayName}' FOUND !!");
            }
            else
            {
                GroupOperations groupOperations = new GroupOperations(graphServiceClient);
                UserOperations userOperations = new UserOperations(graphServiceClient);

                Console.WriteLine($"Id-{servicePrincipal.Id}, Enabled- {servicePrincipal.AccountEnabled}, AppDisplayName-{servicePrincipal.AppDisplayName}, AppId-{servicePrincipal.AppId}, " +
                $"AppOwnerOrganizationId-{servicePrincipal.AppOwnerOrganizationId}, AppRoleAssignmentRequired-{servicePrincipal?.AppRoleAssignmentRequired}, " +
                $"DisplayName-{servicePrincipal.DisplayName}, Homepage-{servicePrincipal?.Homepage}PreferredTokenSigningKeyThumbprint-{servicePrincipal?.PreferredTokenSigningKeyThumbprint}, " +
                $"PublisherName-{servicePrincipal.PublisherName}, Homepage-{servicePrincipal?.Homepage}, PreferredTokenSigningKeyThumbprint-{servicePrincipal?.PreferredTokenSigningKeyThumbprint}");

                if (servicePrincipal?.Owners?.Count > 0)
                {
                    Console.WriteLine("--------------------Owners-------------------");
                    foreach (var owner in servicePrincipal.Owners)
                    {
                        Beta.User userOwner = await userOperations.GetUserByIdAsync(owner);

                        PrintBetaUserDetails(userOwner);
                    }
                    Console.WriteLine("----------------------------------------------------------");
                }

                Dictionary<Guid?, Beta.AppRole> approles = new Dictionary<Guid?, Beta.AppRole>();

                if (servicePrincipal?.AppRoles?.Count() > 0)
                {
                    Console.WriteLine("--------------------------AppRoles-------------------");
                    foreach (var appRole in servicePrincipal.AppRoles)
                    {
                        approles.Add(appRole.Id, appRole);
                        Console.WriteLine($"Id-{appRole.Id}, IsEnabled- {appRole.IsEnabled}, UserConsentDisplayName-{appRole.Value}, AllowedMemberTypes- {String.Join(",", appRole.AllowedMemberTypes)}");
                    }
                    Console.WriteLine("----------------------------------------------------------");
                }

                var approleassignments = await GetServicePrincipalsAppRoleAssignedToAsync(graphServiceClient, servicePrincipal);

                if (approleassignments?.Count > 0)
                {
                    Console.WriteLine("--------------------AppRole Assignments-------------------");
                    foreach (var approleassignment in approleassignments)
                    {
                        Console.WriteLine($"PrincipalDisplayName - '{approleassignment.PrincipalDisplayName}'" +
                            $", AppRole- '{approles[approleassignment.AppRoleId].DisplayName}'" +
                            $", PrincipalType- '{approleassignment.PrincipalType}'");
                    }
                    Console.WriteLine("----------------------------------------------------------");
                }

                var applicationAssignedTo = await GetServicePrincipalsAppRoleAssignmentsAsync(graphServiceClient, servicePrincipal);

                if (applicationAssignedTo != null && applicationAssignedTo.Count() > 0)
                {
                    Console.WriteLine("--------------------------Apps assigned to-------------------");

                    applicationAssignedTo.ToList().ForEach(x => Console.WriteLine($"AppRole-'{approles[x.AppRoleId].DisplayName}', Principal-{x.PrincipalDisplayName}," +
                        $", PrincipalType- '{x.PrincipalType}'"));

                    Console.WriteLine("----------------------------------------------------------");
                }

                if (servicePrincipal.ReplyUrls != null && servicePrincipal.ReplyUrls.Count() > 0)
                {
                    Console.WriteLine("--------------------------ReplyUrls-------------------");

                    servicePrincipal.ReplyUrls.ToList().ForEach(x => Console.WriteLine($"{x}"));

                    Console.WriteLine("----------------------------------------------------------");
                }

                if (servicePrincipal.ServicePrincipalNames != null && servicePrincipal.ReplyUrls.Count() > 0)
                {
                    Console.WriteLine("--------------------------ServicePrincipalNames-------------------");

                    servicePrincipal.ServicePrincipalNames.ToList().ForEach(x => Console.WriteLine($"{x}"));

                    Console.WriteLine("----------------------------------------------------------");
                }

                if (servicePrincipal.Tags != null && servicePrincipal.Tags.Count() > 0)
                {
                    Console.WriteLine("--------------------------Tags-------------------");

                    servicePrincipal.Tags.ToList().ForEach(x => Console.WriteLine($"{x}"));

                    Console.WriteLine("----------------------------------------------------------");
                }

                if (servicePrincipal.MemberOf != null && servicePrincipal.MemberOf.Count() > 0)
                {
                    Console.WriteLine("--------------------------MemberOf Group-------------------");

                    foreach (var group in servicePrincipal.MemberOf)
                    {
                        Beta.Group adGroup = await groupOperations.GetGroupByIdAsync(group);

                        Console.WriteLine($"    DisplayName-'{adGroup.DisplayName}'");
                    }

                    Console.WriteLine("----------------------------------------------------------");
                }

                if (servicePrincipal.TransitiveMemberOf != null && servicePrincipal.TransitiveMemberOf.Count() > 0)
                {
                    Console.WriteLine("--------------------------TransitiveMemberOf Group-------------------");

                    foreach (var group in servicePrincipal.TransitiveMemberOf)
                    {
                        Beta.Group adGroup = await groupOperations.GetGroupByIdAsync(group);

                        Console.WriteLine($"    DisplayName-'{adGroup.DisplayName}'");
                    }

                    Console.WriteLine("----------------------------------------------------------");
                }

                if (servicePrincipal?.Oauth2PermissionGrants?.Count() > 0)
                {
                    Console.WriteLine("--------------------------PublishedPermissionScopes-------------------");
                    foreach (var oAuth2Permission in servicePrincipal.PublishedPermissionScopes)
                    {
                        Console.WriteLine($"Id-{oAuth2Permission?.Id}, IsEnabled- {oAuth2Permission.IsEnabled}, Origin- {oAuth2Permission.Origin}, Type- {oAuth2Permission.Type} "
                            + $"UserConsentDescription-{oAuth2Permission.UserConsentDescription}, UserConsentDisplayName-{oAuth2Permission.UserConsentDisplayName}, Value-{oAuth2Permission.Value}");
                    }
                    Console.WriteLine("----------------------------------------------------------");
                }

                if (servicePrincipal?.Oauth2PermissionGrants?.Count() > 0)
                {
                    Console.WriteLine("--------------------------Oauth2PermissionGrants-------------------");
                    foreach (var oAuth2PermissionGrants in servicePrincipal.Oauth2PermissionGrants)
                    {
                        Beta.ServicePrincipal resourceServicePrincipal = await GetServicePrincipalByAppIdAsync(graphServiceClient, oAuth2PermissionGrants.ResourceId);

                        Console.WriteLine($"Resource Name-{resourceServicePrincipal.DisplayName}, Id-{oAuth2PermissionGrants?.Id}, PrincipalId- {oAuth2PermissionGrants.PrincipalId}, " +
                            $"ResourceId- {oAuth2PermissionGrants.ResourceId}, Scope- {oAuth2PermissionGrants.Scope}, ConsentType- {oAuth2PermissionGrants.ConsentType}  "
                            + $"StartTime-{oAuth2PermissionGrants.StartTime}, ExpiryTime-{oAuth2PermissionGrants.ExpiryTime}");
                    }

                    Console.WriteLine("----------------------------------------------------------");
                }

                if (servicePrincipal?.CreatedObjects?.Count() > 0)
                {
                    Console.WriteLine("--------------------------CreatedObjects-------------------");
                    foreach (var createdObjects in servicePrincipal.CreatedObjects)
                    {
                        Console.WriteLine($"Id-{createdObjects.Id}, DeletedDateTime- {createdObjects.DeletedDateTime}");
                    }
                    Console.WriteLine("----------------------------------------------------------");
                }

                if (servicePrincipal?.OwnedObjects?.Count() > 0)
                {
                    Console.WriteLine("--------------------------OwnedObjects-------------------");
                    foreach (var ownedObject in servicePrincipal.OwnedObjects)
                    {
                        Console.WriteLine($"Id-{ownedObject.Id}, DeletedDateTime- {ownedObject.DeletedDateTime}");
                    }
                    Console.WriteLine("----------------------------------------------------------");
                }

                if (servicePrincipal?.KeyCredentials?.Count() > 0)
                {
                    Console.WriteLine("--------------------------KeyCredentials-------------------");
                    foreach (var keyCredential in servicePrincipal.KeyCredentials)
                    {
                        Console.WriteLine($"DisplayName-{keyCredential?.DisplayName}, KeyId- {keyCredential.KeyId}, StartDateTime- {keyCredential.StartDateTime}, EndDateTime- {keyCredential.EndDateTime} "
                            + $"Key-{keyCredential.Key}, Type-{keyCredential.Type}, Usage-{keyCredential.Usage}");
                    }
                    Console.WriteLine("----------------------------------------------------------");
                }

                if (servicePrincipal?.PasswordCredentials?.Count() > 0)
                {
                    Console.WriteLine("--------------------------PasswordCredentials-------------------");
                    foreach (var passwordCredential in servicePrincipal.PasswordCredentials)
                    {
                        Console.WriteLine($"DisplayName-{passwordCredential?.DisplayName}, KeyId- {passwordCredential.KeyId}, StartDateTime- {passwordCredential.StartDateTime}, EndDateTime- {passwordCredential.EndDateTime} "
                            + $"Hint-{passwordCredential.Hint}, SecretText-{passwordCredential.SecretText}, Hint-{passwordCredential?.Hint}");
                    }
                    Console.WriteLine("----------------------------------------------------------");
                }

                if (servicePrincipal?.AdditionalData.Count > 0)
                {
                    Console.WriteLine("--------------------------servicePrincipal.AdditionalData start-------------------");
                    Console.WriteLine(servicePrincipal?.AdditionalData.ToDebugString());
                    Console.WriteLine("--------------------------servicePrincipal.AdditionalData end-------------------");
                }

                await PrintServicePrincipalOAuth2PermissionGrantsAsync(servicePrincipal, graphServiceClient);
            }
            Console.WriteLine($"--------------------------------ServicePrincipal '{application.DisplayName}' end----------------------------------------");
        }

        private static async Task PrintServicePrincipalOAuth2PermissionGrantsAsync(Beta.ServicePrincipal servicePrincipal, Beta.GraphServiceClient graphServiceClient)
        {
            if (servicePrincipal != null)
            {
                UserOperations userOperations = new UserOperations(graphServiceClient);

                Console.WriteLine("");
                Console.WriteLine($"--------------------------------OAuth2PermissionGrants for '{servicePrincipal.DisplayName}' start----------------------------------------");

                try
                {
                    var OAuth2PermissionGrants = await graphServiceClient.Oauth2PermissionGrants.Request().Filter($"clientId eq '{servicePrincipal.Id}'").GetAsync();

                    if (OAuth2PermissionGrants != null)
                    {
                        do
                        {
                            // Page through results
                            foreach (var OAuth2PermissionGrant in OAuth2PermissionGrants.CurrentPage)
                            {
                                Console.WriteLine("-------------------------------");

                                Console.WriteLine($"ClientId:{OAuth2PermissionGrant.ClientId}, ConsentType:{OAuth2PermissionGrant.ConsentType}, Scope:{OAuth2PermissionGrant.Scope}" +
                                    $"PrincipalId:{OAuth2PermissionGrant.PrincipalId}, ResourceId:{OAuth2PermissionGrant.ResourceId}, " +
                                    $"StartTime:{OAuth2PermissionGrant.StartTime}, ExpiryTime:{OAuth2PermissionGrant.ExpiryTime}");

                                Beta.ServicePrincipal resourceServicePrincipal = await GetServicePrincipalByIdAsync(graphServiceClient, OAuth2PermissionGrant.ResourceId);

                                if (resourceServicePrincipal != null)
                                {
                                    Console.WriteLine($"Resource Name-{resourceServicePrincipal.DisplayName}, Scope:{OAuth2PermissionGrant.Scope}");

                                    if (OAuth2PermissionGrant.ConsentType == "AllPrincipals")
                                    {
                                        Console.WriteLine($"has been granted by Admin consent");
                                    }
                                    else
                                    {
                                        Beta.User grantPrincipal = await userOperations.GetUserByIdAsync(OAuth2PermissionGrant.PrincipalId);

                                        if (grantPrincipal != null)
                                        {
                                            Console.WriteLine($"Granted to -{grantPrincipal.DisplayName}");
                                        }
                                        else
                                        {
                                            Console.WriteLine($"PrincipalId:{grantPrincipal.Id} is an orphan user in this tenant");
                                        }
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"ResourceId:{OAuth2PermissionGrant.ResourceId} is orphan resource in this tenant");
                                }

                                Console.WriteLine("-------------------------------");

                                if (servicePrincipal?.AdditionalData.Count > 0)
                                {
                                    Console.WriteLine(servicePrincipal?.AdditionalData.ToDebugString());
                                }
                            }

                            // are there more pages (Has a @odata.nextLink ?)
                            if (OAuth2PermissionGrants.NextPageRequest != null)
                            {
                                OAuth2PermissionGrants = await OAuth2PermissionGrants.NextPageRequest.GetAsync();
                            }
                            else
                            {
                                OAuth2PermissionGrants = null;
                            }
                        } while (OAuth2PermissionGrants != null);
                    }
                }
                catch (ServiceException e)
                {
                    Console.WriteLine($"We could not retrieve the user's list: {e}");
                }
            }
            else
            {
                Console.WriteLine("The provided ServicePrincipal is null!");
            }

            Console.WriteLine($"--------------------------------OAuth2PermissionGrants for '{servicePrincipal.DisplayName}' start----------------------------------------");
        }

        private static async Task<Beta.Application> GetApplicationByAppIdAsync(Beta.GraphServiceClient graphServiceClient, string appId)
        {
            var applications = await graphServiceClient.Applications.Request().Filter($"appId eq '{appId}'").GetAsync();
            //Request.Header("Prefer","outlook.body-content-type=\"text\"")
            return applications.FirstOrDefault();
        }

        private static async Task<Beta.ServicePrincipal> GetServicePrincipalByAppIdAsync(Beta.GraphServiceClient graphServiceClient, string appId)
        {
            var servicePrincipals = await graphServiceClient.ServicePrincipals.Request().Filter($"appId eq '{appId}'").GetAsync();
            return servicePrincipals.FirstOrDefault();
        }

        private static async Task<Beta.ServicePrincipal> GetServicePrincipalByAppDisplayNameAsync(Beta.GraphServiceClient graphServiceClient, string appDisplayName)
        {
            var servicePrincipals = await graphServiceClient.ServicePrincipals.Request().Filter($"displayName eq '{appDisplayName}'").GetAsync();
            return servicePrincipals.FirstOrDefault();
        }

        private static async Task<Beta.ServicePrincipal> GetServicePrincipalByIdAsync(Beta.GraphServiceClient graphServiceClient, string Id)
        {
            var servicePrincipals = await graphServiceClient.ServicePrincipals.Request().Filter($"id eq '{Id}'").GetAsync();
            return servicePrincipals.FirstOrDefault();
        }

        private static void PrintAppRoleAssignment(Beta.AppRoleAssignment assignment)
        {
            if (assignment != null)
            {
                Console.WriteLine($"AppRoleId-{assignment.AppRoleId}, PrincipalDisplayName- {assignment.PrincipalDisplayName}, " +
                    $"PrincipalType-{assignment.PrincipalType}, ResourceDisplayName-{assignment.ResourceDisplayName}, ResourceId-{assignment.ResourceId}, " +
                    $"PrincipalId-{assignment.PrincipalId}, Id-{assignment.Id}");
            }
            else
            {
                Console.WriteLine("The provided AppRoleAssignment is null!");
            }
        }

        private static async Task<List<Beta.AppRoleAssignment>> GetUsersAppRoleAssignmentsAsync(Beta.GraphServiceClient graphServiceClient)
        {
            List<Beta.AppRoleAssignment> allAssignments = new List<Beta.AppRoleAssignment>();

            try
            {
                Beta.IUserAppRoleAssignmentsCollectionPage assignments = await graphServiceClient.Me.AppRoleAssignments.Request().GetAsync();

                //Beta.IUserAppRoleAssignmentsCollectionPage assignments = await graphServiceClient.Me.AppRoleAssignments  .Request().GetAsync();

                if (assignments?.CurrentPage.Count > 0)
                {
                    foreach (Beta.AppRoleAssignment appRoleAssignment in assignments)
                    {
                        allAssignments.Add(appRoleAssignment);
                    }
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not retrieve the role assignments: {e}");
                return null;
            }

            return allAssignments;
        }

        /// <summary>
        /// Applications that the service principal is assigned to. Read-only.
        /// </summary>
        /// <param name="graphServiceClient">The graph service client.</param>
        /// <param name="servicePrincipal">The service principal.</param>
        /// <returns></returns>
        private static async Task<List<Beta.AppRoleAssignment>> GetServicePrincipalsAppRoleAssignedToAsync(Beta.GraphServiceClient graphServiceClient, Beta.ServicePrincipal servicePrincipal)
        {
            List<Beta.AppRoleAssignment> allAssignments = new List<Beta.AppRoleAssignment>();

            try
            {
                var approleAssignedToPages = await graphServiceClient.ServicePrincipals[servicePrincipal.Id].AppRoleAssignedTo.Request().GetAsync();

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
                Console.WriteLine($"We could not retrieve the role assigned to: {e}");
                return null;
            }

            return allAssignments;
        }

        /// <summary>
        /// Users and groups assigned in AppRoles in this service Principal
        /// </summary>
        /// <param name="graphServiceClient">The graph service client.</param>
        /// <param name="servicePrincipal">The service principal.</param>
        /// <returns></returns>
        private static async Task<List<Beta.AppRoleAssignment>> GetServicePrincipalsAppRoleAssignmentsAsync(Beta.GraphServiceClient graphServiceClient, Beta.ServicePrincipal servicePrincipal)
        {
            List<Beta.AppRoleAssignment> allAssignments = new List<Beta.AppRoleAssignment>();

            try
            {
                var approleAssignmentPages = await graphServiceClient.ServicePrincipals[servicePrincipal.Id].AppRoleAssignments.Request().GetAsync();

                if (approleAssignmentPages != null)
                {
                    do
                    {
                        // Page through results
                        foreach (var user in approleAssignmentPages.CurrentPage)
                        {
                            allAssignments.Add(user);
                        }

                        // are there more pages (Has a @odata.nextLink ?)
                        if (approleAssignmentPages.NextPageRequest != null)
                        {
                            approleAssignmentPages = await approleAssignmentPages.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            approleAssignmentPages = null;
                        }
                    } while (approleAssignmentPages != null);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not retrieve the role assigned to: {e}");
                return null;
            }

            return allAssignments;
        }

        /// <summary>
        /// Applications that the service principal OAuth2PermissionGrants
        /// </summary>
        /// <param name="graphServiceClient">The graph service client.</param>
        /// <param name="servicePrincipal">The service principal.</param>
        /// <returns></returns>
        private static async Task<List<Beta.OAuth2PermissionGrant>> GetServicePrincipalsOauth2PermissionGrantsAsync(Beta.GraphServiceClient graphServiceClient, Beta.ServicePrincipal servicePrincipal)
        {
            List<Beta.OAuth2PermissionGrant> alloauth2PermissionGrants = new List<Beta.OAuth2PermissionGrant>();

            try
            {
                var OAuth2PermissionGrantsPages = await graphServiceClient.ServicePrincipals[servicePrincipal.Id].Oauth2PermissionGrants.Request().GetAsync();

                if (OAuth2PermissionGrantsPages != null)
                {
                    do
                    {
                        // Page through results
                        foreach (var grant in OAuth2PermissionGrantsPages.CurrentPage)
                        {
                            alloauth2PermissionGrants.Add(grant);
                        }

                        // are there more pages (Has a @odata.nextLink ?)
                        if (OAuth2PermissionGrantsPages.NextPageRequest != null)
                        {
                            OAuth2PermissionGrantsPages = await OAuth2PermissionGrantsPages.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            OAuth2PermissionGrantsPages = null;
                        }
                    } while (OAuth2PermissionGrantsPages != null);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not retrieve the permissions grants: {e}");
                return null;
            }

            return alloauth2PermissionGrants;
        }

        #region User

        private static async Task<User> CreateUserAsync(GraphServiceClient graphServiceClient)
        {
            User newUserObject = null;

            string displayname = $"{givenName} {surname}";
            string mailNickName = $"{givenName}{surname}";
            string upn = $"{mailNickName}@kkaad.onmicrosoft.com";
            string password = "p@$$w0rd!";

            try
            {
                newUserObject = await graphServiceClient.Users.Request().AddAsync(new User
                {
                    AccountEnabled = true,
                    DisplayName = displayname,
                    MailNickname = mailNickName,
                    GivenName = givenName,
                    Surname = surname,
                    PasswordProfile = new PasswordProfile
                    {
                        Password = password
                    },
                    UserPrincipalName = upn
                });
            }
            catch (ServiceException e)
            {
                Console.WriteLine("We could not add a new user: " + e.Error.Message);
                return null;
            }

            return newUserObject;
        }

        private static void PrintUserDetails(User user)
        {
            if (user != null)
            {
                Console.WriteLine($"DisplayName-{user.DisplayName}, MailNickname- {user.MailNickname}, GivenName-{user.GivenName}, Surname-{user.Surname}, Upn-{user.UserPrincipalName}, JobTitle-{user.JobTitle}, Id-{user.Id}");
            }
            else
            {
                Console.WriteLine("The provided User is null!");
            }
        }

        private static void PrintBetaUserDetails(Beta.User user)
        {
            if (user != null)
            {
                Console.WriteLine($"DisplayName-{user.DisplayName}, MailNickname- {user.MailNickname}, GivenName-{user.GivenName}, Surname-{user.Surname}, Upn-{user.UserPrincipalName}, JobTitle-{user.JobTitle}, Id-{user.Id}");
            }
            else
            {
                Console.WriteLine("The provided User is null!");
            }
        }

        private static async Task<User> UpdateUserAsync(GraphServiceClient graphServiceClient, string userId, string jobTitle)
        {
            User updatedUser = null;
            try
            {
                // Update the user.
                updatedUser = await graphServiceClient.Users[userId].Request().UpdateAsync(new User
                {
                    JobTitle = jobTitle
                });
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not update details of the user with Id {userId}: {e}");
            }

            return updatedUser;
        }

        private static async Task DeleteUserAsync(GraphServiceClient graphServiceClient, string userId)
        {
            try
            {
                await graphServiceClient.Users[userId].Request().DeleteAsync();
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not delete the user with Id-{userId}: {e}");
            }
        }

        #endregion User
    }
}