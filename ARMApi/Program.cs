extern alias BetaLib;

using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System;
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

            RoleManagementOperations roleManagementOperations = new RoleManagementOperations(betaClient);

            // Create
            Console.WriteLine("Creating role definition");
            var roledefinition = await roleManagementOperations.CreateRoleDefinition();

            // Get
            Console.WriteLine("Getting role definition");
            roledefinition = await roleManagementOperations.GetRoleDefinitionByIdAsync(roledefinition.Id);
            roleManagementOperations.PrintRoleDefinition(roledefinition, true);

            // Update
            Console.WriteLine("Updating role definition");
            await roleManagementOperations.UpdateRoleDefinitionAsync(roledefinition.Id, true);
            roleManagementOperations.PrintRoleDefinition(roledefinition, true);

            // List
            Console.WriteLine("Listing all role definitions");
            var roleDefinitions = await roleManagementOperations.ListUnifiedRoleDefinitions();
            roleDefinitions.ForEach(y => ColorConsole.WriteLine(ConsoleColor.Green, $"Role:- Id-{y.Id},DisplayName-{y.DisplayName},Description-{y.Description},IsBuiltIn-{y.IsBuiltIn},IsEnabled-{y.IsEnabled}"));

            // Deleting 
            Console.WriteLine("Deleting role definition");
            await roleManagementOperations.DeleteRoleDefinitionAsync(roledefinition.Id);
            roledefinition = await roleManagementOperations.GetRoleDefinitionByIdAsync(roledefinition.Id);

            if (roledefinition == null)
            {
                Console.WriteLine("role definition successfully deleted");
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}