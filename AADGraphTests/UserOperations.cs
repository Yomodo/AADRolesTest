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
            return await GetUserByIdAsync(owner.Id);
        }

        public async Task<Beta.User> CreateUserAsync(string givenName, string surname)
        {
            Beta.User newUserObject = null;

            string displayname = $"{givenName} {surname}";
            string mailNickName = $"{givenName}{surname}";
            string upn = $"{mailNickName}@kkaad.onmicrosoft.com";
            string password = "p@$$w0rd!";

            try
            {
                newUserObject = await _graphServiceClient.Users.Request().AddAsync(new Beta.User
                {
                    AccountEnabled = true,
                    DisplayName = displayname,
                    MailNickname = mailNickName,
                    GivenName = givenName,
                    Surname = surname,
                    PasswordProfile = new Beta.PasswordProfile
                    {
                        Password = password
                    },
                    UserPrincipalName = upn
                });
            }
            catch (ServiceException e)
            {
                Console.WriteLine("We could not add a new user: " + e.Error.Message);
                return null;
            }

            return newUserObject;
        }

        public void PrintUserDetails(User user)
        {
            if (user != null)
            {
                Console.WriteLine($"DisplayName-{user.DisplayName}, MailNickname- {user.MailNickname}, GivenName-{user.GivenName}, Surname-{user.Surname}, Upn-{user.UserPrincipalName}, JobTitle-{user.JobTitle}, Id-{user.Id}");
            }
            else
            {
                Console.WriteLine("The provided User is null!");
            }
        }

        public void PrintBetaUserDetails(Beta.User user)
        {
            if (user != null)
            {
                Console.WriteLine($"DisplayName-{user.DisplayName}, MailNickname- {user.MailNickname}, GivenName-{user.GivenName}, Surname-{user.Surname}, Upn-{user.UserPrincipalName}, JobTitle-{user.JobTitle}, Id-{user.Id}");
            }
            else
            {
                Console.WriteLine("The provided User is null!");
            }
        }

        public async Task<Beta.User> UpdateUserAsync(string userId, string jobTitle)
        {
            Beta.User updatedUser = null;
            try
            {
                // Update the user.
                updatedUser = await _graphServiceClient.Users[userId].Request().UpdateAsync(new Beta.User
                {
                    JobTitle = jobTitle
                });
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not update details of the user with Id {userId}: {e}");
            }

            return updatedUser;
        }

        public async Task DeleteUserAsync(string userId)
        {
            try
            {
                await _graphServiceClient.Users[userId].Request().DeleteAsync();
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not delete the user with Id-{userId}: {e}");
            }
        }

        public async Task<List<Beta.User>> GetMyDirectReportsAsync()
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