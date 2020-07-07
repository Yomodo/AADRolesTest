extern alias BetaLib;

using Common;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Beta = BetaLib.Microsoft.Graph;

namespace AuthNMethodsTesting
{
    public class RiskyUserOperations
    {
        private Beta.GraphServiceClient _graphServiceClient;
        private UserOperations _userOperations;

        public RiskyUserOperations(Beta.GraphServiceClient graphServiceClient, UserOperations userOperations)
        {
            this._graphServiceClient = graphServiceClient;
            this._userOperations = userOperations;
        }

        public async Task<List<Beta.RiskyUser>> ListRiskyUsersAsync()
        {
            List<Beta.RiskyUser> allLocations = new List<Beta.RiskyUser>();
            Beta.IGraphServiceRiskyUsersCollectionPage riskyusers = null;

            try
            {
                riskyusers = await _graphServiceClient.RiskyUsers.Request().GetAsync();

                if (riskyusers != null)
                {
                    allLocations = await ProcessIGraphServiceRiskyUsersCollectionPage(riskyusers);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not retrieve the risky users: {e}");
                return null;
            }

            return allLocations;
        }

        public async Task<string> PrintRiskyUsersAsync(Beta.RiskyUser riskyUser, bool verbose = false, bool printHistory = false)
        {
            string toPrint = string.Empty;
            StringBuilder more = new StringBuilder();

            if (riskyUser != null)
            {
                toPrint = $"UPN-{riskyUser.UserPrincipalName}, RiskLevel-{riskyUser?.RiskLevel.Value}, RiskState-{riskyUser?.RiskState.Value}, RiskDetail-{riskyUser?.RiskDetail.Value}";

                if (verbose)
                {
                    toPrint = toPrint + $", Id-{riskyUser.Id}, DisplayName-{riskyUser.UserDisplayName}, IsProcessing-{riskyUser.IsProcessing}, IsDeleted-{riskyUser.IsDeleted}";
                    more.AppendLine("");
                }

                if (printHistory)
                {
                    var riskHistoryEvents = await ProcessIRiskyUserHistoryCollectionPage(await GetRiskyUsersHistoryByIdAsync(riskyUser.Id));

                    if (riskHistoryEvents.Count > 0)
                    {
                        more.AppendLine($"Total History events - {riskHistoryEvents.Count}");

                        riskHistoryEvents.ForEach(evt =>
                        {
                            more.AppendLine($"InitiatedBy-{evt.InitiatedBy}, RiskLevel-{evt?.RiskLevel.Value}, RiskState-{evt?.RiskState.Value}, RiskDetail-{evt?.RiskDetail.Value}, IsProcessing-{evt.IsProcessing}, InitiatedBy-{evt.InitiatedBy}, Last updated-{evt.RiskLastUpdatedDateTime}");
                            more.AppendLine($"Activity details-> Detail-{evt.Activity?.Detail.Value}, EventTypes-{evt.Activity.EventTypes.ToCommaSeparatedString()}");
                        });
                    }
                }
            }
            else
            {
                toPrint = "The provided risky user is null!";
            }

            return toPrint + more.ToString();
        }

        public async Task ConfirmCompromisedAsync(string userId)
        {
            await ConfirmCompromisedAsync(new List<string> { userId });
        }

        public async Task ConfirmCompromisedAsync(IList<string> userIds)
        {
            await _graphServiceClient.RiskyUsers.ConfirmCompromised(userIds).Request().PostAsync();
        }

        public async Task DismissAsync(string userId)
        {
            await DismissAsync(new List<string> { userId });
        }

        public async Task DismissAsync(IList<string> userIds)
        {
            await _graphServiceClient.RiskyUsers.Dismiss(userIds).Request().PostAsync();
        }

        public async Task<Beta.IRiskyUserHistoryCollectionPage> GetRiskyUsersHistoryByIdAsync(string riskyUserId)
        {
            try
            {
                return await _graphServiceClient.RiskyUsers[riskyUserId].History.Request().GetAsync();
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

        public async Task<Beta.RiskyUser> GetRiskyUsersByIdUnsafeAsync(string riskyUserRecordId)
        {
            // Note record id != userId
            return await _graphServiceClient.RiskyUsers[riskyUserRecordId].Request().GetAsync();
        }

        public async Task<List<Beta.RiskyUser>> GetRiskyUsersByUPNUnsafeAsync(string userPrincipalName)
        {
            return await ProcessIGraphServiceRiskyUsersCollectionPage( await _graphServiceClient.RiskyUsers.Request().Filter($"userPrincipalName eq '{userPrincipalName}'").GetAsync());
        }

        public async Task<Beta.RiskyUser> GetRiskyUsersByIdAsync(string riskyUserId)
        {
            try
            {
                return await _graphServiceClient.RiskyUsers[riskyUserId].Request().GetAsync();
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

        private async Task<List<Beta.RiskyUserHistoryItem>> ProcessIRiskyUserHistoryCollectionPage(Beta.IRiskyUserHistoryCollectionPage riskyUsersHistory)
        {
            List<Beta.RiskyUserHistoryItem> allriskyUsersHistory = new List<Beta.RiskyUserHistoryItem>();

            try
            {
                if (riskyUsersHistory != null)
                {
                    do
                    {
                        // Page through results
                        foreach (var riskyUser in riskyUsersHistory.CurrentPage)
                        {
                            //Console.WriteLine($"Role:{riskyUsersHistory.DisplayName}");
                            allriskyUsersHistory.Add(riskyUser);
                        }

                        // are there more pages (Has a @odata.nextLink ?)
                        if (riskyUsersHistory.NextPageRequest != null)
                        {
                            riskyUsersHistory = await riskyUsersHistory.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            riskyUsersHistory = null;
                        }
                    } while (riskyUsersHistory != null);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not process the risky users history: {e}");
                return null;
            }

            return allriskyUsersHistory;
        }

        private async Task<List<Beta.RiskyUser>> ProcessIGraphServiceRiskyUsersCollectionPage(Beta.IGraphServiceRiskyUsersCollectionPage riskyUsers)
        {
            List<Beta.RiskyUser> allriskyUsers = new List<Beta.RiskyUser>();

            try
            {
                if (riskyUsers != null)
                {
                    do
                    {
                        // Page through results
                        foreach (var riskyUser in riskyUsers.CurrentPage)
                        {
                            //Console.WriteLine($"Role:{riskyUsers.DisplayName}");
                            allriskyUsers.Add(riskyUser);
                        }

                        // are there more pages (Has a @odata.nextLink ?)
                        if (riskyUsers.NextPageRequest != null)
                        {
                            riskyUsers = await riskyUsers.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            riskyUsers = null;
                        }
                    } while (riskyUsers != null);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not process the risky users list: {e}");
                return null;
            }

            return allriskyUsers;
        }
    }
}