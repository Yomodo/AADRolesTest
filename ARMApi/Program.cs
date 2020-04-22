extern alias BetaLib;

using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
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
            string[] scopes = new string[] { "RoleManagement.ReadWrite.Directory" };

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
            RoleManagementOperations roleManagementOperations = new RoleManagementOperations(betaClient, userOperations);

            IEnumerable<Beta.User> allUsersInTenant = await userOperations.GetUsersAsync();
            IList<Beta.User> randomUsersFromTenant = GenericUtility<Beta.User>.GetaRandomNumberOfItemsFromList(allUsersInTenant, 5);

            // Create
            Console.WriteLine("Creating role definition");
            var roledefinition = await roleManagementOperations.CreateRoleDefinition();

            try
            {
                // Get
                Console.WriteLine("Getting role definition");
                roledefinition = await roleManagementOperations.GetRoleDefinitionByIdAsync(roledefinition.Id);
                await roleManagementOperations.PrintRoleDefinition(roledefinition, false);

                // Update
                Console.WriteLine("Updating role definition");
                await roleManagementOperations.UpdateRoleDefinitionAsync(roledefinition.Id, true);

                Console.WriteLine("Creating role assignments");
                IList<Beta.UnifiedRoleAssignment> roleAssignments = await roleManagementOperations.CreateRoleAssignment(roledefinition, randomUsersFromTenant);

                //Get roleAssignemnt
                Console.WriteLine("Getting newly created role assignments");
                foreach (var newroleAssignment in roleAssignments)
                {
                    var roleAssignment = await roleManagementOperations.GetRoleAssignmentByIdAsync(newroleAssignment.Id);
                    Console.WriteLine("\t" + await roleManagementOperations.PrintRoleAssignment(roleAssignment));
                }

                // Get
                Console.WriteLine("Getting role definition with assignments after update");
                roledefinition = await roleManagementOperations.GetRoleDefinitionByIdAsync(roledefinition.Id);
                await roleManagementOperations.PrintRoleDefinition(roledefinition, false);

                // removing role assignments
                Console.WriteLine("Removing role assignments");

                int assignmentsToRemove = roleAssignments.Count - 3;
                for (int i = 0; i < assignmentsToRemove; i++)
                {
                    await roleManagementOperations.DeleteRoleAssignmentAsync(roleAssignments[i].Id);
                }
                await roleManagementOperations.PrintRoleDefinition(roledefinition, true);

                // List
                Console.WriteLine("Listing all role definitions");
                var roleDefinitions = await roleManagementOperations.ListUnifiedRoleDefinitions();
                roleDefinitions.ForEach(y => ColorConsole.WriteLine(ConsoleColor.Green, $"Role:- Id-{y.Id},DisplayName-{y.DisplayName},Description-{y.Description},IsBuiltIn-{y.IsBuiltIn},IsEnabled-{y.IsEnabled}"));
            }
            catch (Exception ex)
            {
                ColorConsole.WriteLine(ConsoleColor.Red, $"{ex}");
            }
            finally
            {
                // Delete role definition
                ColorConsole.WriteLine(ConsoleColor.DarkRed, "Deleting role definition");
                await roleManagementOperations.DeleteRoleDefinitionAsync(roledefinition.Id);
                
                roledefinition = await roleManagementOperations.GetRoleDefinitionByIdAsync(roledefinition.Id);

                if (roledefinition == null)
                {
                    ColorConsole.WriteLine(ConsoleColor.Green, "Role definition successfully deleted");
                }

                IEnumerable<Beta.UnifiedRoleDefinition> roledefinitionstoDelete = await roleManagementOperations.GetRoleDefinitionByDisplayNameAsync("Application Registration Support Administrator");
                
                if(roledefinitionstoDelete.Count() > 0)
                {
                    foreach (var roleDef in roledefinitionstoDelete)
                    {
                        await roleManagementOperations.DeleteRoleDefinitionAsync(roleDef.Id);
                    }
                }
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}