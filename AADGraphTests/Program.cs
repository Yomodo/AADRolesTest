extern alias BetaLib;

using Common;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
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
        private const string clientId = "10d8a46c-3059-4fe4-a779-c38415c04a4b";
        private const string tenant = "woodgrove.ms";
        private const string redirectUri = "msal10d8a46c-3059-4fe4-a779-c38415c04a4b://auth";

        // Change the following between each call to create/update user if not deleting the user
        private static string givenName = "test99";

        private static string surname = "user99";

        private static async Task Main(string[] args)
        {
            // Initialize and prepare MSAL
            string[] scopes = new string[] { "user.read", "user.readwrite.all", "Directory.AccessAsUser.All", "Directory.ReadWrite.All", 
                "Contacts.ReadWrite", "AppRoleAssignment.ReadWrite.All", "Policy.ReadWrite.ApplicationConfiguration" };

            IPublicClientApplication app = PublicClientApplicationBuilder.Create(clientId)
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{tenant}"))
                .WithRedirectUri(redirectUri)
                .Build();

            // Initialize the Graph SDK authentication provider
            InteractiveAuthenticationProvider authenticationProvider = new InteractiveAuthenticationProvider(app, scopes);
            GraphServiceClient graphServiceClient = new GraphServiceClient(authenticationProvider);

            Beta.GraphServiceClient betaClient = new Beta.GraphServiceClient(authenticationProvider);
            //Beta.ServicePrincipal graphServicePrincipal = GetServicePrincipalByAppDisplayNameAsync(betaClient, "Microsoft Graph").Result;

            #region ActivityBasedTimeoutPolicy

            //PolicyOperations policyOperations = new PolicyOperations(betaClient);

            //var activityBasedTimeoutPolicies = await policyOperations.ListActivityBasedTimeoutPoliciesAsync();
            //activityBasedTimeoutPolicies.ForEach(x => policyOperations.PrintActivityBasedTimeoutPolicy(x));

            #endregion ActivityBasedTimeoutPolicy

            #region groupSettings

            GroupSettingOperations groupSettingOperations = new GroupSettingOperations(graphServiceClient);

            Console.WriteLine("Fetching group settings templates");
            var groupSettingsTemplates = await groupSettingOperations.ListGroupSettingTemplatesAsync();

            Console.WriteLine("Printing group settings templates");
            groupSettingsTemplates.ForEach(t =>
            {
                Console.WriteLine("---------------------------------------------------------------------");
                groupSettingOperations.PrintGroupSettingTemplates(t);
                Console.WriteLine("---------------------------------------------------------------------");
            });

            Console.WriteLine("Fetching group settings ");
            var groupSettings = await groupSettingOperations.ListGroupSettingsAsync();

            Console.WriteLine("Printing group settings ");
            groupSettings.ForEach(async t =>
            {
                Console.WriteLine("---------------------------------------------------------------------");
                await groupSettingOperations.PrintGroupSettingsAsync(t);
                Console.WriteLine("---------------------------------------------------------------------");
            });

            //// WARNING: Cross check with the listed printed above before fiddling with the group settings

            //// Add one from "Prohibited Names Settings" template
            //var safeSettingsToAdd = new List<(string, string, string)>()
            //{
            //    ("80661d51-be2f-4d46-9713-98a2fcaec5bc","CustomBlockedSubStringsList","Kalyan, Krishna"),
            //    ("80661d51-be2f-4d46-9713-98a2fcaec5bc","CustomBlockedWholeWordsList","")
            //};

            //var safeSettingsToUpdate = new List<(string, string, string)>()
            //{
            //    ("80661d51-be2f-4d46-9713-98a2fcaec5bc","CustomBlockedSubStringsList","Kalyan, Krishna"),
            //    ("80661d51-be2f-4d46-9713-98a2fcaec5bc","CustomBlockedWholeWordsList","")
            //};
            //GroupSetting newGroupSetting = null;

            //try
            //{
            //    //Delete if existing
            //    await groupSettingOperations.DeleteGroupSettingAsync(await groupSettingOperations.GetGroupSettingByIdAsync("6b5c57a9-fa4d-45ce-a34c-e2a107d4fbfb"));

            //    Console.WriteLine("Adding a new group settings");

            //    IList<SettingValue> settings = new List<SettingValue>();
            //    safeSettingsToAdd.ForEach(s =>
            //    {
            //        settings.Add(new SettingValue() { Name = s.Item2, Value = s.Item3 });
            //    });

            //    newGroupSetting = new GroupSetting()
            //    {
            //        TemplateId = safeSettingsToAdd[0].Item1,
            //        DisplayName = "My custom display name",
            //        Values = settings
            //    };

            //    newGroupSetting = await groupSettingOperations.AddGroupSettingAsync(newGroupSetting);
            //    newGroupSetting = await groupSettingOperations.GetGroupSettingByIdAsync(newGroupSetting.Id);
            //    await groupSettingOperations.PrintGroupSettingsAsync(newGroupSetting);

            //    Console.WriteLine("Updating a group settings");

            //    GroupSetting updatedGroupSetting = await groupSettingOperations.UpdateGroupSettingAsync(newGroupSetting.Id, safeSettingsToUpdate[0].Item2, safeSettingsToUpdate[0].Item3);
            //    updatedGroupSetting = await groupSettingOperations.GetGroupSettingByIdAsync(newGroupSetting.Id);
            //    await groupSettingOperations.PrintGroupSettingsAsync(updatedGroupSetting);
            //}
            //catch (Exception ex)
            //{
            //    ColorConsole.WriteLine(ConsoleColor.Red, $"{ex}");
            //}
            //finally
            //{
            //    Console.WriteLine("Deleting a group settings");
            //    await groupSettingOperations.DeleteGroupSettingAsync(newGroupSetting);
            //}

            #endregion groupSettings

            #region Application operations

            //ApplicationOperations applicationOperations = new ApplicationOperations(betaClient);
            //UserOperations userOperations = new UserOperations(betaClient);

            //List<Beta.Application> applications = await applicationOperations.GetAllApplicationsAsync();
            //applications.ForEach(async (u) => await applicationOperations.PrintApplicationDetailsAsync(u));
            //Beta.Application rolesapp = applications.FirstOrDefault(x => x.DisplayName == "WebApp-RolesClaims");

            //IEnumerable<Beta.User> allUsersInTenant = await userOperations.GetUsersAsync();

            //Beta.Application newApp = await applicationOperations.CreateApplicationAsync(betaClient);

            //try
            //{
            //    await applicationOperations.PrintApplicationDetailsAsync(newApp);
            //    await applicationOperations.AssignUsersToAppRoles(newApp, allUsersInTenant.ToList());
            //    await applicationOperations.PrintServicePrincipalDetailsAsync(newApp);
            //    await applicationOperations.UpdateServicePrincipalSettings(newApp, allUsersInTenant);
            //    await applicationOperations.PrintServicePrincipalDetailsAsync(newApp);
            //}
            //catch (Exception ex)
            //{
            //    ColorConsole.WriteLine(ConsoleColor.Red, $"{ex}");
            //}
            //finally
            //{
            //    ColorConsole.WriteLine(ConsoleColor.Green, "Press any key to delete this app");
            //    Console.ReadKey();
            //    await applicationOperations.DeleteApplicationAsync(newApp, betaClient);
            //}

            #endregion Application operations

            #region appRoleAssignments

            //UserOperations userOperations = new UserOperations(betaClient);
            //List<Beta.AppRoleAssignment> usersApproleAssignments = await userOperations.GetUsersAppRoleAssignmentsAsync();
            //usersApproleAssignments.ForEach(u => userOperations.PrintAppRoleAssignment(u));

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

            //    IEnumerable<Beta.User> membersToAdd = GenericUtility<Beta.User>.GetaRandomNumberOfItemsFromList(allNonGuestUsersInTenant, 15);
            //    IEnumerable<Beta.User> ownersToAdd = GenericUtility<Beta.User>.GetaRandomNumberOfItemsFromList(allNonGuestUsersInTenant, 4);

            //    IEnumerable<Beta.User> ownersToUpdate = allNonGuestUsersInTenant.Except(ownersToAdd).Take(4);
            //    IEnumerable<Beta.User> membersToUpdate = allNonGuestUsersInTenant.Except(membersToAdd).Take(15);

            //    newGroup = await groupOperations.CreateUnifiedGroupAsync(tenant, membersToAdd, ownersToAdd);
            //    groupCreated = true;
            //    await groupOperations.PrintGroupDetails(newGroup, true);

            //    // Update List
            //    foreach (var owner in ownersToUpdate)
            //    {
            //        await groupOperations.AddOwnerToGroupAsync(newGroup, owner);
            //    }

            //    foreach (var member in membersToUpdate)
            //    {
            //        await groupOperations.AddMemberToGroup(newGroup, member);
            //    }

            //    await groupOperations.PrintGroupDetails(newGroup, true);

            //    //newGroup = await groupOperations.AllowExternalSendersAsync(newGroup);

            //    // Delete a bunch
            //    foreach (var owner in ownersToAdd)
            //    {
            //        await groupOperations.RemoveGroupOwnerAsync(newGroup, owner);
            //    }

            //    foreach (var member in membersToAdd)
            //    {
            //        await groupOperations.RemoveGroupMemberAsync(newGroup, member);
            //    }

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

            //GroupOperations groupOperations = new GroupOperations(betaClient);
            //UserOperations userOperations = new UserOperations(betaClient);
            //Beta.Group newGroup = null;
            //bool groupCreated = false;

            //try
            //{
            //    IEnumerable<Beta.User> allUsersInTenant = await userOperations.GetUsersAsync();
            //    IEnumerable<Beta.User> allNonGuestUsersInTenant = await userOperations.GetNonGuestUsersAsync();

            //    var signedInUser = await userOperations.GetMeAsync();

            //    IEnumerable<Beta.User> membersToAdd = GenericUtility<Beta.User>.GetaRandomNumberOfItemsFromList(allUsersInTenant, 5);
            //    // Remove the current user as they have been added as owner automatically
            //    IEnumerable<Beta.User> ownersToAdd = GenericUtility<Beta.User>.GetaRandomNumberOfItemsFromList(allNonGuestUsersInTenant.Except(new List<Beta.User> { signedInUser }), 2);

            //    newGroup = await groupOperations.CreateDistributionGroupAsync(tenant);
            //    groupCreated = true;

            //    await groupOperations.PrintGroupDetails(newGroup, true);

            //    // Add owners
            //    ownersToAdd.ToList().ForEach(y => AsyncHelper.RunSync(async () =>
            //        await groupOperations.AddOwnerToGroupAsync(newGroup, y)));

            //    // Add members
            //    membersToAdd.ToList().ForEach(y => AsyncHelper.RunSync(async () =>
            //        await groupOperations.AddMemberToGroup(newGroup, y)));

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

            #endregion Distribution Groups operations

            #region user operations

            //UserOperations userOperations = new UserOperations(betaClient);

            //// Get information from Graph about the currently signed-In user
            //Console.WriteLine("--Fetching details of the currently signed-in user--");
            //Beta.User currentUser = await userOperations.GetMeAsync();
            //userOperations.PrintBetaUserDetails(currentUser);
            //Console.WriteLine("---------");

            //// Create a new user
            //Console.WriteLine($"--Creating a new user in the tenant '{tenant}'--");
            //Beta.User newUser = await userOperations.CreateUserAsync(givenName, surname);
            //userOperations.PrintBetaUserDetails(newUser);
            //Console.WriteLine("---------");

            //// Update an existing user
            //if (newUser != null)
            //{
            //    Console.WriteLine("--Updating the detail of an existing user--");
            //    Beta.User updatedUser = await userOperations.UpdateUserAsync(userId: newUser.Id, jobTitle: "Program Manager");
            //    userOperations.PrintBetaUserDetails(updatedUser);
            //    Console.WriteLine("---------");
            //}

            //// List existing users
            //Console.WriteLine("--Listing all users in the tenant--");
            //List<Beta.User> users = await userOperations.GetUsersAsync();
            //users.ForEach(u => userOperations.PrintBetaUserDetails(u));
            //Console.WriteLine("---------");

            //// Delete this user
            //Console.WriteLine("--Deleting a user in the tenant--");
            //if (newUser != null)
            //{
            //    await userOperations.DeleteUserAsync(newUser?.Id);
            //}

            //Console.WriteLine("---------");

            //// List existing users after deletion
            //Console.WriteLine("--Listing all users in the tenant after deleting a user.--");
            //users = await userOperations.GetUsersAsync();
            //users.ForEach(u => userOperations.PrintBetaUserDetails(u));
            //Console.WriteLine("---------");

            #endregion user operations

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}