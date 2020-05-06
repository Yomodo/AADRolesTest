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

namespace AuthNMethodsTesting
{
    public class ConditionalAccessPolicyOperations
    {
        private Beta.GraphServiceClient _graphServiceClient;
        private UserOperations _userOperations;
        private ServicePrincipalOperations _servicePrincipalOperations;
        private GroupOperations _groupOperations;
        private NamedLocationOperations _namedLocationOperations;

        public ConditionalAccessPolicyOperations(Beta.GraphServiceClient graphServiceClient, UserOperations userOperations, ServicePrincipalOperations servicePrincipalOperations, GroupOperations groupOperations)
        {
            this._graphServiceClient = graphServiceClient;
            this._userOperations = userOperations;
            this._servicePrincipalOperations = servicePrincipalOperations;
            this._groupOperations = groupOperations;
            this._namedLocationOperations = new NamedLocationOperations(this._graphServiceClient);
        }

        public async Task<List<Beta.ConditionalAccessPolicy>> ListConditionalAccessPoliciesAsync()
        {
            List<Beta.ConditionalAccessPolicy> allPolicies = new List<Beta.ConditionalAccessPolicy>();
            Beta.IConditionalAccessRootPoliciesCollectionPage policies = null;

            try
            {
                await _namedLocationOperations.ListNamedLocationsAsync();

                policies = await _graphServiceClient.Identity.ConditionalAccess.Policies.Request().GetAsync();

                if (policies != null)
                {
                    allPolicies = await ProcessIConditionalAccessRootPoliciesCollectionPage(policies);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not retrieve the conditional access policies: {e}");
                return null;
            }

            return allPolicies;
        }

        public async Task<string> PrintConditionalAccessPolicyAsync(Beta.ConditionalAccessPolicy conditionalAccessPolicy, bool verbose = false)
        {
            string toPrint = string.Empty;
            StringBuilder more = new StringBuilder();

            if (conditionalAccessPolicy != null)
            {
                Console.WriteLine($"Processing CA policy {conditionalAccessPolicy.DisplayName}");

                toPrint = $"DisplayName-{conditionalAccessPolicy.DisplayName}, State-{conditionalAccessPolicy.State}";

                if (verbose)
                {
                    toPrint = toPrint + $", Id-{conditionalAccessPolicy.Id}, CreatedDateTime-{conditionalAccessPolicy.CreatedDateTime}, ModifiedDateTime-{conditionalAccessPolicy.ModifiedDateTime}";
                    more.AppendLine("");

                    #region Conditions

                    // applications
                    more.AppendLine($"\tApplications");

                    if (conditionalAccessPolicy.Conditions.Applications.IncludeApplications.Count() > 0)
                    {
                        more.AppendLine($"\t\tIncluded applications");

                        await conditionalAccessPolicy.Conditions.Applications.IncludeApplications.ForEachAsync(async appId =>
                        {
                            Guid actualAppId;
                            bool isguid = Guid.TryParse(appId, out actualAppId);

                            if (isguid)
                            {
                                var app = await _servicePrincipalOperations.GetServicePrincipalByAppIdAsync(appId);
                                more.AppendLine($"\t\t\t{app.DisplayName}");
                            }
                            else
                            {
                                more.AppendLine($"\t\t\t{appId}");
                            }
                        });
                    }

                    if (conditionalAccessPolicy.Conditions.Applications.ExcludeApplications.Count() > 0)
                    {
                        more.AppendLine($"\tExcluded applications");

                        await conditionalAccessPolicy.Conditions.Applications.ExcludeApplications.ForEachAsync(async appId =>
                        {
                            Guid actualAppId;
                            bool isguid = Guid.TryParse(appId, out actualAppId);

                            if (isguid)
                            {
                                var app = await _servicePrincipalOperations.GetServicePrincipalByAppIdAsync(appId);
                                more.AppendLine($"\t\t\t{app.DisplayName}");
                            }
                            else
                            {
                                more.AppendLine($"\t\t\t{appId}");
                            }
                        });
                    }

                    if (conditionalAccessPolicy.Conditions.Applications.IncludeUserActions.Count() > 0)
                    {
                        more.AppendLine($"\tIncluded User Actions");

                        await conditionalAccessPolicy.Conditions.Applications.IncludeUserActions.ForEachAsync(act =>
                        {
                            more.AppendLine($"\t\t\t{act}");
                        });
                    }

                    if (conditionalAccessPolicy.Conditions.Applications?.AdditionalData?.Count() > 0)
                    {
                        more.AppendLine($"\tAdditional Data");

                        await conditionalAccessPolicy.Conditions.Applications.AdditionalData.ForEachAsync(data =>
                        {
                            more.AppendLine($"\t\t\t{data}");
                        });
                    }

                    // Client app types
                    if (conditionalAccessPolicy.Conditions.ClientAppTypes.Count() > 0)
                    {
                        more.AppendLine($"\tClient App Types");

                        await conditionalAccessPolicy.Conditions.ClientAppTypes.ForEachAsync(app =>
                        {
                            more.AppendLine($"\t\t{app}");
                        });
                    }

                    // Device states
                    more.AppendLine($"\tDevice states");

                    if (conditionalAccessPolicy.Conditions?.DeviceStates?.IncludeStates?.Count() > 0)
                    {
                        more.AppendLine($"\t\tDevice included states");

                        conditionalAccessPolicy.Conditions?.DeviceStates?.IncludeStates.ForEachAsync(state =>
                        {
                            more.AppendLine($"\t\t\t{state}");
                        });
                    }

                    if (conditionalAccessPolicy.Conditions?.DeviceStates?.ExcludeStates?.Count() > 0)
                    {
                        more.AppendLine($"\t\tDevice excluded states");

                        conditionalAccessPolicy.Conditions?.DeviceStates?.ExcludeStates.ForEachAsync(state =>
                        {
                            more.AppendLine($"\t\t\t{state}");
                        });
                    }

                    if (conditionalAccessPolicy.Conditions.DeviceStates?.AdditionalData?.Count() > 0)
                    {
                        more.AppendLine($"\t AdditionalData");

                        conditionalAccessPolicy.Conditions?.DeviceStates?.AdditionalData.ForEachAsync(data =>
                        {
                            more.AppendLine($"\t\t\t{data}");
                        });
                    }

                    // Users
                    more.AppendLine($"\tUsers");

                    if (conditionalAccessPolicy.Conditions.Users.IncludeUsers.Count() > 0)
                    {
                        more.AppendLine($"\t Included users");

                        await conditionalAccessPolicy.Conditions.Users.IncludeUsers.ForEachAsync(async userId =>
                        {
                            Guid actualUserId;
                            bool isguid = Guid.TryParse(userId, out actualUserId);

                            if (isguid)
                            {
                                var user = await _userOperations.GetUserByIdAsync(userId);
                                more.AppendLine($"\t\t\t{ _userOperations.PrintBetaUserDetails(user, false, userId)}");
                            }
                            else
                            {
                                more.AppendLine($"\t\t\t{userId}");
                            }
                        });
                    }

                    if (conditionalAccessPolicy.Conditions.Users.ExcludeUsers.Count() > 0)
                    {
                        more.AppendLine($"\t Excluded users");

                        await conditionalAccessPolicy.Conditions.Users.ExcludeUsers.ForEachAsync(async userId =>
                        {
                            Guid actualUserId;
                            bool isguid = Guid.TryParse(userId, out actualUserId);

                            if (isguid)
                            {
                                var user = await _userOperations.GetUserByIdAsync(userId);
                                more.AppendLine($"\t\t\t{ _userOperations.PrintBetaUserDetails(user, false, userId)}");
                            }
                            else
                            {
                                more.AppendLine($"\t\t\t{userId}");
                            }
                        });
                    }

                    // Groups
                    more.AppendLine($"\tGroups");

                    if (conditionalAccessPolicy.Conditions?.Users?.IncludeGroups.Count() > 0)
                    {
                        more.AppendLine($"\t Included groups");

                        conditionalAccessPolicy.Conditions?.Users.IncludeGroups.ForEachAsync(async grpId =>
                        {
                            var Group = await _groupOperations.GetGroupByIdAsync(grpId);
                            more.AppendLine($"\t\t\t{_groupOperations.PrintGroupBasic(Group)}");
                        });
                    }

                    if (conditionalAccessPolicy?.Conditions?.Users?.ExcludeGroups.Count() > 0)
                    {
                        more.AppendLine($"\t Excluded groups");

                        await conditionalAccessPolicy.Conditions.Users.ExcludeGroups.ForEachAsync(async grpId =>
                        {
                            var Group = await _groupOperations.GetGroupByIdAsync(grpId);
                            more.AppendLine($"\t\t\t{Group.DisplayName}");
                        });
                    }

                    // Roles
                    more.AppendLine($"\tRoles");
                    if (conditionalAccessPolicy?.Conditions?.Users?.IncludeRoles.Count() > 0)
                    {
                        more.AppendLine($"\t Included roles");

                        await conditionalAccessPolicy.Conditions.Users.IncludeRoles.ForEachAsync(role =>
                        {
                            more.AppendLine($"\t\t\t{role}");
                        });
                    }

                    if (conditionalAccessPolicy.Conditions?.Users?.ExcludeRoles.Count() > 0)
                    {
                        more.AppendLine($"\t Excluded roles");

                        await conditionalAccessPolicy.Conditions.Users.ExcludeRoles.ForEachAsync(role =>
                        {
                            more.AppendLine($"\t\t\t{role}");
                        });
                    }

                    if (conditionalAccessPolicy.Conditions.Users?.AdditionalData?.Count() > 0)
                    {
                        more.AppendLine($"\t AdditionalData");

                        await conditionalAccessPolicy.Conditions.Users.AdditionalData.ForEachAsync(data =>
                        {
                            more.AppendLine($"\t\t\t{data}");
                        });
                    }

                    // Locations
                    more.AppendLine($"\tLocations");
                    if (conditionalAccessPolicy.Conditions?.Locations?.IncludeLocations.Count() > 0)
                    {
                        more.AppendLine($"\t Included locations");

                        conditionalAccessPolicy.Conditions?.Locations?.IncludeLocations.ForEachAsync(async locationId =>
                        {
                            Guid actualLocationId;
                            bool isguid = Guid.TryParse(locationId, out actualLocationId);

                            if (isguid)
                            {
                                var location = await this._namedLocationOperations.GetNamedLocationByIdAsync(locationId);
                                more.AppendLine($"\t\t\t{this._namedLocationOperations.PrintNamedLocation(location)}");
                            }
                        });
                    }

                    if (conditionalAccessPolicy.Conditions?.Locations?.ExcludeLocations.Count() > 0)
                    {
                        more.AppendLine($"\t Excluded locations");

                        conditionalAccessPolicy.Conditions?.Locations?.ExcludeLocations.ForEachAsync(async locationId =>
                        {
                            Guid actualLocationId;
                            bool isguid = Guid.TryParse(locationId, out actualLocationId);

                            if (isguid)
                            {
                                var location = await this._namedLocationOperations.GetNamedLocationByIdAsync(locationId);
                                more.AppendLine($"\t\t\t{this._namedLocationOperations.PrintNamedLocation(location)}");
                            }
                        });
                    }

                    if (conditionalAccessPolicy.Conditions?.Locations?.AdditionalData?.Count() > 0)
                    {
                        more.AppendLine($"\t AdditionalData");

                        await conditionalAccessPolicy.Conditions.Locations.AdditionalData.ForEachAsync(data =>
                        {
                            more.AppendLine($"\t\t\t{data}");
                        });
                    }

                    // Platforms
                    more.AppendLine($"\tPlatforms");
                    if (conditionalAccessPolicy.Conditions?.Platforms?.IncludePlatforms.Count() > 0)
                    {
                        more.AppendLine($"\t Included Platforms");

                        conditionalAccessPolicy.Conditions?.Platforms?.IncludePlatforms.ForEachAsync(platform =>
                        {
                            more.AppendLine($"\t\t\t{platform}");
                        });
                    }

                    if (conditionalAccessPolicy.Conditions?.Platforms?.ExcludePlatforms.Count() > 0)
                    {
                        more.AppendLine($"\t Excluded Platforms");

                        conditionalAccessPolicy.Conditions?.Platforms?.ExcludePlatforms.ForEachAsync(platform =>
                        {
                            more.AppendLine($"\t\t\t{platform}");
                        });
                    }

                    if (conditionalAccessPolicy.Conditions?.Platforms?.AdditionalData?.Count() > 0)
                    {
                        more.AppendLine($"\t AdditionalData");

                        conditionalAccessPolicy.Conditions?.Platforms?.AdditionalData.ForEachAsync(data =>
                        {
                            more.AppendLine($"\t\t\t{data}");
                        });
                    }

                    #endregion Conditions

                    // Grant controls
                    more.AppendLine($"\tGrant Controls");
                    more.AppendLine($"\tOperator-{conditionalAccessPolicy?.GrantControls?.Operator}");

                    if (conditionalAccessPolicy?.GrantControls?.BuiltInControls.Count() > 0)
                    {
                        more.AppendLine($"\tBuiltIn Controls");

                        conditionalAccessPolicy?.GrantControls?.BuiltInControls.ForEachAsync(control =>
                        {
                            more.AppendLine($"\t\t{control}");
                        });
                    }

                    if (conditionalAccessPolicy?.GrantControls?.CustomAuthenticationFactors.Count() > 0)
                    {
                        more.AppendLine($"\tCustom Authentication Factors");

                        conditionalAccessPolicy?.GrantControls?.CustomAuthenticationFactors.ForEachAsync(control =>
                        {
                            more.AppendLine($"\t\t{control}");
                        });
                    }

                    if (conditionalAccessPolicy?.GrantControls?.TermsOfUse.Count() > 0)
                    {
                        more.AppendLine($"\tTerms Of Use");

                        conditionalAccessPolicy?.GrantControls?.TermsOfUse.ForEachAsync(control =>
                        {
                            more.AppendLine($"\t\t{control}");
                        });
                    }

                    if (conditionalAccessPolicy?.GrantControls?.AdditionalData?.Count() > 0)
                    {
                        more.AppendLine($"\tAdditional Data");

                        conditionalAccessPolicy?.Conditions.Applications.AdditionalData.ForEachAsync(data =>
                        {
                            more.AppendLine($"\t\t{data}");
                        });
                    }

                    more.AppendLine($"\tSession Controls");

                    if (conditionalAccessPolicy?.SessionControls?.SignInFrequency != null)
                    {
                        var signInFrequency = conditionalAccessPolicy.SessionControls.SignInFrequency;

                        more.AppendLine($"\t\t SignInFrequency - IsEnabled-{signInFrequency.IsEnabled}, Type- {signInFrequency.Type}, Value-{signInFrequency?.Type.Value}");
                    }

                    if (conditionalAccessPolicy?.SessionControls?.PersistentBrowser != null)
                    {
                        var persistentBrowser = conditionalAccessPolicy.SessionControls.PersistentBrowser;

                        more.AppendLine($"\t\t PersistentBrowser - IsEnabled-{persistentBrowser.IsEnabled}, Mode- {persistentBrowser.Mode}");
                    }

                    if (conditionalAccessPolicy?.SessionControls?.ApplicationEnforcedRestrictions != null)
                    {
                        var appEnforcedRestrictions = conditionalAccessPolicy.SessionControls.ApplicationEnforcedRestrictions;

                        more.AppendLine($"\t\t Application Enforced Restrictions - IsEnabled-{appEnforcedRestrictions.IsEnabled}");
                    }

                    if (conditionalAccessPolicy?.SessionControls?.CloudAppSecurity != null)
                    {
                        var cloudAppSecurity = conditionalAccessPolicy.SessionControls.CloudAppSecurity;

                        more.AppendLine($"\t\t Cloud App Security - IsEnabled-{cloudAppSecurity.IsEnabled}, Value- {cloudAppSecurity?.CloudAppSecurityType.Value}");
                    }
                }
            }
            else
            {
                ColorConsole.WriteLine(ConsoleColor.Red, $"The provided conditional access is null");
            }

            return toPrint + more.ToString();
        }

        private async Task<List<Beta.ConditionalAccessPolicy>> ProcessIConditionalAccessRootPoliciesCollectionPage(Beta.IConditionalAccessRootPoliciesCollectionPage conditionalAccessPolicies)
        {
            List<Beta.ConditionalAccessPolicy> allconditionalAccessPolicies = new List<Beta.ConditionalAccessPolicy>();

            try
            {
                if (conditionalAccessPolicies != null)
                {
                    do
                    {
                        // Page through results
                        foreach (var conditionalAccessPolicy in conditionalAccessPolicies.CurrentPage)
                        {
                            //Console.WriteLine($"Role:{conditionalAccessPolicy.DisplayName}");
                            allconditionalAccessPolicies.Add(conditionalAccessPolicy);
                        }

                        // are there more pages (Has a @odata.nextLink ?)
                        if (conditionalAccessPolicies.NextPageRequest != null)
                        {
                            conditionalAccessPolicies = await conditionalAccessPolicies.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            conditionalAccessPolicies = null;
                        }
                    } while (conditionalAccessPolicies != null);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not process the conditional Access Policys list: {e}");
                return null;
            }

            return allconditionalAccessPolicies;
        }
    }
}