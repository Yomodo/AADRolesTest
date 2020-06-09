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
    public class AADReportsOperations
    {
        private Beta.GraphServiceClient _graphServiceClient;
        private UserOperations _userOperations;

        public AADReportsOperations(Beta.GraphServiceClient graphServiceClient, UserOperations userOperations)
        {
            this._graphServiceClient = graphServiceClient;
            this._userOperations = userOperations;
        }

        #region credentialUserRegistrationDetails

        public async Task<List<Beta.CredentialUserRegistrationDetails>> ListCredentialUserRegistrationRegisteredForSSPRAsync()
        {
            return await ListCredentialUserRegistrationDetailsAsync("isRegistered eq true");
        }

        public async Task<List<Beta.CredentialUserRegistrationDetails>> ListCredentialUserRegistrationEnabledForSSPRAsync()
        {
            return await ListCredentialUserRegistrationDetailsAsync("isEnabled eq true");
        }

        public async Task<List<Beta.CredentialUserRegistrationDetails>> ListCredentialUserRegistrationCapableOfMFAsync()
        {
            return await ListCredentialUserRegistrationDetailsAsync("isCapable eq true");
        }

        public async Task<List<Beta.CredentialUserRegistrationDetails>> ListCredentialUserRegistrationRegisteredForMFAAsync()
        {
            return await ListCredentialUserRegistrationDetailsAsync("isMfaRegistered eq true");
        }

        public async Task<Beta.CredentialUserRegistrationDetails> ListCredentialUserRegistrationDetailsByUserPrincipalNameAsync(string userPrincipalName)
        {
            var data = await ListCredentialUserRegistrationDetailsAsync($"userPrincipalName eq '{userPrincipalName}'");
            return data.FirstOrDefault();
        }

        public async Task<Beta.CredentialUserRegistrationDetails> ListCredentialUserRegistrationDetailsByUserDisplayNameAsync(string userPrincipalName)
        {
            var data = await ListCredentialUserRegistrationDetailsAsync($"userDisplayName eq '{userPrincipalName}'");
            return data.FirstOrDefault();
        }

        public async Task<List<Beta.CredentialUserRegistrationDetails>> ListCredentialUserRegistrationDetailsAsync(string filter)
        {
            var credentialUserRegistrationDetails = await _graphServiceClient.Reports.CredentialUserRegistrationDetails.Request().Filter(filter).GetAsync();
            return await ProcessIReportRootCredentialUserRegistrationDetailsCollectionPage(credentialUserRegistrationDetails);
        }

        private async Task<List<Beta.CredentialUserRegistrationDetails>> ProcessIReportRootCredentialUserRegistrationDetailsCollectionPage(Beta.IReportRootCredentialUserRegistrationDetailsCollectionPage credentialUserRegistrationDetailsCollectionPage)
        {
            List<Beta.CredentialUserRegistrationDetails> allcredentialUserRegistrationDetails = new List<Beta.CredentialUserRegistrationDetails>();

            try
            {
                if (credentialUserRegistrationDetailsCollectionPage != null)
                {
                    do
                    {
                        // Page through results
                        foreach (var credentialUserRegistrationDetail in credentialUserRegistrationDetailsCollectionPage.CurrentPage)
                        {
                            allcredentialUserRegistrationDetails.Add(credentialUserRegistrationDetail);
                        }

                        // are there more pages (Has a @odata.nextLink ?)
                        if (credentialUserRegistrationDetailsCollectionPage.NextPageRequest != null)
                        {
                            credentialUserRegistrationDetailsCollectionPage = await credentialUserRegistrationDetailsCollectionPage.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            credentialUserRegistrationDetailsCollectionPage = null;
                        }
                    } while (credentialUserRegistrationDetailsCollectionPage != null);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not process the credential User Registration Details list: {e}");
                return null;
            }

            return allcredentialUserRegistrationDetails;
        }

        public string PrintCredentialUserRegistrationDetails(Beta.CredentialUserRegistrationDetails details)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"UserPrincipalName-{details.UserPrincipalName}, AuthMethods-{String.Join(",", details.AuthMethods.ToList())}, " +
                $"isCapable-{details.IsCapable}, IsEnabled-{details.IsEnabled}, IsMfaRegistered-{details.IsMfaRegistered}" +
                $", IsRegistered-{details.IsRegistered}, Id-{details.Id}");

            return sb.ToString();
        }

        #endregion credentialUserRegistrationDetails
    }
}