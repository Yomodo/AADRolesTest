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
    public class DirectoryRolesOperations
    {
        private Beta.GraphServiceClient _graphServiceClient;
        private UserOperations _userOperations;
        private ServicePrincipalOperations _servicePrincipalOperations;

        public DirectoryRolesOperations(Beta.GraphServiceClient graphServiceClient, UserOperations userOperations, ServicePrincipalOperations _servicePrincipalOperations)
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

        public async Task<Beta.DirectoryRole> GetDirectoryRoleByDisplayNameAsync(string directoryRoleDisplayName)
        {
            try
            {
                var directoryRoles = await _graphServiceClient.DirectoryRoles.Request().Filter($"displayName eq '{directoryRoleDisplayName}'").GetAsync();
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
    }
}