extern alias BetaLib;

using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beta = BetaLib.Microsoft.Graph;

namespace AADGraphTesting
{
    public class UserOperations
    {
        private Beta.GraphServiceClient _graphServiceClient;

        public UserOperations(Beta.GraphServiceClient graphServiceClient)
        {
            this._graphServiceClient = graphServiceClient;
        }

        public async Task<Beta.User> GetMeAsync()
        {
            // Call /me Api
            return await _graphServiceClient.Me.Request().GetAsync();
        }

        public async Task<Beta.User> GetUserByIdAsync(string principalId)
        {
            var users = await _graphServiceClient.Users.Request().Filter($"id eq '{principalId}'").GetAsync();
            return users.CurrentPage.FirstOrDefault();
        }

        public async Task<Beta.User> GetUserByIdAsync(Beta.DirectoryObject owner)
        {
            return await GetUserByIdAsync( owner.Id);
        }


        private async Task<List<Beta.User>> GetMyDirectReportsAsync()
        {
            List<Beta.User> allReportees = new List<Beta.User>();

            var directreports = await _graphServiceClient.Me.DirectReports.Request().GetAsync();

            if (directreports != null)
            {
                try
                {
                    do
                    {
                        // Page through results
                        foreach (var reportee in directreports.CurrentPage)
                        {
                            if (reportee.ODataType == "#microsoft.graph.user")
                            {
                                //Console.WriteLine($"User:{user.DisplayName}");
                                allReportees.Add(reportee as Beta.User);
                            }
                        }

                        // are there more pages (Has a @odata.nextLink ?)
                        if (directreports.NextPageRequest != null)
                        {
                            directreports = await directreports.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            directreports = null;
                        }
                    } while (directreports != null);
                }
                catch (ServiceException e)
                {
                    Console.WriteLine($"We could not process the reportee's list: {e}");
                    return null;
                }
            }
            return allReportees;
        }

        public async Task<List<Beta.User>> GetUsersAsync(int top = 15, bool useSelect = true)
        {
            List<Beta.User> allUsers = new List<Beta.User>();
            Beta.IGraphServiceUsersCollectionPage users = null;

            try
            {
                if (!useSelect)
                {
                    users = await _graphServiceClient.Users.Request().Top(top).GetAsync();
                }
                else
                {
                    users = await _graphServiceClient.Users.Request()
                        .Select("id,displayName,givenName,surname,mail,mailNickname,userPrincipalName,imAddresses,userType,jobTitle,accountEnabled,country,usageLocation,otherMails,proxyAddresses,identities,passwordPolicies")
                        .Top(top).GetAsync();
                }

                if (users != null)
                {
                    allUsers = await ProcessIGraphServiceUsersCollectionPage(users);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not retrieve the user's list: {e}");
                return null;
            }

            return allUsers;
        }

        public async Task<List<Beta.User>> GetNonGuestUsersAsync(int top = 15, bool useSelect = true)
        {
            List<Beta.User> allUsers = new List<Beta.User>();
            Beta.IGraphServiceUsersCollectionPage users = null;

            try
            {
                if (!useSelect)
                {
                    users = await _graphServiceClient.Users.Request().Top(top).GetAsync();
                }
                else
                {
                    users = await _graphServiceClient.Users.Request()
                        .Select("id,displayName,givenName,surname,mail,mailNickname,userPrincipalName,imAddresses,userType,jobTitle,accountEnabled,country,usageLocation,otherMails,proxyAddresses,identities,passwordPolicies")
                        .Filter("userType eq 'Member'")
                        .Top(top).GetAsync();
                }

                if (users != null)
                {
                    allUsers = await ProcessIGraphServiceUsersCollectionPage(users);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not retrieve the user's list: {e}");
                return null;
            }

            return allUsers;
        }

        private async Task<List<Beta.User>> ProcessIGraphServiceUsersCollectionPage(Beta.IGraphServiceUsersCollectionPage users)
        {
            List<Beta.User> allUsers = new List<Beta.User>();

            try
            {
                if (users != null)
                {
                    do
                    {
                        // Page through results
                        foreach (var user in users.CurrentPage)
                        {
                            //Console.WriteLine($"User:{user.DisplayName}");
                            allUsers.Add(user);
                        }

                        // are there more pages (Has a @odata.nextLink ?)
                        if (users.NextPageRequest != null)
                        {
                            users = await users.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            users = null;
                        }
                    } while (users != null);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not process the user's list: {e}");
                return null;
            }

            return allUsers;
        }
    }
}