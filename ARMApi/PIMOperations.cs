extern alias BetaLib;

using Common;
using Microsoft.Graph;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
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
        private DirectoryObjectOperations _directoryObjectOperations;
        private ConcurrentDictionary<string, Beta.GovernanceResource> _CachedGovernanceResources;


        private HashSet<string> _AllTypes = new HashSet<string>();
        private HashSet<string> _AllStatuses = new HashSet<string>();

        public HashSet<string> AllTypes { get => _AllTypes; }

        public HashSet<string> AllStatuses { get => _AllStatuses; }

        public PIMOperations(Beta.GraphServiceClient graphServiceClient, UserOperations userOperations, DirectoryObjectOperations directoryObjectOperations)
        {
            this._graphServiceClient = graphServiceClient;
            this._userOperations = userOperations;
            this._directoryObjectOperations = directoryObjectOperations;
            this._CachedGovernanceResources = new ConcurrentDictionary<string, Beta.GovernanceResource>();
        }

        /// <summary>
        /// List a collection of resources the requestor has access to.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Beta.GovernanceResource>> ListGovernanceResourcesAsync(int top = 999)
        {
            var governanceReources = await _graphServiceClient.PrivilegedAccess["azureResources"].Resources.Request()
                .Top(top)
                .GetAsync();

            return await ProcessIPrivilegedAccessResourcesCollectionPage(governanceReources);
        }

        public async Task<List<Beta.GovernanceResource>> DiscoverGovernanceResourcesExpandedAsync(int top = 5)
        {
            // Note: will time out !
            var governanceReources = await _graphServiceClient.PrivilegedAccess["azureResources"].Resources.Request()
                .Select("id,externalId,type,displayName,status,onboardDateTime,registeredDateTime,managedAt,registeredRoot,roleAssignmentCount,roleDefinitionCount,permissions")
                .Top(top)
                .GetAsync();

            return await ProcessIPrivilegedAccessResourcesCollectionPage(governanceReources);
        }

        public async Task<Beta.GovernanceResource> GetGovernanceResourceByIdAsync(string resourceId)
        {
            try
            {
                return await _graphServiceClient.PrivilegedAccess["azureResources"].Resources[resourceId].Request()
                    .Select("id,externalId,type,displayName,status,onboardDateTime,registeredDateTime,managedAt,registeredRoot,roleAssignmentCount,roleDefinitionCount,permissions")
                    .GetAsync();
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

        public async Task RegisterGovernanceResourceAsync(string externalId)
        {
            await _graphServiceClient.PrivilegedAccess["azureResources"].Resources.Register(externalId).Request().PostAsync();
        }

        public async Task<List<Beta.GovernanceRoleDefinition>> ListGovernanceRoleDefinitionsAsync(Beta.GovernanceResource governanceResource)
        {
            List<Beta.GovernanceRoleDefinition> roleDefinitions = new List<Beta.GovernanceRoleDefinition>();

            if (governanceResource != null)
            {
                var gocroledefs = await _graphServiceClient.PrivilegedAccess["azureResources"].Resources[governanceResource.Id].RoleDefinitions.Request().GetAsync();
                roleDefinitions = await ProcessIGovernanceResourceRoleDefinitionsCollectionPage(gocroledefs);
            }

            return roleDefinitions;
        }

        public async Task<Beta.GovernanceRoleDefinition> GetGovernanceRoleDefinitionByIdAsync(Beta.GovernanceRoleDefinition governanceRoleDefinition)
        {
            if (governanceRoleDefinition == null || string.IsNullOrWhiteSpace(governanceRoleDefinition.ResourceId))
            {
                return null;
            }

            try
            {
                return await _graphServiceClient.PrivilegedAccess["azureResources"].Resources[governanceRoleDefinition.ResourceId].RoleDefinitions[governanceRoleDefinition.Id].Request().GetAsync();
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

        public async Task<List<Beta.GovernanceRoleAssignment>> ListGovernanceRoleAssignmentsAsync(Beta.GovernanceResource governanceResource)
        {
            List<Beta.GovernanceRoleAssignment> roleAssignments = new List<Beta.GovernanceRoleAssignment>();

            if (governanceResource != null)
            {
                var govassignments = await _graphServiceClient.PrivilegedAccess["azureResources"].Resources[governanceResource.Id].RoleAssignments.Request().GetAsync();
                roleAssignments = await ProcessIGovernanceResourceRoleAssignmentsCollectionPage(govassignments);
            }

            return roleAssignments;
        }

        public async Task<Beta.GovernanceRoleAssignment> GetGovernanceRoleAssignmentByIdAsync(Beta.GovernanceRoleAssignment governanceRoleAssignment)
        {
            if (governanceRoleAssignment == null || string.IsNullOrWhiteSpace(governanceRoleAssignment.ResourceId))
            {
                return null;
            }

            try
            {
                return await _graphServiceClient.PrivilegedAccess["azureResources"].Resources[governanceRoleAssignment.ResourceId].RoleAssignments[governanceRoleAssignment.Id].Request().GetAsync();
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

        public async Task<List<Beta.GovernanceRoleAssignmentRequestObject>> ListGovernanceRoleAssignmentRequestsAsync(Beta.GovernanceResource governanceResource)
        {
            List<Beta.GovernanceRoleAssignmentRequestObject> roleAssignmentRequests = new List<Beta.GovernanceRoleAssignmentRequestObject>();

            if (governanceResource != null)
            {
                var govassignmentRequests = await _graphServiceClient.PrivilegedAccess["azureResources"].Resources[governanceResource.Id].RoleAssignmentRequests.Request().GetAsync();
                roleAssignmentRequests = await ProcessIGovernanceResourceRoleAssignmentRequestsCollectionPage(govassignmentRequests);
            }

            return roleAssignmentRequests;
        }

        public async Task<Beta.GovernanceRoleAssignmentRequestObject> GetGovernanceRoleAssignmentRequestByIdAsync(Beta.GovernanceRoleAssignmentRequestObject governanceRoleAssignmentRequest)
        {
            if (governanceRoleAssignmentRequest == null || string.IsNullOrWhiteSpace(governanceRoleAssignmentRequest.ResourceId))
            {
                return null;
            }

            try
            {
                return await _graphServiceClient.PrivilegedAccess["azureResources"].Resources[governanceRoleAssignmentRequest.ResourceId].RoleAssignmentRequests[governanceRoleAssignmentRequest.Id].Request().GetAsync();
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

        //public async Task<Beta.GovernanceRoleAssignmentRequestObject> CreateGovernanceRoleAssignmentRequestAsync(Beta.GovernanceRoleAssignmentRequestObject governanceRoleAssignmentRequest)
        //{
        //}

        //public async Task<Beta.GovernanceRoleAssignmentRequestObject> UpdateGovernanceRoleAssignmentRequestAsync(Beta.GovernanceRoleAssignmentRequestObject governanceRoleAssignmentRequest)
        //{
        //}

        //public async Task<Beta.GovernanceRoleAssignmentRequestObject> CancelGovernanceRoleAssignmentRequestAsync(Beta.GovernanceRoleAssignmentRequestObject governanceRoleAssignmentRequest)
        //{
        //}

        //public async Task<Beta.GovernanceRoleAssignmentRequestObject> GetGovernanceRoleAssignmentRequestBySubjectIdAsync(Beta.GovernanceRoleAssignmentRequestObject governanceRoleAssignmentRequest)
        //{
        //}

        public async Task<List<Beta.GovernanceRoleAssignment>> GetGovernanceRoleAssignmentsBySubjectIdAsync(string subjectId)
        {
            List<Beta.GovernanceRoleAssignment> roleAssignments = new List<Beta.GovernanceRoleAssignment>();

            var govassignments = await _graphServiceClient.PrivilegedAccess["azureResources"].RoleAssignments.Request().Filter($"subjectId eq '{subjectId}'").GetAsync();
            roleAssignments = await ProcessIPrivilegedAccessRoleAssignmentsCollectionPage(govassignments);

            return roleAssignments;
        }

        public async Task<List<Beta.GovernanceRoleSetting>> ListGovernanceRoleSettingsAsync(Beta.GovernanceResource governanceResource)
        {
            List<Beta.GovernanceRoleSetting> roleAssignmentRequests = new List<Beta.GovernanceRoleSetting>();

            if (governanceResource != null)
            {
                var govassignmentRequests = await _graphServiceClient.PrivilegedAccess["azureResources"].Resources[governanceResource.Id].RoleSettings.Request().GetAsync();
                roleAssignmentRequests = await ProcessIGovernanceResourceRoleSettingMyCollectionPage(govassignmentRequests);
            }

            return roleAssignmentRequests;
        }

        public async Task<Beta.GovernanceRoleSetting> GetGovernanceRoleSettingByIdAsync(Beta.GovernanceRoleSetting governanceRoleSetting)
        {
            if (governanceRoleSetting == null || string.IsNullOrWhiteSpace(governanceRoleSetting.ResourceId))
            {
                return null;
            }

            try
            {
                return await _graphServiceClient.PrivilegedAccess["azureResources"].Resources[governanceRoleSetting.ResourceId].RoleSettings[governanceRoleSetting.Id].Request().GetAsync();
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

        //    public async Task<Beta.GovernanceRoleSetting> UpdateGovernanceRoleSettingByIdAsync(Beta.GovernanceRoleSetting governanceRoleSetting)
        //    {
        //        var governanceRoleSetting = new Beta.GovernanceRoleSetting
        //        {
        //            AdminEligibleSettings = new List<Beta.GovernanceRuleSetting>()
        //{
        //    new Beta.GovernanceRuleSetting
        //    {
        //        RuleIdentifier = "ExpirationRule",
        //        Setting = "{\"permanentAssignment\":false,\"maximumGrantPeriodInMinutes\":129600}"
        //    }
        //}
        //        };

        //        await _graphServiceClient.PrivilegedAccess["azureResources"].RoleSettings["5fb5aef8-1081-4b8e-bb16-9d5d0385bab5"]
        //            .Request()
        //            .UpdateAsync(governanceRoleSetting);
        //            .UpdateAsync(governanceRoleSetting);
        //    }

        public string PrintGovernanceResourceSlim(Beta.GovernanceResource governanceResource)
        {
            string retVal = string.Empty;

            if(governanceResource != null)
            {
                return $"{governanceResource.DisplayName}, {governanceResource.Status}, {governanceResource.Type}, {governanceResource.Id}";
            }
            
            return retVal;
        }

        public async Task<string> PrintGovernanceResourceAsync(Beta.GovernanceResource governanceResource, bool printVerbose = false, bool printAssignments = false)
        {
            StringBuilder sb = new StringBuilder();

            if (governanceResource != null)
            {
                sb.AppendLine($"Id:{governanceResource.Id}");
                sb.AppendLine($"ExternalId:{governanceResource.ExternalId}");
                sb.AppendLine($"DisplayName:{governanceResource.DisplayName}");
                sb.AppendLine($"Status:{governanceResource.Status}");
                sb.AppendLine($"RegisteredRoot:{governanceResource.RegisteredRoot}");
                sb.AppendLine($"Type:{governanceResource.Type}");
                sb.AppendLine($"RegisteredDateTime:{governanceResource.RegisteredDateTime}");

                if (printVerbose)
                {
                    // role settings
                    int i = 1;

                    var roleSettings = await ListGovernanceRoleSettingsAsync(governanceResource);
                    //var rolesettings = await ProcessIGovernanceResourceRoleSettingMyCollectionPage(governanceResource.RoleSettings);

                    sb.AppendLine($"\tPrinting role settings of governance resource -'{governanceResource.DisplayName}'. Total count -{roleSettings.Count}");

                    await roleSettings.ForEachAsync(async roleSetting =>
                    {
                        sb.AppendLine($"Printing {i}/{roleSettings.Count} role settings.");
                        sb.AppendLine($"\t\t{await PrintGovernanceRoleSettingAsync(roleSetting, printVerbose)}");
                        i++;
                    });

                    if (governanceResource.Parent != null)
                    {
                        sb.AppendLine("Parent");
                        sb.AppendLine("\t" + await this.PrintGovernanceResourceAsync(governanceResource.Parent));
                        sb.AppendLine("");
                    }

                    if (governanceResource?.AdditionalData?.Count > 0)
                    {
                        sb.AppendLine($"\t AdditionalData");

                        governanceResource.AdditionalData.ForEach(data =>
                        {
                            sb.AppendLine($"\t\t{data}");
                        });
                    }

                    if (printAssignments)
                    {
                        i = 1;
                        //Role definitions
                        var roledefinitions = await ListGovernanceRoleDefinitionsAsync(governanceResource);
                        sb.AppendLine($"\tPrinting roleDefintions of governance resource -'{governanceResource.DisplayName}'. Total count -{roledefinitions.Count}");

                        await roledefinitions.ForEachAsync(async r =>
                        {
                            sb.AppendLine($"Printing {i}/{roledefinitions.Count} role definition.");
                            sb.AppendLine($"\t\t{await PrintGovernanceRoleDefinitionAsync(r, true)}");
                            i++;
                        });
                        i = 1;

                        //Role assignments
                        var roleassignments = await ListGovernanceRoleAssignmentsAsync(governanceResource);
                        sb.AppendLine($"\tPrinting role assignments of governance resource -'{governanceResource.DisplayName}. Total count -{roleassignments.Count}");

                        await roleassignments.ForEachAsync(async r =>
                        {
                            var txt = await PrintGovernanceRoleAssignmentAsync(r, true);
                            sb.AppendLine($"Printing {i}/{roleassignments.Count} roleassignments.");
                            sb.AppendLine($"\t\t{txt}");
                            i++;
                        });
                        i = 1;

                        // role assignment requests
                        var roleassignmentRequests = await ListGovernanceRoleAssignmentRequestsAsync(governanceResource);
                        sb.AppendLine($"\tPrinting role assignment requests of governance resource -'{governanceResource.DisplayName}. Total count -{roleassignmentRequests.Count}");

                        roleassignmentRequests.ForEach(async r =>
                        {
                            sb.AppendLine($"Printing {i}/{roleassignmentRequests.Count} roleassignment request.");
                            sb.AppendLine($"\t\t{await PrintGovernanceRoleAssignmentRequestAsync(r, true)}");
                            i++;
                        });
                        i = 1;
                    }
                }
            }

            return sb.ToString();
        }

        public async Task<string> PrintGovernanceRoleSettingAsync(Beta.GovernanceRoleSetting roleSetting, bool printVerbose = false)
        {
            StringBuilder sb = new StringBuilder();

            if (roleSetting != null)
            {
                sb.AppendLine("---Role settings---");
                sb.AppendLine($"Id:{roleSetting.Id}");
                sb.AppendLine($"IsDefault:{roleSetting.IsDefault}");
                sb.AppendLine($"ResourceId:{roleSetting.ResourceId}");
                if (roleSetting.LastUpdatedBy != null)
                {
                    var user = await _userOperations.GetUserByDisplayNameAsync(roleSetting.LastUpdatedBy);
                    sb.AppendLine($"\tLastUpdatedBy:{ _userOperations.PrintBetaUserDetails(user, false, roleSetting.LastUpdatedBy)}");
                }
                if (roleSetting.LastUpdatedDateTime != null)
                {
                    sb.AppendLine($"LastUpdatedDateTime:{roleSetting.LastUpdatedDateTime}");
                }

                if (printVerbose)
                {
                    //if (roleSetting.Resource != null)
                    //{
                    //    sb.AppendLine($"\tResource:{await PrintGovernanceResourceAsync(roleSetting.Resource)}");
                    //}

                    if (roleSetting.RoleDefinition != null)
                    {
                        sb.AppendLine($"\t RoleDefinition:{ await PrintGovernanceRoleDefinitionAsync(roleSetting.RoleDefinition)}");
                    }
                    sb.AppendLine($"RoleDefinitionId:{roleSetting.RoleDefinitionId}");

                    if (roleSetting?.AdminEligibleSettings?.Count() > 0)
                    {
                        sb.AppendLine($"\t AdminEligibleSettings");

                        roleSetting.AdminEligibleSettings.ForEach(setting =>
                        {
                            sb.AppendLine($"\t\t{ PrintGovernanceRuleSetting(setting)}");
                        });
                    }

                    if (roleSetting?.AdminMemberSettings?.Count() > 0)
                    {
                        sb.AppendLine($"\t AdminMemberSettings:");

                        roleSetting.AdminMemberSettings.ForEach(setting =>
                        {
                            sb.AppendLine($"\t\t{ PrintGovernanceRuleSetting(setting)}");
                        });
                    }

                    if (roleSetting?.UserEligibleSettings?.Count() > 0)
                    {
                        sb.AppendLine($"\t UserEligibleSettings");

                        roleSetting.UserEligibleSettings.ForEach(setting =>
                       {
                           sb.AppendLine($"\t\t{ PrintGovernanceRuleSetting(setting)}");
                       });
                    }

                    if (roleSetting?.UserMemberSettings?.Count() > 0)
                    {
                        sb.AppendLine($"\t UserMemberSettings:");

                        roleSetting.UserMemberSettings.ForEach(setting =>
                        {
                            sb.AppendLine($"\t\t{ PrintGovernanceRuleSetting(setting)}");
                        });
                    }

                    if (roleSetting?.AdditionalData?.Count() > 0)
                    {
                        sb.AppendLine($"\t AdditionalData:");

                        roleSetting.AdditionalData.ForEach(data =>
                        {
                            sb.AppendLine($"\t\t{data}");
                        });
                    }
                }

                sb.AppendLine("---Role settings end---");
            }

            return sb.ToString();
        }

        public string PrintGovernanceRuleSetting(Beta.GovernanceRuleSetting governanceRuleSetting, bool printVerbose = false)
        {
            StringBuilder sb = new StringBuilder();

            if (governanceRuleSetting != null)
            {
                sb.AppendLine($"RuleIdentifier:{governanceRuleSetting.RuleIdentifier}");
                if (!string.IsNullOrWhiteSpace(governanceRuleSetting.Setting))
                {
                    sb.AppendLine($"\t Settings:");

                    try
                    {
                        var settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(governanceRuleSetting.Setting);

                        settings.ForEach(data =>
                        {
                            sb.AppendLine($"\t\t{data.Key}:{data.Value}");
                        });
                    }
                    catch (Exception)
                    {
                        sb.AppendLine($"\t\t{governanceRuleSetting.Setting}");
                    }
                }

                if (printVerbose)
                {
                    if (governanceRuleSetting?.AdditionalData?.Count > 0)
                    {
                        sb.AppendLine($"\t AdditionalData");

                        governanceRuleSetting.AdditionalData.ForEach(data =>
                        {
                            sb.AppendLine($"\t\t{data}");
                        });
                    }
                }
            }
            return sb.ToString();
        }

        public async Task<string> PrintGovernanceRoleDefinitionAsync(Beta.GovernanceRoleDefinition governanceRoleDefinition, bool printVerbose = false)
        {
            StringBuilder sb = new StringBuilder();

            if (governanceRoleDefinition != null)
            {
                sb.AppendLine($"Id:{governanceRoleDefinition.Id}");
                sb.AppendLine($"DisplayName:{governanceRoleDefinition.DisplayName}");
                sb.AppendLine($"TemplateId:{governanceRoleDefinition.TemplateId}");
                sb.AppendLine($"ExternalId:{governanceRoleDefinition.ExternalId}");

                if (printVerbose)
                {
                    //if (governanceRoleDefinition.Resource != null)
                    //{
                    //    sb.AppendLine($"\tResource:{await PrintGovernanceResourceAsync(governanceRoleDefinition.Resource)}");
                    //}
                    sb.AppendLine($"ResourceId:{governanceRoleDefinition.ResourceId}");

                    if (governanceRoleDefinition.RoleSetting != null)
                    {
                        sb.AppendLine($"\tRoleSetting:{await PrintGovernanceRoleSettingAsync(governanceRoleDefinition.RoleSetting)}");
                    }

                    if (governanceRoleDefinition?.AdditionalData?.Count > 0)
                    {
                        sb.AppendLine($"\t AdditionalData");

                        governanceRoleDefinition.AdditionalData.ForEach(data =>
                        {
                            sb.AppendLine($"\t\t{data}");
                        });
                    }
                }
            }

            return sb.ToString();
        }

        public async Task<string> PrintGovernanceRoleAssignmentAsync(Beta.GovernanceRoleAssignment governanceRoleAssignment, bool printVerbose = false)
        {
            StringBuilder sb = new StringBuilder();

            if (governanceRoleAssignment != null)
            {
                sb.AppendLine($"Id:{governanceRoleAssignment.Id}");
                sb.AppendLine($"AssignmentState:{governanceRoleAssignment.AssignmentState}");
                sb.AppendLine($"StartDateTime:{governanceRoleAssignment.StartDateTime}");

                var resource = await GetGovernanceResourceByIdAsync(governanceRoleAssignment.ResourceId);
                sb.AppendLine(await PrintGovernanceResourceAsync(resource, false, false));

                //sb.AppendLine($"ResourceId:{governanceRoleAssignment.ResourceId}");
                sb.AppendLine($"MemberType:{governanceRoleAssignment.MemberType}");
                sb.AppendLine($"Status:{governanceRoleAssignment.Status}");
                if (!string.IsNullOrWhiteSpace(governanceRoleAssignment.SubjectId))
                {
                    var directoryObject = await _directoryObjectOperations.GetDirectoryObjectByIdAsync(governanceRoleAssignment.SubjectId);
                    sb.AppendLine($"\tSubjectId:{ _directoryObjectOperations.PrintDirectoryObject(directoryObject, false)}");
                }
                sb.AppendLine($"\tSubject:{PrintGovernanceSubject(governanceRoleAssignment.Subject, printVerbose)}");

                if (printVerbose)
                {
                    if (governanceRoleAssignment.LinkedEligibleRoleAssignment != null)
                    {
                        sb.AppendLine($"\t LinkedEligibleRoleAssignment: {await PrintGovernanceRoleAssignmentAsync(governanceRoleAssignment.LinkedEligibleRoleAssignment)}");
                        sb.AppendLine($"LinkedEligibleRoleAssignmentId:{governanceRoleAssignment.LinkedEligibleRoleAssignmentId}");
                    }

                    if (governanceRoleAssignment?.AdditionalData?.Count > 0)
                    {
                        sb.AppendLine($"\t AdditionalData");

                        governanceRoleAssignment.AdditionalData.ForEach(data =>
                        {
                            sb.AppendLine($"\t\t{data}");
                        });
                    }
                }
            }
            return sb.ToString();
        }

        public async Task<string> PrintGovernanceRoleAssignmentRequestAsync(Beta.GovernanceRoleAssignmentRequestObject governanceRoleAssignmentRequest, bool printVerbose = false)
        {
            StringBuilder sb = new StringBuilder();

            if (governanceRoleAssignmentRequest != null)
            {
                sb.AppendLine($"Id:{governanceRoleAssignmentRequest.Id}");
                sb.AppendLine($"AssignmentState:{governanceRoleAssignmentRequest.AssignmentState}");
                sb.AppendLine($"Reason:{governanceRoleAssignmentRequest.Reason}");
                sb.AppendLine($"ResourceId:{governanceRoleAssignmentRequest.ResourceId}");
                sb.AppendLine($"RequestedDateTime:{governanceRoleAssignmentRequest.RequestedDateTime}");
                sb.AppendLine($"Status:{governanceRoleAssignmentRequest.Status}");
                sb.AppendLine($"SubjectId:{governanceRoleAssignmentRequest.SubjectId}");

                if (!string.IsNullOrWhiteSpace(governanceRoleAssignmentRequest.SubjectId))
                {
                    var user = await _userOperations.GetUserByIdAsync(governanceRoleAssignmentRequest.SubjectId);
                    sb.AppendLine($"\tSubjectId:{ _userOperations.PrintBetaUserDetails(user, false)}");
                }

                sb.AppendLine($"\tSubject:{PrintGovernanceSubject(governanceRoleAssignmentRequest.Subject, printVerbose)}");
                sb.AppendLine($"LinkedEligibleRoleAssignmentId:{governanceRoleAssignmentRequest.LinkedEligibleRoleAssignmentId}");

                if (printVerbose)
                {
                    if (governanceRoleAssignmentRequest?.AdditionalData?.Count > 0)
                    {
                        sb.AppendLine($"\t AdditionalData");

                        governanceRoleAssignmentRequest.AdditionalData.ForEach(data =>
                        {
                            sb.AppendLine($"\t\t{data}");
                        });
                    }
                }
            }
            return sb.ToString();
        }

        public string PrintGovernanceSubject(Beta.GovernanceSubject governanceSubject, bool printVerbose = false)
        {
            StringBuilder sb = new StringBuilder();

            if (governanceSubject != null)
            {
                sb.AppendLine($"Id:{governanceSubject.Id}");
                sb.AppendLine($"Type:{governanceSubject.Type}");
                sb.AppendLine($"DisplayName:{governanceSubject.DisplayName}");
                sb.AppendLine($"PrincipalName:{governanceSubject.PrincipalName}");
                sb.AppendLine($"Email:{governanceSubject.Email}");

                if (printVerbose)
                {
                    if (governanceSubject?.AdditionalData?.Count > 0)
                    {
                        sb.AppendLine($"\t AdditionalData");

                        governanceSubject.AdditionalData.ForEach(data =>
                        {
                            sb.AppendLine($"\t\t{data}");
                        });
                    }
                }
            }
            return sb.ToString();
        }

        public string PrintGovernanceSchedule(Beta.GovernanceSchedule governanceSchedule, bool printVerbose = false)
        {
            StringBuilder sb = new StringBuilder();

            if (governanceSchedule != null)
            {
                sb.AppendLine($"Id:{governanceSchedule.StartDateTime}");
                sb.AppendLine($"EndDateTime:{governanceSchedule.EndDateTime}");
                sb.AppendLine($"Duration:{governanceSchedule.Duration}");
                sb.AppendLine($"Type:{governanceSchedule.Type}");

                if (printVerbose)
                {
                    if (governanceSchedule?.AdditionalData?.Count > 0)
                    {
                        sb.AppendLine($"\t AdditionalData");

                        governanceSchedule.AdditionalData.ForEach(data =>
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
                        foreach (var governanceResource in governanceResources.CurrentPage)
                        {
                            allPrivilegedAccessReourcess.Add(governanceResource);
                            _CachedGovernanceResources[governanceResource.ExternalId] = governanceResource;
                            AllTypes.Add(governanceResource.Type);
                            AllStatuses.Add(governanceResource.Status);
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

        private async Task<List<Beta.GovernanceRoleAssignment>> ProcessIPrivilegedAccessRoleAssignmentsCollectionPage(Beta.IPrivilegedAccessRoleAssignmentsCollectionPage roleassignments)
        {
            List<Beta.GovernanceRoleAssignment> allGovernanceRoleAssignments = new List<Beta.GovernanceRoleAssignment>();

            try
            {
                if (roleassignments != null)
                {
                    do
                    {
                        // Page through results
                        foreach (var roleAssignment in roleassignments.CurrentPage)
                        {
                            allGovernanceRoleAssignments.Add(roleAssignment);
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
                Console.WriteLine($"We could not process the privileged access role assignments list: {e}");
                return null;
            }

            return allGovernanceRoleAssignments;
        }
    }
}