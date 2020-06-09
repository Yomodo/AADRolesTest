extern alias BetaLib;

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

namespace Common
{
    internal class Program
    {
        private const string clientId = "10d8a46c-3059-4fe4-a779-c38415c04a4b";
        private const string tenant = "woodgrove.ms";
        private const string redirectUri = "https://localhost";
        //private const string redirectUri = "msal10d8a46c-3059-4fe4-a779-c38415c04a4b://auth";

        // Change the following between each call to create/update user if not deleting the user
        private static string givenName = "test99";

        private static string surname = "user99";

        private static async Task Main(string[] args)
        {
            //MSALClient client = new MSALClient();
            //var token = await client.GetAuthenticationToken();
            //Console.WriteLine($"Token-{token}");

            //return;

            // Initialize and prepare MSAL
            string[] scopes = new string[] { "user.read", "user.readwrite.all", "Directory.AccessAsUser.All", "Directory.ReadWrite.All",
                "Contacts.ReadWrite", "AppRoleAssignment.ReadWrite.All","Group.ReadWrite.All" };

            IPublicClientApplication app = PublicClientApplicationBuilder.Create(clientId)
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{tenant}"))
                .WithRedirectUri(redirectUri)
                .Build();

            // Initialize the Graph SDK authentication provider
            InteractiveAuthenticationProvider authenticationProvider = new InteractiveAuthenticationProvider(app, scopes);
            GraphServiceClient graphServiceClient = new GraphServiceClient(authenticationProvider);

            Beta.GraphServiceClient betaClient = new Beta.GraphServiceClient(authenticationProvider);
            //Beta.ServicePrincipal graphServicePrincipal = GetServicePrincipalByAppDisplayNameAsync(betaClient, "Microsoft Graph").Result;

            #region Invitations API

            //UserOperations userOperations = new UserOperations(betaClient, "woodgrove.ms");
            //InvitationOperations invitationOperations = new InvitationOperations(betaClient);

            //Console.WriteLine("Sending invitation");
            //var invitation = await invitationOperations.SendInvitation("Kalyan", "krishna", "kalyankrishna1@gmail.com");

            //ColorConsole.WriteLine(ConsoleColor.Red, $"Invitation sent to user with redeem URL -{invitation.InviteRedeemUrl}, " +
            //    $"Status-{invitation.Status}, resetRedemption-{invitation?.ResetRedemption.Value}");

            //Beta.User inviteduser = await userOperations.GetUserByIdAsync(invitation.InvitedUser.Id);

            //if (inviteduser != null)
            //{
            //    ColorConsole.WriteLine(ConsoleColor.Green, userOperations.PrintBetaUserDetails(inviteduser));
            //    ColorConsole.WriteLine(ConsoleColor.Green, $"UserType-{inviteduser.UserType}, ExternalUserState-{inviteduser.ExternalUserState}, ExternalUserStateChangeDateTime-{inviteduser.ExternalUserStateChangeDateTime}");

            //    // Delete user
            //    Console.WriteLine("Deleting the invited user");
            //    await userOperations.DeleteUserAsync(inviteduser.Id);
            //    Console.WriteLine("User deleted successfully");
            //}

            #endregion Invitations API

            #region ActivityBasedTimeoutPolicy

            //PolicyOperations policyOperations = new PolicyOperations(betaClient);

            //var activityBasedTimeoutPolicies = await policyOperations.ListActivityBasedTimeoutPoliciesAsync();
            //activityBasedTimeoutPolicies.ForEach(x => policyOperations.PrintActivityBasedTimeoutPolicy(x));

            #endregion ActivityBasedTimeoutPolicy

            #region groupSettings

            //GroupSettingOperations groupSettingOperations = new GroupSettingOperations(graphServiceClient);

            //Console.WriteLine("Fetching group settings templates");
            //var groupSettingsTemplates = await groupSettingOperations.ListGroupSettingTemplatesAsync();

            //Console.WriteLine("Printing group settings templates");
            //groupSettingsTemplates.ForEach(async t =>
            //{
            //    Console.WriteLine("---------------------------------------------------------------------");
            //    await groupSettingOperations.PrintGroupSettingTemplates(t);
            //    Console.WriteLine("---------------------------------------------------------------------");
            //});

            //Console.WriteLine("Fetching group settings ");
            //var groupSettings = await groupSettingOperations.ListGroupSettingsAsync();

            //Console.WriteLine("Printing group settings ");
            //groupSettings.ForEach(async t =>
            //{
            //    Console.WriteLine("---------------------------------------------------------------------");
            //    await groupSettingOperations.PrintGroupSettingsAsync(t);
            //    Console.WriteLine("---------------------------------------------------------------------");
            //});

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

            #region Delta Groups operations

            GroupOperations groupOperations = new GroupOperations(betaClient);
            //UserOperations userOperations = new UserOperations(betaClient);

            //// Prepare to add a new group with members
            //IEnumerable<Beta.User> allUsersInTenant = await userOperations.GetUsersAsync();
            //IEnumerable<Beta.User> allNonGuestUsersInTenant = await userOperations.GetNonGuestUsersAsync();

            //var signedInUser = await userOperations.GetMeAsync();

            //IEnumerable<Beta.User> membersToAdd = GenericUtility<Beta.User>.GetaRandomNumberOfItemsFromList(allUsersInTenant, 5);

            //// Remove the current user as they have been added as owner automatically
            //// IEnumerable<Beta.User> ownersToAdd = GenericUtility<Beta.User>.GetaRandomNumberOfItemsFromList(allNonGuestUsersInTenant.Except(new List<Beta.User> { signedInUser }), 2);

            //IList<Beta.User> ownersToAdd = GenericUtility<Beta.User>.GetaRandomNumberOfItemsFromList(allNonGuestUsersInTenant.Except(new List<Beta.User> { signedInUser }), 2);

            //if (ownersToAdd.Where(x=> x.Id == signedInUser.Id).Count() == 0)
            //{
            //    ownersToAdd.Add(signedInUser);
            //}

            // Delta operations with groups
            var groupswithDelta = await groupOperations.ListGroupsForDeltaAsync(true);
            var groups = groupswithDelta.Item1;
            string deltaLink = groupswithDelta.Item2;

            ColorConsole.WriteLine(ConsoleColor.Green, $"Delta query fetched {groups.Count()} groups. Delta link is '{deltaLink}'");

            groups.ForEach(async group =>
            {
                Console.WriteLine(await groupOperations.PrintGroupDetails(group, false));
            });

            //// Add a new group
            //Beta.Group newGroup = null;
            //bool groupCreated = false;

            //try
            //{
            //    newGroup = await groupOperations.CreateUnifiedGroupAsync(tenant, membersToAdd, ownersToAdd);
            //    groupCreated = true;

            //    if (newGroup != null)
            //    {
            //        // Wait for group to be created
            //        Beta.Group grp = null;

            //        while (grp == null)
            //        {
            //            await Task.Delay(3000);
            //            ColorConsole.WriteLine(ConsoleColor.DarkGreen, $"Failed to pick details of the newly created group. Trying again.. ");
            //            grp = await groupOperations.GetGroupByIdAsync(newGroup.Id, true);
            //        }

            //        Console.WriteLine(await groupOperations.PrintGroupDetails(grp, true));

            //        // test delta changes
            //        #region now get changes since last delta sync


            //        Console.WriteLine("Press any key to execute delta query.");
            //        Console.ReadKey();
            //        Console.WriteLine("=== Getting delta changes....");

            //        /// Get the first page using the delta link (to see the new group)
            //        groupswithDelta = await groupOperations.ListGroupsForDeltaAsync(deltaLink);
            //        groups = groupswithDelta.Item1;
            //        string newDeltaLink = groupswithDelta.Item2;

            //        groups.ForEach(async group =>
            //        {
            //            Console.WriteLine(await groupOperations.PrintGroupDetails(group, true));
            //        });

            //        /// <summary>
            //        /// Display groups again and get NEW delta link... notice that only the added group is returned
            //        /// Keep trying (in case there are replication delays) to get changes
            //        /// </summary>
            //        while (deltaLink.Equals(newDeltaLink))
            //        {
            //            ColorConsole.WriteLine(ConsoleColor.DarkGreen, $"Failed to pick delta changes, trying again ");
            //            // If the two are equal, then we didn't receive changes yet query to get first page using the delta link
            //            groupswithDelta = await groupOperations.ListGroupsForDeltaAsync(deltaLink);
            //            groups = groupswithDelta.Item1;
            //            newDeltaLink = groupswithDelta.Item2;
            //        }

            //        //Printing group details picked from delta query
            //        ColorConsole.WriteLine(ConsoleColor.Green, $"Delta query # 2fetched {groups.Count()} groups. Delta link is '{deltaLink}'");
            //        groups.ForEach(async group =>
            //        {
            //            Console.WriteLine(await groupOperations.PrintGroupDetails(group, false, true));
            //        });

            //        #endregion now get changes since last delta sync
            //    }
            //    else
            //    {
            //        ColorConsole.WriteLine(ConsoleColor.Red, "Failed to create a group");
            //    }
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

            #endregion Delta Groups operations

            #region Dynamic groups operations

            //GroupOperations groupOperations = new GroupOperations(betaClient);

            //var dynamicGroups = await groupOperations.ListDynamicGroupsAsync();

            //ColorConsole.WriteLine(ConsoleColor.Green, $"Found {dynamicGroups.Count} dynamic groups. Listing");
            //dynamicGroups.ForEach(group => 
            //{
            //    ColorConsole.WriteLine(ConsoleColor.Yellow, groupOperations.PrintGroupBasic(group));
            //});

            //dynamicGroups = await groupOperations.ListDynamicGroupsAsync(true);

            //ColorConsole.WriteLine(ConsoleColor.Green, $"Found {dynamicGroups.Count} dynamic groups. Listing with members");
            //dynamicGroups.ForEach(async group =>
            //{
            //    Console.WriteLine(await groupOperations.PrintGroupDetails(group, true, true));
            //});

            // Create a new dynamic group

            //UserOperations userOperations = new UserOperations(betaClient);
            //var signedInUser = await userOperations.GetMeAsync();

            //IEnumerable<Beta.User> allNonGuestUsersInTenant = await userOperations.GetNonGuestUsersAsync();

            //// Remove the current user as they have been added as owner automatically
            //// IEnumerable<Beta.User> ownersToAdd = GenericUtility<Beta.User>.GetaRandomNumberOfItemsFromList(allNonGuestUsersInTenant.Except(new List<Beta.User> { signedInUser }), 2);

            //IList<Beta.User> ownersToAdd = GenericUtility<Beta.User>.GetaRandomNumberOfItemsFromList(allNonGuestUsersInTenant.Except(new List<Beta.User> { signedInUser }), 2);

            //if (ownersToAdd.Where(x => x.Id == signedInUser.Id).Count() == 0)
            //{
            //     ownersToAdd.Add(signedInUser);
            //}

            //// Add a new group
            //Beta.Group newGroup = null;
            //bool groupCreated = false;

            //try
            //{
            //    newGroup = await groupOperations.CreateDynamicGroupAsync(tenant, ownersToAdd);
            //    groupCreated = true;

            //    if (newGroup != null)
            //    {
            //        // Wait for group to be created
            //        Beta.Group grp = null;

            //        while (grp == null)
            //        {
            //            await Task.Delay(3000);
            //            ColorConsole.WriteLine(ConsoleColor.DarkGreen, $"Failed to pick details of the newly created dynamic group. Trying again.. ");
            //            grp = await groupOperations.GetGroupByIdAsync(newGroup.Id, true);
            //            if (grp != null)
            //            {
            //                Console.WriteLine($"\nMembershipRule-{grp.MembershipRule}, membershipRuleProcessingState-{grp.MembershipRuleProcessingState}, renewedDateTime-{grp.RenewedDateTime} ");
            //            }
            //        }

            //        // TODO: Check processing status

            //        Console.WriteLine(await groupOperations.PrintGroupDetails(grp, true, true));
            //    }
            //    else
            //    {
            //        ColorConsole.WriteLine(ConsoleColor.Red, "Failed to create a group");
            //    }

            //}
            //catch (Exception ex)
            //{
            //    ColorConsole.WriteLine(ConsoleColor.Red, $"{ex}");

            //}
            //finally
            //{
            //    if (groupCreated && newGroup != null)
            //    {
            //        ColorConsole.WriteLine(ConsoleColor.Green, "Press any key to delete this dynamic group");
            //        Console.ReadKey();
            //        await groupOperations.DeleteGroupAsync(newGroup);
            //    }
            //}

            #endregion Dynamic groups operations

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