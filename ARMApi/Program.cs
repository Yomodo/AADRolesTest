extern alias BetaLib;

using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
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
            string[] scopes = new string[] { "user.readbasic.all", "RoleManagement.ReadWrite.Directory", "AdministrativeUnit.Read.All" };

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

            UserOperations userOperations = new UserOperations(betaClient);
            ServicePrincipalOperations servicePrincipalOperations = new ServicePrincipalOperations(betaClient);
            RoleManagementOperations roleManagementOperations = new RoleManagementOperations(betaClient, userOperations, servicePrincipalOperations);

            IEnumerable<Beta.User> allUsersInTenant = await userOperations.GetUsersAsync();
            IList<Beta.User> randomUsersFromTenant = GenericUtility<Beta.User>.GetaRandomNumberOfItemsFromList(allUsersInTenant, 5);

            IEnumerable<Beta.ServicePrincipal> allServicePrincipals = await servicePrincipalOperations.GetAllServicePrincipalsAsync();
            IList<Beta.ServicePrincipal> randomServicePrincipals = GenericUtility<Beta.ServicePrincipal>.GetaRandomNumberOfItemsFromList(allServicePrincipals, 3);

            #region Directory roles and assignment

            // List
            Console.WriteLine("Getting directory roles");

            var directoryroles = await roleManagementOperations.ListDirectoryRolesAsync();
            IList<Beta.DirectoryRole> randomDirectoryRoles = GenericUtility<Beta.DirectoryRole>.GetaRandomNumberOfItemsFromList(directoryroles, 2);

            foreach (var directoryRole in randomDirectoryRoles)
            {
                Console.WriteLine("Printing role details ");
                ColorConsole.WriteLine(ConsoleColor.Green, await roleManagementOperations.PrintDirectoryRoleAsync(directoryRole, true, true));

                Console.WriteLine("Adding users to role ");
                foreach (var user in randomUsersFromTenant)
                {
                    Console.WriteLine($"Adding user '{user.DisplayName}' to role '{directoryRole.DisplayName}'");
                    await roleManagementOperations.AddMemberToDirectoryRole(directoryRole, user);
                }

                Console.WriteLine("Adding service principals to role ");
                foreach (var servicePrincipal in randomServicePrincipals)
                {
                    Console.WriteLine($"Adding service principal '{servicePrincipal.DisplayName}' to role '{directoryRole.DisplayName}'");
                    await roleManagementOperations.AddMemberToDirectoryRole(directoryRole, servicePrincipal);
                }

                Console.WriteLine("Fetching updated role");
                var updatedrole = await roleManagementOperations.GetDirectoryRoleByIdAsync(directoryRole.Id);

                Console.WriteLine("Printing role details after update");
                ColorConsole.WriteLine(ConsoleColor.Green, await roleManagementOperations.PrintDirectoryRoleAsync(updatedrole, true, true));

                Console.WriteLine("Removing users from role ");
                foreach (var user in randomUsersFromTenant)
                {
                    Console.WriteLine($"Removing user '{user.DisplayName}' to role '{directoryRole.DisplayName}'");
                    await roleManagementOperations.RemoveMemberFromDirectoryRole(updatedrole, user);
                }

                Console.WriteLine("Removing service principal from role ");
                foreach (var servicePrincipal in randomServicePrincipals)
                {
                    Console.WriteLine($"Removing service principal '{servicePrincipal.DisplayName}' from role '{directoryRole.DisplayName}'");
                    await roleManagementOperations.RemoveMemberFromDirectoryRole(updatedrole, servicePrincipal);
                }

                Console.WriteLine("Fetching updated role");
                updatedrole = await roleManagementOperations.GetDirectoryRoleByIdAsync(updatedrole.Id);

                Console.WriteLine("Printing role details after update");
                ColorConsole.WriteLine(ConsoleColor.Green, await roleManagementOperations.PrintDirectoryRoleAsync(updatedrole, true, true));
            }

            //// Print all directory roles and its members
            //for (int i = 0; i < directoryroles.Count(); i++)
            //{
            //    Console.WriteLine($"Printing role {i}/{directoryroles.Count()}");
            //    Console.WriteLine(AsyncHelper.RunSync(async() => await roleManagementOperations.PrintDirectoryRoleAsync(directoryroles[i], true, true)));
            //}

            #endregion Directory roles and assignment

            #region Unified role definition and assignment

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

            //    // List
            //    Console.WriteLine("Listing all role definitions");
            //    var roleDefinitions = await roleManagementOperations.ListUnifiedRoleDefinitions();
            //    roleDefinitions.ForEach(y => ColorConsole.WriteLine(ConsoleColor.Green, $"Role:- Id-{y.Id},DisplayName-{y.DisplayName},Description-{y.Description},IsBuiltIn-{y.IsBuiltIn},IsEnabled-{y.IsEnabled}"));
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

            //    if(roledefinitionstoDelete.Count() > 0)
            //    {
            //        foreach (var roleDef in roledefinitionstoDelete)
            //        {
            //            await roleManagementOperations.DeleteRoleDefinitionAsync(roleDef.Id);
            //        }
            //    }
            //}

            #endregion Unified role definition and assignment

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}