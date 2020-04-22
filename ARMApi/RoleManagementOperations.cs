extern alias BetaLib;

using Common;
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
        private UserOperations _userOperations;

        public RoleManagementOperations(Beta.GraphServiceClient graphServiceClient, UserOperations userOperations)
        {
            this._graphServiceClient = graphServiceClient;
            this._userOperations = userOperations;
        }

        #region RoleDefinitions

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
                IList<Beta.UnifiedRolePermission> rolePermissions = new List<Beta.UnifiedRolePermission>()
                {
                    new Beta.UnifiedRolePermission()
                    {
                        AllowedResourceActions = new string[] { "microsoft.directory/applications/basic/read" } ,
                    ODataType = null // TEMP till bug fixed
                    }
                };

                newRoleDefinition = await _graphServiceClient.RoleManagement.Directory.RoleDefinitions.Request().AddAsync(new Beta.UnifiedRoleDefinition
                {
                    Description = "Update basic properties of application registrations",
                    DisplayName = "Application Registration Support Administrator",
                    RolePermissions = rolePermissions,
                    IsEnabled = false,
                    ODataType = null // TEMP till bug fixed
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
            try
            {
                var newRoleDefinitions = await _graphServiceClient.RoleManagement.Directory.RoleDefinitions.Request().Filter($"id eq '{roleDefinitionId}'").GetAsync();
                return newRoleDefinitions?.CurrentPage?.FirstOrDefault();
            }
            catch (Microsoft.Graph.ServiceException gex)
            {
                if (gex.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    throw;
                }
            }
            return null;
        }

        public async Task<Beta.UnifiedRoleDefinition> UpdateRoleDefinitionAsync(string roleDefinitionId, bool isEnabled)
        {
            Beta.UnifiedRoleDefinition updatedroleDefinition = null;
            try
            {
                // Update the role definition.
                updatedroleDefinition = await _graphServiceClient.RoleManagement.Directory.RoleDefinitions[roleDefinitionId].Request().UpdateAsync(new Beta.UnifiedRoleDefinition
                {
                    IsEnabled = isEnabled,
                    ODataType = null // TEMP till bug fixed
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

        public async Task<IEnumerable<Beta.UnifiedRoleDefinition>> GetRoleDefinitionByDisplayNameAsync(string displayName)
        {
            IEnumerable<Beta.UnifiedRoleDefinition> roledefinitions = null;
            try
            {
                roledefinitions = await _graphServiceClient.RoleManagement.Directory.RoleDefinitions.Request().Filter($"DisplayName eq '{displayName}'").GetAsync();
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not get the Role Definition with name-{displayName}: {e}");
            }

            return roledefinitions;
        }

        public async Task PrintRoleDefinition(Beta.UnifiedRoleDefinition roleDefinition, bool verbose = false, bool printAssignments = true)
        {
            string toPrint = string.Empty;

            if (roleDefinition != null)
            {
                toPrint = $"Role:- DisplayName-{roleDefinition.DisplayName}";
                Console.WriteLine(toPrint);
                StringBuilder more = new StringBuilder();

                if (verbose)
                {
                    more.AppendLine($", Id-{roleDefinition.Id},IsBuiltIn-{roleDefinition.IsBuiltIn},IsEnabled-{roleDefinition.IsEnabled},Description-{roleDefinition.Description}");
                }

                if (printAssignments)
                {
                    var roleAssignments = await ListUnifiedRoleAssignments(roleDefinition.Id);

                    if (roleAssignments.Count() > 0)
                    {
                        foreach (var roleAssignment in roleAssignments)
                        {
                            more.AppendLine($"\t{await PrintRoleAssignment(roleAssignment)}");
                        }
                    }
                }

                Console.WriteLine("\t" + toPrint + more.ToString());
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

        #endregion RoleDefinitions

        #region RoleAssignment

        public async Task<List<Beta.UnifiedRoleAssignment>> ListUnifiedRoleAssignments(string roleDefinitionId)
        {
            List<Beta.UnifiedRoleAssignment> allUnifiedRoleAssignments = new List<Beta.UnifiedRoleAssignment>();

            Beta.IRbacApplicationRoleAssignmentsCollectionPage roleassignments = null;

            try
            {
                roleassignments = await _graphServiceClient.RoleManagement.Directory.RoleAssignments.Request().Filter($"roleDefinitionId eq '{roleDefinitionId}'").GetAsync();

                if (roleassignments != null)
                {
                    allUnifiedRoleAssignments = await ProcessIRbacApplicationRoleAssignmentsCollectionPage(roleassignments);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not retrieve the role assignments list: {e}");
                return null;
            }

            return allUnifiedRoleAssignments;
        }

        public async Task<IList<Beta.UnifiedRoleAssignment>> CreateRoleAssignment(Beta.UnifiedRoleDefinition roleDefinition, IList<Beta.User> usersToAssign)
        {
            IList<Beta.UnifiedRoleAssignment> newRoleAssignments = new List<Beta.UnifiedRoleAssignment>();

            try
            {
                foreach (var user in usersToAssign)
                {
                    var newRoleAssignment = await _graphServiceClient.RoleManagement.Directory.RoleAssignments.Request().AddAsync(new Beta.UnifiedRoleAssignment
                    {
                        PrincipalId = user.Id,
                        RoleDefinitionId = roleDefinition.Id,
                        ResourceScope = "/",
                        ODataType = null // TEMP till bug fixed
                    });

                    newRoleAssignments.Add(newRoleAssignment);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine("We could not add a new UnifiedRoleAssignment: " + e.Error.Message);
                return null;
            }

            return newRoleAssignments;
        }

        public async Task<Beta.UnifiedRoleAssignment> GetRoleAssignmentByIdAsync(string roleAssignmentId)
        {
            try
            {
                var roleAssignment = await _graphServiceClient.RoleManagement.Directory.RoleAssignments[roleAssignmentId].Request().GetAsync();
                return roleAssignment;
            }
            catch (Microsoft.Graph.ServiceException gex)
            {
                if (gex.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    throw;
                }
            }
            return null;
        }

        public async Task DeleteRoleAssignmentAsync(string roleAssignmentId)
        {
            try
            {
                await _graphServiceClient.RoleManagement.Directory.RoleAssignments[roleAssignmentId].Request().DeleteAsync();
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not delete the Role Definition with Id-{roleAssignmentId}: {e}");
            }
        }

        public async Task<string> PrintRoleAssignment(Beta.UnifiedRoleAssignment roleAssignment)
        {
            string toPrint = string.Empty;

            if (roleAssignment != null)
            {
                Beta.User principal = await _userOperations.GetUserByIdAsync(roleAssignment.PrincipalId);
                Beta.UnifiedRoleDefinition roleDefinition = await GetRoleDefinitionByIdAsync(roleAssignment.RoleDefinitionId);

                toPrint = $"Role Assignment:- Role-{roleDefinition.DisplayName}, User-{_userOperations.PrintBetaUserDetails(principal, false)}";
            }
            else
            {
                toPrint = "The provided role assignment is null!";
            }

            return toPrint;
        }

        private async Task<List<Beta.UnifiedRoleAssignment>> ProcessIRbacApplicationRoleAssignmentsCollectionPage(Beta.IRbacApplicationRoleAssignmentsCollectionPage roleassignments)
        {
            List<Beta.UnifiedRoleAssignment> allUnifiedRoleAssignments = new List<Beta.UnifiedRoleAssignment>();

            try
            {
                if (roleassignments != null)
                {
                    do
                    {
                        // Page through results
                        foreach (var roleAssignment in roleassignments.CurrentPage)
                        {
                            Beta.User user = await _userOperations.GetUserByIdAsync(roleAssignment.PrincipalId);
                            // Console.WriteLine($"\tAssigned User:{_userOperations.PrintBetaUserDetails(user)}");
                            allUnifiedRoleAssignments.Add(roleAssignment);
                        }

                        // are there more pages (Has a @odata.nextLink ?)
                        if (roleassignments.NextPageRequest != null)
                        {
                            roleassignments = await roleassignments.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            roleassignments = null;
                        }
                    } while (roleassignments != null);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not process the role assignments list: {e}");
                return null;
            }

            return allUnifiedRoleAssignments;
        }

        #endregion RoleAssignment
    }
}