extern alias BetaLib;

using Common;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beta = BetaLib.Microsoft.Graph;

namespace AADGraphTesting
{
    public class ApplicationOperations
    {
        private Beta.GraphServiceClient _graphServiceClient;

        public ApplicationOperations(Beta.GraphServiceClient graphServiceClient)
        {
            this._graphServiceClient = graphServiceClient;
        }

        public async Task<List<Beta.Application>> GetAllApplicationsAsync()
        {
            //// Supported and works
            //var a = await _graphServiceClient.Me.AppRoleAssignments.Request().GetAsync();

            //// Not navigable today, thus no way to retrieve the assignments for a service principal or group
            //var b = await _graphServiceClient.Groups.AppRoleAssignments.Request().GetAsync();
            //var c = await _graphServiceClient.ServicePrincipals.AppRoleAssignments.Request().GetAsync();

            //// this throws the unsupported query error
            //var d = await _graphServiceClient.AppRoleAssignments.Request().GetAsync()

            List<Beta.Application> allApplications = new List<Beta.Application>();

            try
            {
                Beta.IGraphServiceApplicationsCollectionPage applications = await _graphServiceClient.Applications.Request().GetAsync();

                if (applications?.CurrentPage.Count > 0)
                {
                    foreach (Beta.Application application in applications)
                    {
                        allApplications.Add(application);
                    }
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not retrieve the applications: {e}");
                return null;
            }

            return allApplications;
        }

        private async Task<Beta.RequiredResourceAccess> GetApplicationRolesByValueAsync(string apiDisplayName, IList<string> appRoleValues)
        {
            Beta.RequiredResourceAccess requiredResourceAccess = null;

            // ResourceAppId of Microsoft Graph
            Beta.ServicePrincipal servicePrincipal = await GetServicePrincipalByAppDisplayNameAsync(apiDisplayName);

            if (servicePrincipal != null)
            {
                requiredResourceAccess = new Beta.RequiredResourceAccess() { ResourceAppId = servicePrincipal.AppId };
                IList<Beta.ResourceAccess> resourceAccesses = new List<Beta.ResourceAccess>();

                appRoleValues.ToList().ForEach(roleValue =>
                {
                    Beta.AppRole appRole = servicePrincipal.AppRoles.Where(x => x.Value == roleValue).FirstOrDefault();

                    if (appRole != null)
                    {
                        resourceAccesses.Add(new Beta.ResourceAccess() { Type = "Role", Id = appRole.Id });
                    }
                });

                if (resourceAccesses.Count() > 0)
                {
                    requiredResourceAccess.ResourceAccess = resourceAccesses;
                }
            }
            else
            {
                ColorConsole.WriteLine(ConsoleColor.Red, $"No service principal matching '{apiDisplayName}' found in the tenant");
            }

            return requiredResourceAccess;
        }



        public async Task PrintApplicationDetailsAsync(Beta.Application application)
        {
            if (application != null)
            {
                UserOperations userOperations = new UserOperations(_graphServiceClient);

                Console.WriteLine($"--------------------------------Application '{application.DisplayName}' start----------------------------------------");
                Console.WriteLine($"Id-{application.Id}, AppId- {application.AppId}, DisplayName-{application.DisplayName}, " +
                    $"SignInAudience-{application.SignInAudience}, " +
                    $"GroupMembershipClaims-{application?.GroupMembershipClaims}");

                if (application?.Owners?.Count > 0)
                {
                    Console.WriteLine("--------------------Owners-------------------");
                    foreach (var owner in application.Owners)
                    {
                        Beta.User userOwner = await userOperations.GetUserByIdAsync(owner);

                        Console.WriteLine(userOperations.PrintBetaUserDetails(userOwner));
                    }
                    Console.WriteLine("----------------------------------------------------------");
                }

                if (application?.AppRoles?.Count() > 0)
                {
                    Console.WriteLine("--------------------------AppRoles-------------------");
                    foreach (var appRole in application.AppRoles)
                    {
                        Console.WriteLine($"Id-{appRole.Id}, IsEnabled- {appRole.IsEnabled}, UserConsentDisplayName-{appRole.Value}, " +
                            $"AllowedMemberTypes- {String.Join(",", appRole.AllowedMemberTypes.ToList())}");
                    }
                    Console.WriteLine("----------------------------------------------------------");
                }

                if (application.Web != null)
                {
                    Console.WriteLine("--------------------------Web App-------------------");

                    Console.WriteLine("Redirect Uris");
                    if (application.Web.RedirectUris != null && application.Web.RedirectUris.Count() > 0)
                    {
                        application.Web.RedirectUris.ToList().ForEach(x => Console.WriteLine($"     {x}"));
                    }

                    Console.WriteLine($"    Oauth2AllowImplicitFlow-'{application.Web?.Oauth2AllowImplicitFlow}'");

                    if (application?.Web?.AdditionalData?.Count > 0)
                    {
                        Console.WriteLine("--------------------------Application.Web.AdditionalData start-------------------");
                        Console.WriteLine(application?.Web.AdditionalData.ToDebugString());
                        Console.WriteLine("--------------------------Application.Web.AdditionalData end-------------------");
                    }

                    Console.WriteLine("----------------------------------------------------------");
                }

                if (application.RequiredResourceAccess != null)
                {
                    if (application.RequiredResourceAccess.Count() > 0)
                    {
                        Console.WriteLine("--------------------------RequiredResourceAccess-------------------");

                        foreach (var requiredResourceAccess in application.RequiredResourceAccess)
                        {
                            string resourceappName = string.Empty;

                            // Search for service principal first
                            Beta.ServicePrincipal servicePrincipal = await GetServicePrincipalByAppIdAsync(requiredResourceAccess.ResourceAppId);

                            if (servicePrincipal == null)
                            {
                                Beta.Application resourceApplication = await GetApplicationByAppIdAsync(requiredResourceAccess.ResourceAppId);
                                resourceappName = resourceApplication.DisplayName;
                            }
                            else
                            {
                                resourceappName = servicePrincipal.DisplayName;
                            }
                            Console.WriteLine($"ResourceAppId-{requiredResourceAccess.ResourceAppId}, Resource-{resourceappName} ");

                            foreach (var resourceAccess in requiredResourceAccess.ResourceAccess)
                            {
                                Beta.AppRole role = servicePrincipal.AppRoles.FirstOrDefault(x => x.Id == resourceAccess.Id);

                                if (role != null)
                                {
                                    Console.WriteLine($"    Id-{resourceAccess.Id}, Value-{role.Value}, DisplayName-{role.DisplayName}, Origin-{role.Origin}, " +
                                        $"Type-{resourceAccess.Type} ");
                                }
                                else
                                {
                                    //Beta.OAuth2Permission oauth2Permission = servicePrincipal.Oauth2Permissions.FirstOrDefault(x => x.Id == resourceAccess.Id);

                                    //Console.WriteLine($"    Id-{resourceAccess.Id}, Value-{oauth2Permission.Value}, UserConsentDisplayName-{oauth2Permission.UserConsentDisplayName}, " +
                                    //    $"Origin-{oauth2Permission?.Origin}, Type-{oauth2Permission.Type} ");
                                }
                            }
                        }

                        Console.WriteLine("----------------------------------------------------------");
                    }
                }

                if (application?.IdentifierUris.ToList().Count > 0)
                {
                    Console.WriteLine("--------------------------Api-------------------");

                    foreach (var identifierUri in application.IdentifierUris)
                    {
                        Console.WriteLine($"    identifierUri-'{identifierUri}'");
                    }

                    Console.WriteLine($"    RequestedAccessTokenVersion-'{application?.Api?.RequestedAccessTokenVersion}', AcceptMappedClaims - {application?.Api?.AcceptMappedClaims}");

                    foreach (var oauth2PermissionScope in application.Api.Oauth2PermissionScopes)
                    {
                        Console.WriteLine($"    Id-{oauth2PermissionScope.Id}, Type- {oauth2PermissionScope.Type}, " +
                            $"UserConsentDisplayName-{oauth2PermissionScope.UserConsentDisplayName}, AdminConsentDisplayName-{oauth2PermissionScope.AdminConsentDisplayName}, " +
                            $"IsEnabled-{oauth2PermissionScope.IsEnabled}");
                    }

                    foreach (var item in application.Api.KnownClientApplications)
                    {
                        Console.WriteLine("--------------------------KnownClientApplications-------------------");
                        application.Api.KnownClientApplications.ToList().ForEach(pz => Console.WriteLine($"KCA-{pz}"));
                        Console.WriteLine("----------------------------------------------------------");
                    }

                    foreach (var item in application.Api.PreAuthorizedApplications)
                    {
                        Console.WriteLine("--------------------------PreAuthorizedApplications-------------------");
                        foreach (var preAuthorizedApplication in application.Api.PreAuthorizedApplications)
                        {
                            Console.WriteLine($"AppId-{preAuthorizedApplication.AppId}");

                            preAuthorizedApplication.PermissionIds.ToList().ForEach(pz => Console.WriteLine($"Pid-{pz}"));
                        }
                        Console.WriteLine("----------------------------------------------------------");
                    }

                    if (application?.Api?.AdditionalData.Count > 0)
                    {
                        Console.WriteLine("--------------------------Application.Api.AdditionalData start-------------------");
                        Console.WriteLine(application?.Api?.AdditionalData.ToDebugString());
                        Console.WriteLine("--------------------------Application.Api.AdditionalData end-------------------");
                    }

                    Console.WriteLine("----------------------------------------------------------");
                }

                if (application?.KeyCredentials?.Count() > 0)
                {
                    Console.WriteLine("--------------------------KeyCredentials-------------------");
                    foreach (var keyCredential in application.KeyCredentials)
                    {
                        Console.WriteLine($"DisplayName-{keyCredential?.DisplayName}, KeyId- {keyCredential.KeyId}, StartDateTime- {keyCredential.StartDateTime}, EndDateTime- {keyCredential.EndDateTime} "
                            + $"Key-{keyCredential.Key}, Type-{keyCredential.Type}, Usage-{keyCredential.Usage}");
                    }
                    Console.WriteLine("----------------------------------------------------------");
                }

                if (application?.PasswordCredentials?.Count() > 0)
                {
                    Console.WriteLine("--------------------------PasswordCredentials-------------------");
                    foreach (var passwordCredential in application.PasswordCredentials)
                    {
                        Console.WriteLine($"DisplayName-{passwordCredential?.DisplayName}, KeyId- {passwordCredential.KeyId}, StartDateTime- {passwordCredential.StartDateTime}, EndDateTime- {passwordCredential.EndDateTime} "
                            + $"Hint-{passwordCredential.Hint}, SecretText-{passwordCredential.SecretText}, Hint-{passwordCredential?.Hint}");
                    }
                    Console.WriteLine("----------------------------------------------------------");
                }

                if (application?.OptionalClaims?.AccessToken.Count() > 0)
                {
                    Console.WriteLine("--------------------------OptionalClaims.AccessToken-------------------");
                    foreach (var optionalClaim in application.OptionalClaims.AccessToken)
                    {
                        Console.WriteLine($"Name-{optionalClaim.Name}, Source- {optionalClaim.Source}, Essential- {optionalClaim.Essential}");
                    }
                    Console.WriteLine("----------------------------------------------------------");
                }

                if (application?.OptionalClaims?.IdToken.Count() > 0)
                {
                    Console.WriteLine("--------------------------OptionalClaims.IdToken-------------------");
                    foreach (var optionalClaim in application.OptionalClaims.IdToken)
                    {
                        Console.WriteLine($"Name-{optionalClaim.Name}, Source- {optionalClaim.Source}, Essential- {optionalClaim.Essential}");
                    }
                    Console.WriteLine("----------------------------------------------------------");
                }

                if (application.Tags != null && application.Tags.Count() > 0)
                {
                    Console.WriteLine("--------------------------Tags-------------------");

                    application.Tags.ToList().ForEach(x => Console.WriteLine($"{x}"));

                    Console.WriteLine("----------------------------------------------------------");
                }

                if (application?.AdditionalData.Count > 0)
                {
                    Console.WriteLine("--------------------------Application.AdditionalData start-------------------");
                    Console.WriteLine(application?.AdditionalData.ToDebugString());
                    Console.WriteLine("--------------------------Application.AdditionalData end-------------------");
                }

                Console.WriteLine($"--------------------------------Application '{application.DisplayName}' end----------------------------------------");
                Console.WriteLine("");
            }
            else
            {
                Console.WriteLine("The provided Application is null!");
            }
        }

        public async Task PrintServicePrincipalDetailsAsync(Beta.Application application)
        {
            Console.WriteLine("");
            Console.WriteLine($"--------------------------------ServicePrincipal '{application.DisplayName}' start----------------------------------------");

            Beta.ServicePrincipal servicePrincipal = await GetServicePrincipalByAppIdAsync(application.AppId);

            if (servicePrincipal == null)
            {
                ColorConsole.WriteLine(ConsoleColor.Red, $"NO SERVICE PRINCIPAL FOR '{application.DisplayName}' FOUND !!");
            }
            else
            {
                GroupOperations groupOperations = new GroupOperations(_graphServiceClient);
                UserOperations userOperations = new UserOperations(_graphServiceClient);

                Console.WriteLine($"Id-{servicePrincipal.Id}, Enabled- {servicePrincipal.AccountEnabled}, AppDisplayName-{servicePrincipal.AppDisplayName}, AppId-{servicePrincipal.AppId}, " +
                $"AppOwnerOrganizationId-{servicePrincipal.AppOwnerOrganizationId}, AppRoleAssignmentRequired-{servicePrincipal?.AppRoleAssignmentRequired}, " +
                $"DisplayName-{servicePrincipal.DisplayName}, Homepage-{servicePrincipal?.Homepage}PreferredTokenSigningKeyThumbprint-{servicePrincipal?.PreferredTokenSigningKeyThumbprint}, " +
                $"PublisherName-{servicePrincipal.PublisherName}, Homepage-{servicePrincipal?.Homepage}, PreferredTokenSigningKeyThumbprint-{servicePrincipal?.PreferredTokenSigningKeyThumbprint}");

                if (servicePrincipal?.Owners?.Count > 0)
                {
                    Console.WriteLine("--------------------Owners-------------------");
                    foreach (var owner in servicePrincipal.Owners)
                    {
                        Beta.User userOwner = await userOperations.GetUserByIdAsync(owner);

                        Console.WriteLine(userOperations.PrintBetaUserDetails(userOwner));
                    }
                    Console.WriteLine("----------------------------------------------------------");
                }

                Dictionary<Guid?, Beta.AppRole> approles = new Dictionary<Guid?, Beta.AppRole>();

                if (servicePrincipal?.AppRoles?.Count() > 0)
                {
                    Console.WriteLine("--------------------------AppRoles-------------------");
                    foreach (var appRole in servicePrincipal.AppRoles)
                    {
                        approles.Add(appRole.Id, appRole);
                        Console.WriteLine($"Id-{appRole.Id}, IsEnabled- {appRole.IsEnabled}, UserConsentDisplayName-{appRole.Value}, AllowedMemberTypes- {String.Join(",", appRole.AllowedMemberTypes)}");
                    }
                    Console.WriteLine("----------------------------------------------------------");
                }

                var approleassignments = await GetServicePrincipalsAppRoleAssignedToAsync(servicePrincipal);

                if (approleassignments?.Count > 0)
                {
                    Console.WriteLine("--------------------AppRole Assignments-------------------");
                    foreach (var approleassignment in approleassignments)
                    {
                        Console.WriteLine($"PrincipalDisplayName - '{approleassignment.PrincipalDisplayName}'" +
                            $", AppRole- '{approles[approleassignment.AppRoleId].DisplayName}'" +
                            $", PrincipalType- '{approleassignment.PrincipalType}'");
                    }
                    Console.WriteLine("----------------------------------------------------------");
                }

                var applicationAssignedTo = await GetServicePrincipalsAppRoleAssignmentsAsync(servicePrincipal);

                if (applicationAssignedTo != null && applicationAssignedTo.Count() > 0)
                {
                    Console.WriteLine("--------------------------Apps assigned to-------------------");

                    applicationAssignedTo.ToList().ForEach(x => Console.WriteLine($"AppRole-'{approles[x.AppRoleId].DisplayName}', Principal-{x.PrincipalDisplayName}," +
                        $", PrincipalType- '{x.PrincipalType}'"));

                    Console.WriteLine("----------------------------------------------------------");
                }

                if (servicePrincipal.ReplyUrls != null && servicePrincipal.ReplyUrls.Count() > 0)
                {
                    Console.WriteLine("--------------------------ReplyUrls-------------------");

                    servicePrincipal.ReplyUrls.ToList().ForEach(x => Console.WriteLine($"{x}"));

                    Console.WriteLine("----------------------------------------------------------");
                }

                if (servicePrincipal.ServicePrincipalNames != null && servicePrincipal.ReplyUrls.Count() > 0)
                {
                    Console.WriteLine("--------------------------ServicePrincipalNames-------------------");

                    servicePrincipal.ServicePrincipalNames.ToList().ForEach(x => Console.WriteLine($"{x}"));

                    Console.WriteLine("----------------------------------------------------------");
                }

                if (servicePrincipal.Tags != null && servicePrincipal.Tags.Count() > 0)
                {
                    Console.WriteLine("--------------------------Tags-------------------");

                    servicePrincipal.Tags.ToList().ForEach(x => Console.WriteLine($"{x}"));

                    Console.WriteLine("----------------------------------------------------------");
                }

                if (servicePrincipal.MemberOf != null && servicePrincipal.MemberOf.Count() > 0)
                {
                    Console.WriteLine("--------------------------MemberOf Group-------------------");

                    foreach (var group in servicePrincipal.MemberOf)
                    {
                        Beta.Group adGroup = await groupOperations.GetGroupByIdAsync(group);

                        Console.WriteLine($"    DisplayName-'{adGroup.DisplayName}'");
                    }

                    Console.WriteLine("----------------------------------------------------------");
                }

                if (servicePrincipal.TransitiveMemberOf != null && servicePrincipal.TransitiveMemberOf.Count() > 0)
                {
                    Console.WriteLine("--------------------------TransitiveMemberOf Group-------------------");

                    foreach (var group in servicePrincipal.TransitiveMemberOf)
                    {
                        Beta.Group adGroup = await groupOperations.GetGroupByIdAsync(group);

                        Console.WriteLine($"    DisplayName-'{adGroup.DisplayName}'");
                    }

                    Console.WriteLine("----------------------------------------------------------");
                }

                if (servicePrincipal?.Oauth2PermissionGrants?.Count() > 0)
                {
                    Console.WriteLine("--------------------------PublishedPermissionScopes-------------------");
                    foreach (var oAuth2Permission in servicePrincipal.PublishedPermissionScopes)
                    {
                        Console.WriteLine($"Id-{oAuth2Permission?.Id}, IsEnabled- {oAuth2Permission.IsEnabled}, Origin- {oAuth2Permission.Origin}, Type- {oAuth2Permission.Type} "
                            + $"UserConsentDescription-{oAuth2Permission.UserConsentDescription}, UserConsentDisplayName-{oAuth2Permission.UserConsentDisplayName}, Value-{oAuth2Permission.Value}");
                    }
                    Console.WriteLine("----------------------------------------------------------");
                }

                if (servicePrincipal?.Oauth2PermissionGrants?.Count() > 0)
                {
                    Console.WriteLine("--------------------------Oauth2PermissionGrants-------------------");
                    foreach (var oAuth2PermissionGrants in servicePrincipal.Oauth2PermissionGrants)
                    {
                        Beta.ServicePrincipal resourceServicePrincipal = await GetServicePrincipalByAppIdAsync(oAuth2PermissionGrants.ResourceId);

                        Console.WriteLine($"Resource Name-{resourceServicePrincipal.DisplayName}, Id-{oAuth2PermissionGrants?.Id}, PrincipalId- {oAuth2PermissionGrants.PrincipalId}, " +
                            $"ResourceId- {oAuth2PermissionGrants.ResourceId}, Scope- {oAuth2PermissionGrants.Scope}, ConsentType- {oAuth2PermissionGrants.ConsentType}  "
                            + $"StartTime-{oAuth2PermissionGrants.StartTime}, ExpiryTime-{oAuth2PermissionGrants.ExpiryTime}");
                    }

                    Console.WriteLine("----------------------------------------------------------");
                }

                if (servicePrincipal?.CreatedObjects?.Count() > 0)
                {
                    Console.WriteLine("--------------------------CreatedObjects-------------------");
                    foreach (var createdObjects in servicePrincipal.CreatedObjects)
                    {
                        Console.WriteLine($"Id-{createdObjects.Id}, DeletedDateTime- {createdObjects.DeletedDateTime}");
                    }
                    Console.WriteLine("----------------------------------------------------------");
                }

                if (servicePrincipal?.OwnedObjects?.Count() > 0)
                {
                    Console.WriteLine("--------------------------OwnedObjects-------------------");
                    foreach (var ownedObject in servicePrincipal.OwnedObjects)
                    {
                        Console.WriteLine($"Id-{ownedObject.Id}, DeletedDateTime- {ownedObject.DeletedDateTime}");
                    }
                    Console.WriteLine("----------------------------------------------------------");
                }

                if (servicePrincipal?.KeyCredentials?.Count() > 0)
                {
                    Console.WriteLine("--------------------------KeyCredentials-------------------");
                    foreach (var keyCredential in servicePrincipal.KeyCredentials)
                    {
                        Console.WriteLine($"DisplayName-{keyCredential?.DisplayName}, KeyId- {keyCredential.KeyId}, StartDateTime- {keyCredential.StartDateTime}, EndDateTime- {keyCredential.EndDateTime} "
                            + $"Key-{keyCredential.Key}, Type-{keyCredential.Type}, Usage-{keyCredential.Usage}");
                    }
                    Console.WriteLine("----------------------------------------------------------");
                }

                if (servicePrincipal?.PasswordCredentials?.Count() > 0)
                {
                    Console.WriteLine("--------------------------PasswordCredentials-------------------");
                    foreach (var passwordCredential in servicePrincipal.PasswordCredentials)
                    {
                        Console.WriteLine($"DisplayName-{passwordCredential?.DisplayName}, KeyId- {passwordCredential.KeyId}, StartDateTime- {passwordCredential.StartDateTime}, EndDateTime- {passwordCredential.EndDateTime} "
                            + $"Hint-{passwordCredential.Hint}, SecretText-{passwordCredential.SecretText}, Hint-{passwordCredential?.Hint}");
                    }
                    Console.WriteLine("----------------------------------------------------------");
                }

                if (servicePrincipal?.AdditionalData.Count > 0)
                {
                    Console.WriteLine("--------------------------servicePrincipal.AdditionalData start-------------------");
                    Console.WriteLine(servicePrincipal?.AdditionalData.ToDebugString());
                    Console.WriteLine("--------------------------servicePrincipal.AdditionalData end-------------------");
                }

                await PrintServicePrincipalOAuth2PermissionGrantsAsync(servicePrincipal);
            }
            Console.WriteLine($"--------------------------------ServicePrincipal '{application.DisplayName}' end----------------------------------------");
        }

        private async Task PrintServicePrincipalOAuth2PermissionGrantsAsync(Beta.ServicePrincipal servicePrincipal)
        {
            if (servicePrincipal != null)
            {
                UserOperations userOperations = new UserOperations(_graphServiceClient);

                Console.WriteLine("");
                Console.WriteLine($"--------------------------------OAuth2PermissionGrants for '{servicePrincipal.DisplayName}' start----------------------------------------");

                try
                {
                    var OAuth2PermissionGrants = await _graphServiceClient.Oauth2PermissionGrants.Request().Filter($"clientId eq '{servicePrincipal.Id}'").GetAsync();

                    if (OAuth2PermissionGrants != null)
                    {
                        do
                        {
                            // Page through results
                            foreach (var OAuth2PermissionGrant in OAuth2PermissionGrants.CurrentPage)
                            {
                                Console.WriteLine("-------------------------------");

                                Console.WriteLine($"ClientId:{OAuth2PermissionGrant.ClientId}, ConsentType:{OAuth2PermissionGrant.ConsentType}, Scope:{OAuth2PermissionGrant.Scope}" +
                                    $"PrincipalId:{OAuth2PermissionGrant.PrincipalId}, ResourceId:{OAuth2PermissionGrant.ResourceId}, " +
                                    $"StartTime:{OAuth2PermissionGrant.StartTime}, ExpiryTime:{OAuth2PermissionGrant.ExpiryTime}");

                                Beta.ServicePrincipal resourceServicePrincipal = await GetServicePrincipalByIdAsync(OAuth2PermissionGrant.ResourceId);

                                if (resourceServicePrincipal != null)
                                {
                                    Console.WriteLine($"Resource Name-{resourceServicePrincipal.DisplayName}, Scope:{OAuth2PermissionGrant.Scope}");

                                    if (OAuth2PermissionGrant.ConsentType == "AllPrincipals")
                                    {
                                        Console.WriteLine($"has been granted by Admin consent");
                                    }
                                    else
                                    {
                                        Beta.User grantPrincipal = await userOperations.GetUserByIdAsync(OAuth2PermissionGrant.PrincipalId);

                                        if (grantPrincipal != null)
                                        {
                                            Console.WriteLine($"Granted to -{grantPrincipal.DisplayName}");
                                        }
                                        else
                                        {
                                            Console.WriteLine($"PrincipalId:{grantPrincipal.Id} is an orphan user in this tenant");
                                        }
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"ResourceId:{OAuth2PermissionGrant.ResourceId} is orphan resource in this tenant");
                                }

                                Console.WriteLine("-------------------------------");

                                if (servicePrincipal?.AdditionalData.Count > 0)
                                {
                                    Console.WriteLine(servicePrincipal?.AdditionalData.ToDebugString());
                                }
                            }

                            // are there more pages (Has a @odata.nextLink ?)
                            if (OAuth2PermissionGrants.NextPageRequest != null)
                            {
                                OAuth2PermissionGrants = await OAuth2PermissionGrants.NextPageRequest.GetAsync();
                            }
                            else
                            {
                                OAuth2PermissionGrants = null;
                            }
                        } while (OAuth2PermissionGrants != null);
                    }
                }
                catch (ServiceException e)
                {
                    Console.WriteLine($"We could not retrieve the user's list: {e}");
                }
            }
            else
            {
                Console.WriteLine("The provided ServicePrincipal is null!");
            }

            Console.WriteLine($"--------------------------------OAuth2PermissionGrants for '{servicePrincipal.DisplayName}' start----------------------------------------");
        }

        /// <summary>
        /// Users and groups assigned in AppRoles in this service Principal
        /// </summary>
        /// <param name="graphServiceClient">The graph service client.</param>
        /// <param name="servicePrincipal">The service principal.</param>
        /// <returns></returns>
        public async Task<List<Beta.AppRoleAssignment>> GetServicePrincipalsAppRoleAssignmentsAsync(Beta.ServicePrincipal servicePrincipal)
        {
            List<Beta.AppRoleAssignment> allAssignments = new List<Beta.AppRoleAssignment>();

            try
            {
                var approleAssignmentPages = await _graphServiceClient.ServicePrincipals[servicePrincipal.Id].AppRoleAssignments.Request().GetAsync();

                if (approleAssignmentPages != null)
                {
                    do
                    {
                        // Page through results
                        foreach (var user in approleAssignmentPages.CurrentPage)
                        {
                            allAssignments.Add(user);
                        }

                        // are there more pages (Has a @odata.nextLink ?)
                        if (approleAssignmentPages.NextPageRequest != null)
                        {
                            approleAssignmentPages = await approleAssignmentPages.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            approleAssignmentPages = null;
                        }
                    } while (approleAssignmentPages != null);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not retrieve the role assigned to: {e}");
                return null;
            }

            return allAssignments;
        }

        /// <summary>
        /// Applications that the service principal is assigned to. Read-only.
        /// </summary>
        /// <param name="graphServiceClient">The graph service client.</param>
        /// <param name="servicePrincipal">The service principal.</param>
        /// <returns></returns>
        private async Task<List<Beta.AppRoleAssignment>> GetServicePrincipalsAppRoleAssignedToAsync(Beta.ServicePrincipal servicePrincipal)
        {
            List<Beta.AppRoleAssignment> allAssignments = new List<Beta.AppRoleAssignment>();

            try
            {
                var approleAssignedToPages = await _graphServiceClient.ServicePrincipals[servicePrincipal.Id].AppRoleAssignedTo.Request().GetAsync();

                if (approleAssignedToPages != null)
                {
                    do
                    {
                        // Page through results
                        foreach (var user in approleAssignedToPages.CurrentPage)
                        {
                            allAssignments.Add(user);
                        }

                        // are there more pages (Has a @odata.nextLink ?)
                        if (approleAssignedToPages.NextPageRequest != null)
                        {
                            approleAssignedToPages = await approleAssignedToPages.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            approleAssignedToPages = null;
                        }
                    } while (approleAssignedToPages != null);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not retrieve the role assigned to: {e}");
                return null;
            }

            return allAssignments;
        }

        /// <summary>
        /// Applications that the service principal OAuth2PermissionGrants
        /// </summary>
        /// <param name="graphServiceClient">The graph service client.</param>
        /// <param name="servicePrincipal">The service principal.</param>
        /// <returns></returns>
        private async Task<List<Beta.OAuth2PermissionGrant>> GetServicePrincipalsOauth2PermissionGrantsAsync(Beta.ServicePrincipal servicePrincipal)
        {
            List<Beta.OAuth2PermissionGrant> alloauth2PermissionGrants = new List<Beta.OAuth2PermissionGrant>();

            try
            {
                var OAuth2PermissionGrantsPages = await _graphServiceClient.ServicePrincipals[servicePrincipal.Id].Oauth2PermissionGrants.Request().GetAsync();

                if (OAuth2PermissionGrantsPages != null)
                {
                    do
                    {
                        // Page through results
                        foreach (var grant in OAuth2PermissionGrantsPages.CurrentPage)
                        {
                            alloauth2PermissionGrants.Add(grant);
                        }

                        // are there more pages (Has a @odata.nextLink ?)
                        if (OAuth2PermissionGrantsPages.NextPageRequest != null)
                        {
                            OAuth2PermissionGrantsPages = await OAuth2PermissionGrantsPages.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            OAuth2PermissionGrantsPages = null;
                        }
                    } while (OAuth2PermissionGrantsPages != null);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not retrieve the permissions grants: {e}");
                return null;
            }

            return alloauth2PermissionGrants;
        }

        //private  async Task DeleteServicePrincipalAsync(Beta.Application application)
        //{
        //    try
        //    {
        //        await _graphServiceClient.Users[userId].Request().DeleteAsync();
        //    }
        //    catch (ServiceException e)
        //    {
        //        Console.WriteLine($"We could not delete the user with Id-{userId}: {e}");
        //    }

        //}

        //private  async Task DeleteServicePrincipalAsync(Beta.Application application)
        //{
        //    try
        //    {
        //        await _graphServiceClient.Users[userId].Request().DeleteAsync();
        //    }
        //    catch (ServiceException e)
        //    {
        //        Console.WriteLine($"We could not delete the user with Id-{userId}: {e}");
        //    }

        //}

        public async Task AssignUsersToAppRoles(Beta.Application application, IList<Beta.User> users)
        {
            Beta.ServicePrincipal servicePrincipal = await GetServicePrincipalByAppIdAsync(application.AppId);

            try
            {
                List<Beta.AppRole> userassignableroles = servicePrincipal.AppRoles.ToList().Where(x => x.AllowedMemberTypes.ToList().Contains("User")).ToList();

                userassignableroles.ForEach(async (approle) =>
                {
                    ColorConsole.WriteLine($"Role name {approle.DisplayName}");

                    IList<Beta.User> usersToAssign = GenericUtility<Beta.User>.GetaRandomNumberOfItemsFromList(users, 6).ToList();

                    for (int i = 0; i < usersToAssign.Count(); i++)
                    {
                        var approleAssignment = new Beta.AppRoleAssignment();
                        approleAssignment.PrincipalId = new Guid(usersToAssign[i].Id);
                        approleAssignment.ResourceId = new Guid(servicePrincipal.Id);
                        approleAssignment.AppRoleId = approle.Id;

                        // await graphServiceClient.AppRoleAssignments.Request().AddAsync(approleAssignment);
                        var assignment = await _graphServiceClient.ServicePrincipals[servicePrincipal.Id].AppRoleAssignedTo.Request().AddAsync(approleAssignment);

                        Console.WriteLine($"{assignment.PrincipalDisplayName} assigned to AppRole '{approle.DisplayName}' with id '{assignment.Id}' ");
                    }
                });
            }
            catch (Exception ex)
            {
                ColorConsole.WriteLine(ConsoleColor.Red, $"{ex}");
                throw;
            }

            ColorConsole.WriteLine(ConsoleColor.Green, "All app role assignments complete");
        }

        public async Task UpdateServicePrincipalSettings(Beta.Application application,            IEnumerable<Beta.User> allUsersInTenant)
        {
            Beta.ServicePrincipal servicePrincipal = await GetServicePrincipalByAppIdAsync(application.AppId);

            if (servicePrincipal == null)
            {
                ColorConsole.WriteLine(ConsoleColor.Red, $"No service principal for app '{application.DisplayName}' found! ");
            }

            servicePrincipal.AppRoleAssignmentRequired = true;

            IList<string> replyUrlsToAdd = new List<string>()
            {
                "https://www.kkaad.onmicrosoft.com/myotherapp/landingpage",
                "https://www.kkaad.onmicrosoft.com/myotherapp/landingpage2"
            };

            //servicePrincipal.ReplyUrls.ToList().Add("https://www.kkaad.onmicrosoft.com/myotherapp/landingpage");
            //servicePrincipal.ReplyUrls = GenericUtility<string>.BackupAddAndReplace(servicePrincipal.ReplyUrls, replyUrlsToAdd);

            IEnumerable<Beta.User> owners = GenericUtility<Beta.User>.GetaRandomNumberOfItemsFromList(allUsersInTenant, 3);
            //servicePrincipal.Owners = Beta.GraphServiceUsersCollectionPage();

            //Beta.GraphServiceUsersCollectionPage userpage = new Beta.GraphServiceUsersCollectionPage();
            //ownersToList().ForEach(x => servicePrincipal.Owners.Add(x));

            // Remove a couple of role assignments
            Dictionary<Guid?, Beta.AppRole> approles = new Dictionary<Guid?, Beta.AppRole>();
            if (servicePrincipal?.AppRoles?.Count() > 0)
            {
                Console.WriteLine("--------------------------AppRoles-------------------");
                foreach (var appRole in servicePrincipal.AppRoles)
                {
                    approles.Add(appRole.Id, appRole);
                    Console.WriteLine($"Id-{appRole.Id}, IsEnabled- {appRole.IsEnabled}, UserConsentDisplayName-{appRole.Value}, " +
                        $"AllowedMemberTypes- {String.Join(",", appRole.AllowedMemberTypes)}");
                }
                Console.WriteLine("----------------------------------------------------------");
            }

            var approleassignments = await GetServicePrincipalsAppRoleAssignedToAsync(servicePrincipal);

            var assignmentsToDelete = GenericUtility<Beta.AppRoleAssignment>.GetaRandomNumberOfItemsFromList(approleassignments, 4).ToList();

            assignmentsToDelete.ForEach(async (assignment) =>
            {
                await _graphServiceClient.ServicePrincipals[servicePrincipal.Id].AppRoleAssignedTo[assignment.Id].Request().DeleteAsync();
                Console.WriteLine($"'{approles[assignment.AppRoleId].DisplayName}' assigned to {assignment.PrincipalDisplayName} with id '{assignment.Id}' deleted");
            });

            await _graphServiceClient.ServicePrincipals[servicePrincipal.Id].Request().UpdateAsync(servicePrincipal);
        }

        public async Task RemoveUsersFromAppRoles(Beta.Application application,
            IList<Beta.User> users)
        {
            Beta.ServicePrincipal servicePrincipal = await GetServicePrincipalByAppIdAsync(application.AppId);

            try
            {
                List<Beta.AppRole> userassignableroles = servicePrincipal.AppRoles.ToList().Where(x => x.AllowedMemberTypes.ToList().Contains("User")).ToList();

                userassignableroles.ForEach(async (approle) =>
                {
                    ColorConsole.WriteLine($"Role name {approle.DisplayName}");

                    int end = users.Count() / 2;

                    for (int i = 0; i < end; i++)
                    {
                        var approleAssignment = new Beta.AppRoleAssignment();
                        approleAssignment.PrincipalId = new Guid(users[i].Id);
                        approleAssignment.ResourceId = new Guid(servicePrincipal.Id);
                        approleAssignment.AppRoleId = approle.Id;

                        // await graphServiceClient.AppRoleAssignments.Request().AddAsync(approleAssignment);
                        var assignment = await _graphServiceClient.ServicePrincipals[servicePrincipal.Id].AppRoleAssignedTo.Request().AddAsync(approleAssignment);
                        Console.WriteLine($"{assignment.PrincipalDisplayName} assigned to '{approle.DisplayName}' with id '{assignment.Id}' ");
                    }
                });
            }
            catch (Exception ex)
            {
                ColorConsole.WriteLine(ConsoleColor.Red, $"{ex}");
                throw;
            }

            ColorConsole.WriteLine(ConsoleColor.Green, "All app role assignments complete");
        }

        public async Task<Beta.Application> CreateApplicationAsync(Beta.GraphServiceClient graphServiceClient)
        {
            Beta.Application application = new Beta.Application() { };

            application.DisplayName = "My app roles demo";

            application.Web = new Beta.WebApplication();
            application.Web.HomePageUrl = "https://localhost:44321/";
            application.Web.LogoutUrl = "https://localhost:44321/signout-oidc";
            application.Web.ImplicitGrantSettings = new Beta.ImplicitGrantSettings()
            { EnableIdTokenIssuance = true };

            IList<String> redirectUris = new List<string>() { "https://localhost:44321/", "https://localhost:44321/signin-oidc" };
            application.Web.RedirectUris = redirectUris;

            application.SignInAudience = "AzureADMyOrg";
            //application.IsFallbackPublicClient = true;

            IList<String> identifierUris = new List<string>() { $"https://kkaad.onmicrosoft.com/{application.DisplayName.Replace(" ", "")}" };
            application.IdentifierUris = identifierUris;

            application.Api = new Beta.ApiApplication();
            application.Api.RequestedAccessTokenVersion = 2;

            IList<Beta.PermissionScope> oauth2PermissionScopes = new List<Beta.PermissionScope>();
            oauth2PermissionScopes.Add(new Beta.PermissionScope()
            {
                Id = Guid.NewGuid(),
                IsEnabled = true,
                Type = "User",
                Value = "access_as_user",
                AdminConsentDisplayName = $"Access {application.DisplayName}",
                AdminConsentDescription = $"Allows the app to have the same access to information in the directory on behalf of the signed-in user.",
                UserConsentDisplayName = $"Access {application.DisplayName}",
                UserConsentDescription = $"Allow the application to access {application.DisplayName} on your behalf."
            });

            oauth2PermissionScopes.Add(new Beta.PermissionScope()
            {
                Id = Guid.NewGuid(),
                IsEnabled = true,
                Type = "Admin",
                Value = "user_impersonation",
                AdminConsentDisplayName = $"Access {application.DisplayName} as the signed-in user",
                AdminConsentDescription = $"Allows the app to have the same access to information in the directory on behalf of the signed-in user.",
                UserConsentDisplayName = $"Access {application.DisplayName} as the signed-in user",
                UserConsentDescription = $"Allow the application to access {application.DisplayName} on your behalf."
            });

            application.Api.Oauth2PermissionScopes = oauth2PermissionScopes;

            // Pre-authorized Apps
            IList<Beta.PreAuthorizedApplication> preAuthorizedApplications = new List<Beta.PreAuthorizedApplication>();
            IList<String> permissionIds = new List<String>();
            oauth2PermissionScopes.ToList().ForEach(x => permissionIds.Add(x.Id.ToString()));

            List<Beta.Application> applications = await GetAllApplicationsAsync();
            preAuthorizedApplications.Add(new Beta.PreAuthorizedApplication()
            {
                AppId = applications.ElementAtOrDefault(new System.Random().Next() % applications.Count()).AppId,
                PermissionIds = permissionIds
            });

            application.Api.PreAuthorizedApplications = preAuthorizedApplications;
            application.Tags = new List<string>() { "HooHoo", "HaaHaa" };

            // App owners
            IList<Beta.IApplicationOwnersCollectionWithReferencesPage> owners = new List<Beta.IApplicationOwnersCollectionWithReferencesPage>();

            // Required resource Access
            IList<Beta.RequiredResourceAccess> requiredResourceAccesses = new List<Beta.RequiredResourceAccess>();

            // App permissions
            requiredResourceAccesses.Add(await GetApplicationRolesByValueAsync("Microsoft Graph", new List<string>() { "Directory.AccessAsUser.All", "Directory.ReadWrite.All", "Contacts.ReadWrite", "AppRoleAssignment.ReadWrite.All" }));
            requiredResourceAccesses.Add(await GetApplicationRolesByValueAsync("Microsoft Intune API", new List<string>() { "get_data_warehouse", "send_data_usage", "update_device_health" }));
            requiredResourceAccesses.Add(await GetApplicationRolesByValueAsync("Skype for Business Online", new List<string>() { "Meetings.JoinManage", "Meetings.ScheduleOnDemand" }));

            // TODO: Scopes
            //requiredResourceAccesses.Add(await GetApplicationScopesByValueAsync( "Microsoft Graph", new List<string>() { "User.Read", "User.ReadWrite.All" }));
            //requiredResourceAccesses.Add(await GetApplicationScopesByValueAsync( "Azure Service Management", new List<string>() { "user_impersonation" }));

            application.RequiredResourceAccess = requiredResourceAccesses;

            // Create app roles to assign users to
            Beta.AppRole viewersrole = new Beta.AppRole() { AllowedMemberTypes = new List<String>() { "User" }, DisplayName = "Viewers", Value = "Viewers", Description = "Users in this role have the permission to read data", Id = Guid.NewGuid(), IsEnabled = true };
            Beta.AppRole adminRole = new Beta.AppRole() { AllowedMemberTypes = new List<String>() { "User" }, Value = "Admins", DisplayName = "Admins", Description = "Users in the admin role have the permission to both read and write data", Id = Guid.NewGuid(), IsEnabled = true };

            // Create application permission
            Beta.AppRole accessAsApplication = new Beta.AppRole() { AllowedMemberTypes = new List<String>() { "Application" }, Value = "access_As_Application", DisplayName = $"Access {application.DisplayName} as an application", Description = "Access {application.DisplayName} as an application", Id = Guid.NewGuid(), IsEnabled = true };

            IList<Beta.AppRole> approles = new List<Beta.AppRole>() { viewersrole, adminRole, accessAsApplication };
            application.AppRoles = approles;

            // Not allowed
            // IList<Beta.PasswordCredential> passwordCredentials = new List<Beta.PasswordCredential>() { CreateAppKey(DateTime.Now, 99, ComputePassword()) };
            // application.PasswordCredentials = passwordCredentials;

            Beta.Application createdApp = await graphServiceClient.Applications.Request().AddAsync(application);

            if (createdApp != null)
            {
                // Not supported
                // Beta.PasswordCredential credential = await graphServiceClient.Applications[createdApp.Id].AddPassword(CreateAppKey(DateTime.Now, 99, ComputePassword())).Request().PostAsync();
                Beta.PasswordCredential credential = await graphServiceClient.Applications[createdApp.Id].AddPassword().Request().PostAsync();

                if (credential != null)
                {
                    Console.WriteLine($"New Credential: DisplayName -{credential.DisplayName}, CustomKeyIdentifier-{credential.CustomKeyIdentifier}, " +
                        $"StartDateTime- {credential.StartDateTime}, EndDateTime-{credential.EndDateTime}, SecretText-{credential.SecretText}");

                    // Refresh the newly created app's instance
                    createdApp = await graphServiceClient.Applications[createdApp.Id].Request().GetAsync();
                }
            }

            // Create a service principal
            Beta.ServicePrincipal servicePrincipal = new Beta.ServicePrincipal()
            {
                AppId = createdApp.AppId,
                Tags = new List<string>() { "WindowsAzureActiveDirectoryIntegratedApp", "PooPoo" }
            };

            await graphServiceClient.ServicePrincipals.Request().AddAsync(servicePrincipal);

            return createdApp;
        }

        public async Task DeleteApplicationAsync(Beta.Application application, Beta.GraphServiceClient graphServiceClient)
        {
            try
            {
                await graphServiceClient.Applications[application.Id].Request().DeleteAsync();
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not delete the application with Id-{application.Id}: {e}");
            }
        }

        public async Task<Beta.RequiredResourceAccess> GetApplicationScopesByValueAsync(string apiDisplayName, IList<string> scopeValues)
        {
            Beta.RequiredResourceAccess requiredResourceAccess = null;

            // ResourceAppId of Microsoft Graph
            Beta.ServicePrincipal servicePrincipal = await GetServicePrincipalByAppDisplayNameAsync(apiDisplayName);

            if (servicePrincipal != null)
            {
                requiredResourceAccess = new Beta.RequiredResourceAccess() { ResourceAppId = servicePrincipal.AppId };
                IList<Beta.ResourceAccess> resourceAccesses = new List<Beta.ResourceAccess>();

                scopeValues.ToList().ForEach(scopeValue =>
                {
                    //Beta.AppRole appRole = servicePrincipal.oAuth2PermissionScopes.Where(x => x.Value == roleValue).FirstOrDefault();

                    //if (appRole != null)
                    //{
                    //    resourceAccesses.Add(new Beta.ResourceAccess() { Type = "Role", Id = appRole.Id });
                    //}

                    //resourceAccesses.Add(new Beta.ResourceAccess() { Type = "Scope", Id = appRole.Id });
                });

                requiredResourceAccess.ResourceAccess = resourceAccesses;
            }
            else
            {
                ColorConsole.WriteLine(ConsoleColor.Red, $"No service principal matching '{apiDisplayName}' found in the tenant");
            }

            return requiredResourceAccess;
        }

        private async Task<Beta.Application> GetApplicationByAppIdAsync(string appId)
        {
            var applications = await _graphServiceClient.Applications.Request().Filter($"appId eq '{appId}'").GetAsync();
            //Request.Header("Prefer","outlook.body-content-type=\"text\"")
            return applications.FirstOrDefault();
        }

        private async Task<Beta.ServicePrincipal> GetServicePrincipalByAppIdAsync(string appId)
        {
            var servicePrincipals = await _graphServiceClient.ServicePrincipals.Request().Filter($"appId eq '{appId}'").GetAsync();
            return servicePrincipals.FirstOrDefault();
        }

        private async Task<Beta.ServicePrincipal> GetServicePrincipalByAppDisplayNameAsync(string appDisplayName)
        {
            var servicePrincipals = await _graphServiceClient.ServicePrincipals.Request().Filter($"displayName eq '{appDisplayName}'").GetAsync();
            return servicePrincipals.FirstOrDefault();
        }

        private async Task<Beta.ServicePrincipal> GetServicePrincipalByIdAsync(string Id)
        {
            var servicePrincipals = await _graphServiceClient.ServicePrincipals.Request().Filter($"id eq '{Id}'").GetAsync();
            return servicePrincipals.FirstOrDefault();
        }
    }
}