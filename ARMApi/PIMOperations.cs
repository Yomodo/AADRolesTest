extern alias BetaLib;

using Common;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Beta = BetaLib.Microsoft.Graph;

namespace ARMApi
{
    public class PIMOperations
    {
        private Beta.GraphServiceClient _graphServiceClient;
        private UserOperations _userOperations;

        public PIMOperations(Beta.GraphServiceClient graphServiceClient, UserOperations userOperations)
        {
            this._graphServiceClient = graphServiceClient;
            this._userOperations = userOperations;
        }

        public async Task<List<Beta.PrivilegedRoleAssignment>> GetMyPrivilegedRoleAssignmentsAsync()
        {
            var roleassignments = await _graphServiceClient.PrivilegedRoleAssignments.My().Request().GetAsync();

            return await ProcessIPrivilegedRoleAssignmentMyCollectionPage(roleassignments);
        }

        public string PrintPrivilegedRoleAssignment(Beta.PrivilegedRoleAssignment roleAssignment)
        {
            string toPrint = string.Empty;

            if (roleAssignment != null)
            {
                toPrint = toPrint + $"Type-{roleAssignment.ODataType}, IsElevated-{roleAssignment.IsElevated}, UserId-{roleAssignment.UserId}";
                toPrint = toPrint + "\n" + PrintPrivilegedRole(roleAssignment.RoleInfo);
            }

            return toPrint;
        }

        public string PrintPrivilegedRole(Beta.PrivilegedRole privilegedRole)
        {

            string toPrint = string.Empty;

            if (privilegedRole != null)
            {
                toPrint = toPrint + $"Type-{privilegedRole.ODataType}, Type-{privilegedRole.Name}";
            }

            return toPrint;
        }

        private async Task<List<Beta.PrivilegedRoleAssignment>> ProcessIPrivilegedRoleAssignmentMyCollectionPage(Beta.IPrivilegedRoleAssignmentMyCollectionPage roleassignments)
        {
            List<Beta.PrivilegedRoleAssignment> allPrivilegedRoleAssignments = new List<Beta.PrivilegedRoleAssignment>();

            try
            {
                if (roleassignments != null)
                {
                    do
                    {
                        // Page through results
                        foreach (var roleAssignment in roleassignments.CurrentPage)
                        {
                            allPrivilegedRoleAssignments.Add(roleAssignment);
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
                Console.WriteLine($"We could not process the privileged role assignments list: {e}");
                return null;
            }

            return allPrivilegedRoleAssignments;
        }

    }
}
