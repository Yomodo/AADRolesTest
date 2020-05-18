extern alias BetaLib;

using AADGraphTesting;
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
    public class PIMOperations
    {
        private Beta.GraphServiceClient _graphServiceClient;
        private UserOperations _userOperations;
        private ServicePrincipalOperations _servicePrincipalOperations;

        public PIMOperations(Beta.GraphServiceClient graphServiceClient, UserOperations userOperations, ServicePrincipalOperations servicePrincipalOperations)
        {
            this._graphServiceClient = graphServiceClient;
            this._userOperations = userOperations;
            this._servicePrincipalOperations = servicePrincipalOperations;
        }

        public async Task<List<Beta.GovernanceResource>> ListGovernanceResourcesAsync()
        {
            var governanceReources = await _graphServiceClient.PrivilegedAccess["azureResources"].Resources.Request().GetAsync();

            return await ProcessIPrivilegedAccessResourcesCollectionPage(governanceReources);
        }

        public async Task<string> PrintGovernanceResourceAsync(Beta.GovernanceResource governanceResource, bool printVerbose = false, bool printAssignments = false)
        {
            StringBuilder sb = new StringBuilder();

            if (governanceResource != null)
            {
                sb.Append($"Id:{governanceResource.Id}");
                sb.Append($"ExternalId:{governanceResource.ExternalId}");
                sb.Append($"DisplayName:{governanceResource.DisplayName}");
                sb.Append($"Status:{governanceResource.Status}");
                sb.Append($"RegisteredRoot:{governanceResource.RegisteredRoot}");
                sb.Append($"Type:{governanceResource.Type}");
                sb.Append($"RegisteredDateTime:{governanceResource.RegisteredDateTime}");

                if (printVerbose)
                {
                    if (governanceResource.RoleSettings != null)
                    {
                        var rolesettings = await ProcessIGovernanceResourceRoleSettingMyCollectionPage(governanceResource.RoleSettings);

                        await rolesettings.ForEachAsync(async roleSetting =>
                        {
                            sb.AppendLine($"\t\t{await PrintGovernanceRoleSettingAsync(roleSetting)}");
                        });
                    }

                    if (governanceResource.Parent != null)
                    {
                        sb.Append("");
                        sb.Append("\t" + await this.PrintGovernanceResourceAsync(governanceResource.Parent, printAssignments));
                        sb.Append("");
                    }

                    if (printAssignments)
                    {
                        sb.Append($"RoleAssignmentRequests:{governanceResource.RoleAssignmentRequests.Count}");

                        sb.Append($"RoleAssignments:{governanceResource.RoleAssignments.Count}");

                        sb.Append($"RoleDefinitions:{governanceResource.RoleDefinitions.Count}");
                    }
                }
            }

            return sb.ToString();
        }

        private async Task<string> PrintGovernanceRoleSettingAsync(Beta.GovernanceRoleSetting roleSetting, bool printVerbose = false)
        {
            StringBuilder sb = new StringBuilder();

            if (roleSetting != null)
            {
                sb.AppendLine("---Role settings---");
                sb.Append($"Id:{roleSetting.Id}");
                sb.Append($"IsDefault:{roleSetting.IsDefault}");
                if (roleSetting.LastUpdatedBy != null)
                {
                    var user = await _userOperations.GetUserByIdAsync(roleSetting.LastUpdatedBy);
                    sb.AppendLine($"\tLastUpdatedBy:{ _userOperations.PrintBetaUserDetails(user, false, roleSetting.LastUpdatedBy)}");
                }
                sb.Append($"LastUpdatedDateTime:{roleSetting.LastUpdatedDateTime}");

                sb.Append($"ResourceId:{roleSetting.ResourceId}");
                if (printVerbose)
                {
                    if (roleSetting.Resource != null)
                    {
                        sb.Append($"\tResource:{await PrintGovernanceResourceAsync(roleSetting.Resource)}");
                    }

                    if (roleSetting.RoleDefinition != null)
                    {
                        sb.Append($"\t RoleDefinition:{ await PrintGovernanceRoleDefinitionAsync(roleSetting.RoleDefinition)}");
                    }
                    sb.Append($"RoleDefinitionId:{roleSetting.RoleDefinitionId}");

                    if (roleSetting.AdminEligibleSettings.Count() > 0)
                    {
                        sb.AppendLine($"\t AdminEligibleSettings");

                        await roleSetting.AdminEligibleSettings.ForEachAsync(async setting =>
                        {
                            sb.AppendLine($"\t\t{await PrintGovernanceRuleSettingAsync(setting)}");
                        });
                    }

                    if (roleSetting.AdminMemberSettings.Count() > 0)
                    {
                        sb.AppendLine($"\t AdminMemberSettings");

                        await roleSetting.AdminMemberSettings.ForEachAsync(async setting =>
                        {
                            sb.AppendLine($"\t\t{await PrintGovernanceRuleSettingAsync(setting)}");
                        });
                    }

                    if (roleSetting.UserEligibleSettings.Count() > 0)
                    {
                        sb.AppendLine($"\t UserEligibleSettings");

                        await roleSetting.UserEligibleSettings.ForEachAsync(async setting =>
                        {
                            sb.AppendLine($"\t\t{await PrintGovernanceRuleSettingAsync(setting)}");
                        });
                    }

                    if (roleSetting.UserMemberSettings.Count() > 0)
                    {
                        sb.AppendLine($"\t UserMemberSettings");

                        await roleSetting.UserMemberSettings.ForEachAsync(async setting =>
                        {
                            sb.AppendLine($"\t\t{await PrintGovernanceRuleSettingAsync(setting)}");
                        });
                    }

                    if (roleSetting.AdditionalData.Count() > 0)
                    {
                        sb.AppendLine($"\t AdditionalData");

                        await roleSetting.AdditionalData.ForEachAsync(data =>
                        {
                            sb.AppendLine($"\t\t{data}");
                        });
                    }
                }

                sb.AppendLine("---Role settings end---");
            }

            return sb.ToString();
        }

        private async Task<string> PrintGovernanceRuleSettingAsync(Beta.GovernanceRuleSetting governanceRuleSetting, bool printVerbose = false)
        {
            StringBuilder sb = new StringBuilder();

            if (governanceRuleSetting != null)
            {
                sb.Append($"RuleIdentifier:{governanceRuleSetting.RuleIdentifier}");
                if (governanceRuleSetting.Setting != null)
                {
                    sb.AppendLine($"\t Settings");

                    await governanceRuleSetting.Setting.ForEachAsync(data =>
                    {
                        sb.AppendLine($"\t\t{data}");
                    });
                }

                if (printVerbose)
                {
                    if (governanceRuleSetting?.AdditionalData?.Count > 0)
                    {
                        sb.AppendLine($"\t AdditionalData");

                        await governanceRuleSetting.AdditionalData.ForEachAsync(data =>
                        {
                            sb.AppendLine($"\t\t{data}");
                        });
                    }
                }
            }
            return sb.ToString();
        }

        private async Task<string> PrintGovernanceRoleDefinitionAsync(Beta.GovernanceRoleDefinition governanceRoleDefinition, bool printVerbose = false)
        {
            StringBuilder sb = new StringBuilder();

            if (governanceRoleDefinition != null)
            {
                sb.Append($"Id:{governanceRoleDefinition.Id}");
                sb.Append($"DisplayName:{governanceRoleDefinition.DisplayName}");
                sb.Append($"TemplateId:{governanceRoleDefinition.TemplateId}");
                sb.Append($"ExternalId:{governanceRoleDefinition.ExternalId}");

                if (printVerbose)
                {
                    if (governanceRoleDefinition.Resource != null)
                    {
                        sb.Append($"\tResource:{await PrintGovernanceResourceAsync(governanceRoleDefinition.Resource)}");
                    }
                    sb.Append($"ResourceId:{governanceRoleDefinition.ResourceId}");

                    if (governanceRoleDefinition.RoleSetting != null)
                    {
                        sb.Append($"\tRoleSetting:{await PrintGovernanceRoleSettingAsync(governanceRoleDefinition.RoleSetting)}");
                    }
                    sb.Append($"ResourceId:{governanceRoleDefinition.ResourceId}");

                    if (governanceRoleDefinition?.AdditionalData?.Count > 0)
                    {
                        sb.AppendLine($"\t AdditionalData");

                        await governanceRoleDefinition.AdditionalData.ForEachAsync(data =>
                        {
                            sb.AppendLine($"\t\t{data}");
                        });
                    }
                }
            }

            return sb.ToString();
        }

        private async Task<string> PrintGovernanceRoleAssignmentAsync(Beta.GovernanceRoleAssignment governanceRoleAssignment, bool printVerbose = false)
        {
            StringBuilder sb = new StringBuilder();

            if (governanceRoleAssignment != null)
            {
                sb.Append($"Id:{governanceRoleAssignment.Id}");
                sb.Append($"AssignmentState:{governanceRoleAssignment.AssignmentState}");
                sb.Append($"StartDateTime:{governanceRoleAssignment.StartDateTime}");
                sb.Append($"ResourceId:{governanceRoleAssignment.ResourceId}");
                sb.Append($"MemberType:{governanceRoleAssignment.MemberType}");
                sb.Append($"Status:{governanceRoleAssignment.Status}");
                sb.Append($"SubjectId:{governanceRoleAssignment.SubjectId}");
                sb.Append($"\tSubject:{await PrintGovernanceSubjectAsync(governanceRoleAssignment.Subject, printVerbose)}");

         
                if (printVerbose)
                {
                    if (governanceRoleAssignment.LinkedEligibleRoleAssignment!= null)
                    {
                        sb.AppendLine($"\t LinkedEligibleRoleAssignment: {await PrintGovernanceRoleAssignmentAsync(governanceRoleAssignment.LinkedEligibleRoleAssignment)}" );
                        sb.Append($"LinkedEligibleRoleAssignmentId:{governanceRoleAssignment.LinkedEligibleRoleAssignmentId}");
                    }

                    if (governanceRoleAssignment?.AdditionalData?.Count > 0)
                    {
                        sb.AppendLine($"\t AdditionalData");

                        await governanceRoleAssignment.AdditionalData.ForEachAsync(data =>
                        {
                            sb.AppendLine($"\t\t{data}");
                        });
                    }
                }
            }
            return sb.ToString();
        }

        private async Task<string> PrintGovernanceRoleAssignmentRequestAsync(Beta.GovernanceRoleAssignmentRequestObject governanceRoleAssignmentRequest, bool printVerbose = false)
        {
            StringBuilder sb = new StringBuilder();

            if (governanceRoleAssignmentRequest != null)
            {
                sb.Append($"Id:{governanceRoleAssignmentRequest.Id}");
                sb.Append($"AssignmentState:{governanceRoleAssignmentRequest.AssignmentState}");
                sb.Append($"Reason:{governanceRoleAssignmentRequest.Reason}");
                sb.Append($"ResourceId:{governanceRoleAssignmentRequest.ResourceId}");
                sb.Append($"RequestedDateTime:{governanceRoleAssignmentRequest.RequestedDateTime}");
                sb.Append($"Status:{governanceRoleAssignmentRequest.Status}");
                sb.Append($"SubjectId:{governanceRoleAssignmentRequest.SubjectId}");
                sb.Append($"\tSubject:{await PrintGovernanceSubjectAsync(governanceRoleAssignmentRequest.Subject, printVerbose)}");
                sb.Append($"LinkedEligibleRoleAssignmentId:{governanceRoleAssignmentRequest.LinkedEligibleRoleAssignmentId}");

                if (printVerbose)
                {
                    
                    
                    if (governanceRoleAssignmentRequest?.AdditionalData?.Count > 0)
                    {
                        sb.AppendLine($"\t AdditionalData");

                        await governanceRoleAssignmentRequest.AdditionalData.ForEachAsync(data =>
                        {
                            sb.AppendLine($"\t\t{data}");
                        });
                    }
                }
            }
            return sb.ToString();
        }


        private async Task<string> PrintGovernanceSubjectAsync(Beta.GovernanceSubject governanceSubject, bool printVerbose = false)
        {
            StringBuilder sb = new StringBuilder();

            if (governanceSubject != null)
            {
                sb.Append($"Id:{governanceSubject.Id}");
                sb.Append($"Type:{governanceSubject.Type}");
                sb.Append($"DisplayName:{governanceSubject.DisplayName}");
                sb.Append($"PrincipalName:{governanceSubject.PrincipalName}");
                sb.Append($"Email:{governanceSubject.Email}");

                if (printVerbose)
                {
                    if (governanceSubject?.AdditionalData?.Count > 0)
                    {
                        sb.AppendLine($"\t AdditionalData");

                        await governanceSubject.AdditionalData.ForEachAsync(data =>
                        {
                            sb.AppendLine($"\t\t{data}");
                        });
                    }
                }
            }
            return sb.ToString();
        }

        private async Task<string> PrintGovernanceScheduleAsync(Beta.GovernanceSchedule governanceSchedule, bool printVerbose = false)
        {
            StringBuilder sb = new StringBuilder();

            if (governanceSchedule != null)
            {
                sb.Append($"Id:{governanceSchedule.StartDateTime}");
                sb.Append($"EndDateTime:{governanceSchedule.EndDateTime}");
                sb.Append($"Duration:{governanceSchedule.Duration}");
                sb.Append($"Type:{governanceSchedule.Type}");

                if (printVerbose)
                {
                    if (governanceSchedule?.AdditionalData?.Count > 0)
                    {
                        sb.AppendLine($"\t AdditionalData");

                        await governanceSchedule.AdditionalData.ForEachAsync(data =>
                        {
                            sb.AppendLine($"\t\t{data}");
                        });
                    }
                }
            }
            return sb.ToString();
        }

        private async Task<List<Beta.GovernanceRoleDefinition>> ProcessIGovernanceResourceRoleDefinitionsCollectionPage(Beta.IGovernanceResourceRoleDefinitionsCollectionPage resourceRoleDefinitionsCollection)
        {
            List<Beta.GovernanceRoleDefinition> allGovernanceResourceRoleDefinitions = new List<Beta.GovernanceRoleDefinition>();

            try
            {
                if (resourceRoleDefinitionsCollection != null)
                {
                    do
                    {
                        // Page through results
                        foreach (var resourceRoleDefinition in resourceRoleDefinitionsCollection.CurrentPage)
                        {
                            allGovernanceResourceRoleDefinitions.Add(resourceRoleDefinition);
                        }

                        // are there more pages (Has a @odata.nextLink ?)
                        if (resourceRoleDefinitionsCollection.NextPageRequest != null)
                        {
                            resourceRoleDefinitionsCollection = await resourceRoleDefinitionsCollection.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            resourceRoleDefinitionsCollection = null;
                        }
                    } while (resourceRoleDefinitionsCollection != null);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not process the governance role definitions list: {e}");
                return null;
            }

            return allGovernanceResourceRoleDefinitions;
        }

        private async Task<List<Beta.GovernanceRoleAssignment>> ProcessIGovernanceResourceRoleAssignmentsCollectionPage(Beta.IGovernanceResourceRoleAssignmentsCollectionPage resourceRoleAssignmentsCollection)
        {
            List<Beta.GovernanceRoleAssignment> allGovernanceResourceRoleAssignments = new List<Beta.GovernanceRoleAssignment>();

            try
            {
                if (resourceRoleAssignmentsCollection != null)
                {
                    do
                    {
                        // Page through results
                        foreach (var resourceRoleAssignment in resourceRoleAssignmentsCollection.CurrentPage)
                        {
                            allGovernanceResourceRoleAssignments.Add(resourceRoleAssignment);
                        }

                        // are there more pages (Has a @odata.nextLink ?)
                        if (resourceRoleAssignmentsCollection.NextPageRequest != null)
                        {
                            resourceRoleAssignmentsCollection = await resourceRoleAssignmentsCollection.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            resourceRoleAssignmentsCollection = null;
                        }
                    } while (resourceRoleAssignmentsCollection != null);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not process the governance role assignments list: {e}");
                return null;
            }

            return allGovernanceResourceRoleAssignments;
        }

        private async Task<List<Beta.GovernanceRoleAssignmentRequestObject>> ProcessIGovernanceResourceRoleAssignmentRequestsCollectionPage(Beta.IGovernanceResourceRoleAssignmentRequestsCollectionPage roleAssignmentRequestsCollection)
        {
            List<Beta.GovernanceRoleAssignmentRequestObject> allGovernanceRoleAssignmentRequests = new List<Beta.GovernanceRoleAssignmentRequestObject>();

            try
            {
                if (roleAssignmentRequestsCollection != null)
                {
                    do
                    {
                        // Page through results
                        foreach (var roleAssignmentRequest in roleAssignmentRequestsCollection.CurrentPage)
                        {
                            allGovernanceRoleAssignmentRequests.Add(roleAssignmentRequest);
                        }

                        // are there more pages (Has a @odata.nextLink ?)
                        if (roleAssignmentRequestsCollection.NextPageRequest != null)
                        {
                            roleAssignmentRequestsCollection = await roleAssignmentRequestsCollection.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            roleAssignmentRequestsCollection = null;
                        }
                    } while (roleAssignmentRequestsCollection != null);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not process the governance role assignment requests list: {e}");
                return null;
            }

            return allGovernanceRoleAssignmentRequests;
        }

        private async Task<List<Beta.GovernanceRoleSetting>> ProcessIGovernanceResourceRoleSettingMyCollectionPage(Beta.IGovernanceResourceRoleSettingsCollectionPage roleSettingsCollection)
        {
            List<Beta.GovernanceRoleSetting> allGovernanceRoleSettings = new List<Beta.GovernanceRoleSetting>();

            try
            {
                if (roleSettingsCollection != null)
                {
                    do
                    {
                        // Page through results
                        foreach (var privilegedAccessReources in roleSettingsCollection.CurrentPage)
                        {
                            allGovernanceRoleSettings.Add(privilegedAccessReources);
                        }

                        // are there more pages (Has a @odata.nextLink ?)
                        if (roleSettingsCollection.NextPageRequest != null)
                        {
                            roleSettingsCollection = await roleSettingsCollection.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            roleSettingsCollection = null;
                        }
                    } while (roleSettingsCollection != null);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not process the governance role settings list: {e}");
                return null;
            }

            return allGovernanceRoleSettings;
        }

        private async Task<List<Beta.GovernanceResource>> ProcessIPrivilegedAccessResourcesCollectionPage(Beta.IPrivilegedAccessResourcesCollectionPage governanceResources)
        {
            List<Beta.GovernanceResource> allPrivilegedAccessReourcess = new List<Beta.GovernanceResource>();

            try
            {
                if (governanceResources != null)
                {
                    do
                    {
                        // Page through results
                        foreach (var privilegedAccessReources in governanceResources.CurrentPage)
                        {
                            allPrivilegedAccessReourcess.Add(privilegedAccessReources);
                        }

                        // are there more pages (Has a @odata.nextLink ?)
                        if (governanceResources.NextPageRequest != null)
                        {
                            governanceResources = await governanceResources.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            governanceResources = null;
                        }
                    } while (governanceResources != null);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not process the governance resources  list: {e}");
                return null;
            }

            return allPrivilegedAccessReourcess;
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