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
        private ServicePrincipalOperations _servicePrincipalOperations;

        public RoleManagementOperations(Beta.GraphServiceClient graphServiceClient, UserOperations userOperations, ServicePrincipalOperations _servicePrincipalOperations)
        {
            this._graphServiceClient = graphServiceClient;
            this._userOperations = userOperations;
            this._servicePrincipalOperations = _servicePrincipalOperations;
        }

        #region Directory roles

        public async Task<Beta.DirectoryRole> GetDirectoryRoleByIdAsync(string directoryRoleId)
        {
            try
            {
                var directoryRoles = await _graphServiceClient.DirectoryRoles.Request().Filter($"id eq '{directoryRoleId}'").GetAsync();
                return directoryRoles?.CurrentPage?.FirstOrDefault();
            }
            catch (ServiceException gex)
            {
                if (gex.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    throw;
                }
            }
            return null;
        }

        public async Task<Beta.ScopedRoleMembership> GetScopedRoleMembershipByIdAsync(string directoryroleId, string scopedRoleMembershipId)
        {
            try
            {
                return await _graphServiceClient.DirectoryRoles[directoryroleId].ScopedMembers[scopedRoleMembershipId].Request().GetAsync();
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

        public async Task<List<Beta.DirectoryRole>> ListDirectoryRolesAsync()
        {
            List<Beta.DirectoryRole> allDirectoryRoles = new List<Beta.DirectoryRole>();
            Beta.IGraphServiceDirectoryRolesCollectionPage directoryRoles = null;

            try
            {
                directoryRoles = await _graphServiceClient.DirectoryRoles.Request().GetAsync();

                if (directoryRoles != null)
                {
                    allDirectoryRoles = await ProcessIGraphServiceDirectoryRolesCollectionPage(directoryRoles);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not retrieve the directory roles: {e}");
                return null;
            }

            return allDirectoryRoles;
        }

        public async Task AddMemberToDirectoryRole(Beta.DirectoryRole directoryRole, Beta.DirectoryObject directoryObject)
        {
            try
            {
                await _graphServiceClient.DirectoryRoles[directoryRole.Id].Members.References.Request().AddAsync(directoryObject);
            }
            catch (Exception)
            {
                Console.WriteLine($"Failed to add '{directoryObject.Id}' to role '{directoryRole.Id}'");
                throw;
            }
        }

        public async Task RemoveMemberFromDirectoryRole(Beta.DirectoryRole directoryRole, Beta.DirectoryObject directoryObject)
        {
            try
            {
                await _graphServiceClient.DirectoryRoles[directoryRole.Id].Members[directoryObject.Id].Reference.Request().DeleteAsync();
            }
            catch (Exception)
            {
                Console.WriteLine($"Failed to remove '{directoryObject.Id}' from role '{directoryRole.Id}'");
                throw;
            }
        }

        public async Task<string> PrintDirectoryRoleAsync(Beta.DirectoryRole directoryRole, bool verbose = false, bool printMembers = true)
        {
            string toPrint = string.Empty;
            StringBuilder more = new StringBuilder();
            int maxmembersToPrint = 999;

            if (directoryRole != null)
            {
                toPrint = $"Role:- DisplayName-{directoryRole.DisplayName}";
                //Console.WriteLine(toPrint);

                if (verbose)
                {
                    toPrint = toPrint + $", Id-{directoryRole.Id}, RoleTemplateId-{directoryRole.RoleTemplateId},\nDescription-{directoryRole.Description}";
                    more.AppendLine("");
                }

                if (printMembers)
                {
                    var roleMembers = await ListRoleMembersAsync(directoryRole.Id);

                    if (roleMembers.Count() > 0)
                    {
                        more.AppendLine($"Total assigned members - {roleMembers.Count()}");

                        // Print first maxmembersToPrint assignments only
                        int top = roleMembers.Count() > maxmembersToPrint ? maxmembersToPrint : roleMembers.Count();

                        for (int i = 0; i < top; i++)
                        {
                            Beta.DirectoryObject member = roleMembers[i];

                            if (member is Beta.User)
                            {
                                Beta.User principal = await _userOperations.GetUserByIdAsync(member.Id);

                                more.AppendLine($"  Role Member[User]:- {_userOperations.PrintBetaUserDetails(principal, false, member.Id)}");
                            }
                            else if (member is Beta.ServicePrincipal)
                            {
                                Beta.ServicePrincipal servicePrincipal = await this._servicePrincipalOperations.GetServicePrincipalByIdAsync(member.Id);
                                more.AppendLine($"  Role Member[SP]:- {_servicePrincipalOperations.PrintServicePrincipalBasic(servicePrincipal)}");
                            }
                            else
                            {
                                more.Append($"  {member.Id} of Type - {member.ODataType} NOT PROCESSED !");
                            }
                        }
                    }

                    var scopedRoleMembers = await ListScopedRoleMembersAsync(directoryRole.Id);

                    if (scopedRoleMembers.Count() > 0)
                    {
                        more.AppendLine($"Total scoped members - {scopedRoleMembers.Count()}");

                        // Print first maxmembersToPrint assignments only
                        int top = scopedRoleMembers.Count() > maxmembersToPrint ? maxmembersToPrint : scopedRoleMembers.Count();

                        for (int i = 0; i < top; i++)
                        {
                            Beta.ScopedRoleMembership member = scopedRoleMembers[i];

                            more.AppendLine($"  Scoped Member:- {member.RoleMemberInfo?.DisplayName}");
                        }
                    }
                }

                // Console.WriteLine("\t" + toPrint + more.ToString());
            }
            else
            {
                Console.WriteLine("The provided directory role is null!");
            }

            return toPrint + more.ToString();
        }

        public async Task<List<Beta.DirectoryObject>> ListRoleMembersAsync(string directoryRoleId)
        {
            List<Beta.DirectoryObject> allRoleMemberAssignments = new List<Beta.DirectoryObject>();

            Beta.IDirectoryRoleMembersCollectionWithReferencesPage roleMemberAssignments = null;

            try
            {
                roleMemberAssignments = await _graphServiceClient.DirectoryRoles[directoryRoleId].Members.Request().GetAsync();

                if (roleMemberAssignments != null)
                {
                    allRoleMemberAssignments = await ProcessIDirectoryRoleMembersCollectionWithReferencesPage(roleMemberAssignments);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not retrieve the role member assignments: {e}");
                return null;
            }

            return allRoleMemberAssignments;
        }

        private async Task<List<Beta.DirectoryObject>> ProcessIDirectoryRoleMembersCollectionWithReferencesPage(Beta.IDirectoryRoleMembersCollectionWithReferencesPage directoryroleMembers)
        {
            List<Beta.DirectoryObject> alldirectoryRoleMembers = new List<Beta.DirectoryObject>();

            try
            {
                if (directoryroleMembers != null)
                {
                    do
                    {
                        // Page through results
                        foreach (var roleMember in directoryroleMembers.CurrentPage)
                        {
                            //Console.WriteLine($"{roleMember.Id}-{roleMember.ODataType}");

                            alldirectoryRoleMembers.Add(roleMember);
                        }

                        // are there more pages (Has a @odata.nextLink ?)
                        if (directoryroleMembers.NextPageRequest != null)
                        {
                            directoryroleMembers = await directoryroleMembers.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            directoryroleMembers = null;
                        }
                    } while (directoryroleMembers != null);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not process the directory role members list: {e}");
                return null;
            }

            return alldirectoryRoleMembers;
        }

        public async Task<List<Beta.ScopedRoleMembership>> ListScopedRoleMembersAsync(string directoryRoleId)
        {
            List<Beta.ScopedRoleMembership> allScopedRoleMemberAssignments = new List<Beta.ScopedRoleMembership>();

            Beta.IDirectoryRoleScopedMembersCollectionPage scopedRoleMemberAssignments = null;

            try
            {
                scopedRoleMemberAssignments = await _graphServiceClient.DirectoryRoles[directoryRoleId].ScopedMembers.Request().GetAsync();

                if (scopedRoleMemberAssignments != null)
                {
                    allScopedRoleMemberAssignments = await ProcessIDirectoryRoleScopedMembersCollectionPage(scopedRoleMemberAssignments);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not retrieve the scoped role member assignments: {e}");
                return null;
            }

            return allScopedRoleMemberAssignments;
        }

        private async Task<List<Beta.ScopedRoleMembership>> ProcessIDirectoryRoleScopedMembersCollectionPage(Beta.IDirectoryRoleScopedMembersCollectionPage directoryroleScopedMembers)
        {
            List<Beta.ScopedRoleMembership> alldirectoryRoleScopedMembers = new List<Beta.ScopedRoleMembership>();

            try
            {
                if (directoryroleScopedMembers != null)
                {
                    do
                    {
                        // Page through results
                        foreach (var roleMember in directoryroleScopedMembers.CurrentPage)
                        {
                            alldirectoryRoleScopedMembers.Add(roleMember);
                        }

                        // are there more pages (Has a @odata.nextLink ?)
                        if (directoryroleScopedMembers.NextPageRequest != null)
                        {
                            directoryroleScopedMembers = await directoryroleScopedMembers.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            directoryroleScopedMembers = null;
                        }
                    } while (directoryroleScopedMembers != null);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not process the directory role scoped members list: {e}");
                return null;
            }

            return alldirectoryRoleScopedMembers;
        }

        private async Task<List<Beta.DirectoryRole>> ProcessIGraphServiceDirectoryRolesCollectionPage(Beta.IGraphServiceDirectoryRolesCollectionPage directoryroles)
        {
            List<Beta.DirectoryRole> alldirectoryRoles = new List<Beta.DirectoryRole>();

            try
            {
                if (directoryroles != null)
                {
                    do
                    {
                        // Page through results
                        foreach (var directoryRole in directoryroles.CurrentPage)
                        {
                            //Console.WriteLine($"Role:{directoryRole.DisplayName}");
                            alldirectoryRoles.Add(directoryRole);
                        }

                        // are there more pages (Has a @odata.nextLink ?)
                        if (directoryroles.NextPageRequest != null)
                        {
                            directoryroles = await directoryroles.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            directoryroles = null;
                        }
                    } while (directoryroles != null);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not process the directory roles list: {e}");
                return null;
            }

            return alldirectoryRoles;
        }

        #endregion Directory roles

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
                toPrint = $"Role Definition:- DisplayName-{roleDefinition.DisplayName}";
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

                toPrint = $"Role Assignment:- Role-{roleDefinition.DisplayName}, User-{_userOperations.PrintBetaUserDetails(principal, false, roleAssignment.PrincipalId)}";
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