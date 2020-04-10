extern alias BetaLib;

using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Beta = BetaLib.Microsoft.Graph;

namespace ARMApi
{
    public class RoleManagementOperations
    {
        private Beta.GraphServiceClient _graphServiceClient;

        public RoleManagementOperations(Beta.GraphServiceClient graphServiceClient)
        {
            this._graphServiceClient = graphServiceClient;
        }

        public async Task<List<Beta.UnifiedRoleDefinition>> ListUnifiedRoleDefinitions()
        {
            List<Beta.UnifiedRoleDefinition> allUnifiedRoleDefinitions = new List<Beta.UnifiedRoleDefinition>();
            Beta.IRbacApplicationRoleDefinitionsCollectionPage roledefinitions = null;

            try
            {
                roledefinitions = await _graphServiceClient.RoleManagement.Directory.RoleDefinitions.Request().GetAsync();

                if (roledefinitions != null)
                {
                    allUnifiedRoleDefinitions = await ProcessIRbacApplicationRoleDefinitionsCollectionPage(roledefinitions);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not retrieve the role definitions list: {e}");
                return null;
            }

            return allUnifiedRoleDefinitions;
        }

        public async Task<Beta.UnifiedRoleDefinition> CreateRoleDefinition()
        {
            Beta.UnifiedRoleDefinition newRoleDefinition = null;

            try
            {
                IList<Beta.UnifiedRolePermission> rolePermissions = new List<Beta.UnifiedRolePermission>() { new Beta.UnifiedRolePermission() { AllowedResourceActions = new string[] { "microsoft.directory/applications/basic/read" } } };

                newRoleDefinition = await _graphServiceClient.RoleManagement.Directory.RoleDefinitions.Request().AddAsync(new Beta.UnifiedRoleDefinition
                {
                    Description = "Update basic properties of application registrations",
                    DisplayName = "Application Registration Support Administrator",                     
                    RolePermissions = rolePermissions,
                    IsEnabled = false
                });
            }
            catch (ServiceException e)
            {
                Console.WriteLine("We could not add a new UnifiedRoleDefinition: " + e.Error.Message);
                return null;
            }

            return newRoleDefinition;
        }

        public async Task<Beta.UnifiedRoleDefinition> GetRoleDefinitionByIdAsync(string roleDefinitionId)
        {
            var newRoleDefinitions = await _graphServiceClient.RoleManagement.Directory.RoleDefinitions.Request().Filter($"id eq '{roleDefinitionId}'").GetAsync();
            return newRoleDefinitions.CurrentPage.FirstOrDefault();
        }

        public async Task<Beta.UnifiedRoleDefinition> UpdateRoleDefinitionAsync(string roleDefinitionId, bool isEnabled)
        {
            Beta.UnifiedRoleDefinition updatedroleDefinition = null;
            try
            {
                // Update the role definition.
                updatedroleDefinition = await _graphServiceClient.RoleManagement.Directory.RoleDefinitions[roleDefinitionId].Request().UpdateAsync(new Beta.UnifiedRoleDefinition
                {
                    IsEnabled = isEnabled
                });
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not update details of the Role Definition with Id {roleDefinitionId}: {e}");
            }

            return updatedroleDefinition;
        }

        public async Task DeleteRoleDefinitionAsync(string roleDefinitionId)
        {
            try
            {
                await _graphServiceClient.RoleManagement.Directory.RoleDefinitions[roleDefinitionId].Request().DeleteAsync();
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not delete the Role Definition with Id-{roleDefinitionId}: {e}");
            }
        }

        public void PrintRoleDefinition(Beta.UnifiedRoleDefinition roleDefinition, bool verbose = false)
        {
            string toPrint = string.Empty;

            if (roleDefinition != null)
            {
                toPrint = $"Role:- DisplayName-{roleDefinition.DisplayName}";
                Console.WriteLine(toPrint);

                if (verbose)
                {
                    StringBuilder more = new StringBuilder();
                    more.AppendLine($", Id-{roleDefinition.Id},Description-{roleDefinition.Description},IsBuiltIn-{roleDefinition.IsBuiltIn},IsEnabled-{roleDefinition.IsEnabled}");

                    Console.WriteLine(toPrint + more.ToString());
                }
            }
            else
            {
                Console.WriteLine("The provided role Definition is null!");
            }
        }

        private async Task<List<Beta.UnifiedRoleDefinition>> ProcessIRbacApplicationRoleDefinitionsCollectionPage(Beta.IRbacApplicationRoleDefinitionsCollectionPage roledefinitions)
        {
            List<Beta.UnifiedRoleDefinition> allUnifiedRoleDefinitions = new List<Beta.UnifiedRoleDefinition>();

            try
            {
                if (roledefinitions != null)
                {
                    do
                    {
                        // Page through results
                        foreach (var roleDefinition in roledefinitions.CurrentPage)
                        {
                            Console.WriteLine($"Role:{roleDefinition.DisplayName}");
                            allUnifiedRoleDefinitions.Add(roleDefinition);
                        }

                        // are there more pages (Has a @odata.nextLink ?)
                        if (roledefinitions.NextPageRequest != null)
                        {
                            roledefinitions = await roledefinitions.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            roledefinitions = null;
                        }
                    } while (roledefinitions != null);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not process the role definitions list: {e}");
                return null;
            }

            return allUnifiedRoleDefinitions;
        }
    }
}