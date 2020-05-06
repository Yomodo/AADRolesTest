extern alias BetaLib;

using Microsoft.Graph;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beta = BetaLib.Microsoft.Graph;

namespace Common
{
    public class UserOperations
    {
        private Beta.GraphServiceClient _graphServiceClient;
        private ConcurrentDictionary<string, Beta.User> _cachedUsers;
        private string _aadDomain = "kkaad.onmicrosoft.com";

        public UserOperations(Beta.GraphServiceClient graphServiceClient, string domain= "kkaad.onmicrosoft.com")
        {
            this._graphServiceClient = graphServiceClient;
            this._aadDomain = domain;
            _cachedUsers = new ConcurrentDictionary<string, Beta.User>();
        }

        public async Task<Beta.User> GetMeAsync()
        {
            // Call /me Api
            return await _graphServiceClient.Me.Request().GetAsync();
        }

        public async Task<Beta.User> GetUserByIdAsync(string principalId, bool useSelect = true)
        {
            Beta.User user = null;
            Beta.IGraphServiceUsersCollectionPage users = null;

            if (_cachedUsers.ContainsKey(principalId))
            {
                return _cachedUsers[principalId];
            }

            try
            {
                if (!useSelect)
                {
                    users = await _graphServiceClient.Users.Request().Filter($"id eq '{principalId}'")
                                       .GetAsync();
                }
                else
                {
                    users = await _graphServiceClient.Users.Request().Filter($"id eq '{principalId}'")
                                       .Select("id,displayName,givenName,surname,mail,mailNickname,userPrincipalName,imAddresses,userType,jobTitle,accountEnabled,country,usageLocation,otherMails,proxyAddresses,identities,passwordPolicies")
                                       .GetAsync();
                }

                user = users.CurrentPage.FirstOrDefault();
                _cachedUsers[principalId] = user;
            }
            catch (ServiceException sx)
            {
                if (sx.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    //ColorConsole.WriteLine(ConsoleColor.Red, $"No user by id-{principalId} was found");
                    return null;
                }
                else
                {
                    throw;
                }
            }

            return user;
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
            string upn = $"{mailNickName}@{_aadDomain}";
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

        //public void PrintUserDetails(User user)
        //{
        //    if (user != null)
        //    {
        //        Console.WriteLine($"DisplayName-{user.DisplayName}, MailNickname- {user.MailNickname}, GivenName-{user.GivenName}, Surname-{user.Surname}, Upn-{user.UserPrincipalName}, JobTitle-{user.JobTitle}, Id-{user.Id}");
        //    }
        //    else
        //    {
        //        Console.WriteLine("The provided User is null!");
        //    }
        //}

        public string PrintBetaUserDetails(Beta.User user, bool verbose = true, string userId = "")
        {
            string retval = string.Empty;

            if (user != null)
            {
                retval = $"DisplayName-{user.DisplayName}, Upn-{user.UserPrincipalName}";

                if (verbose)
                {
                    retval = retval + $" GivenName-{user.GivenName}, Surname-{user.Surname}, MailNickname- {user.MailNickname}, Id-{user.Id}, JobTitle-{user.JobTitle},";
                }
            }
            else
            {
                retval = $"The provided User is null! The Id provided was '{userId}'.";
            }

            return retval;
        }

        public async Task<Beta.User> UpdateUserAsync(string userId, string jobTitle)
        {
            Beta.User updatedUser = null;
            try
            {
                bool removed = _cachedUsers.TryRemove(userId, out updatedUser);

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

                Beta.User removedUser = null;
                bool removed = _cachedUsers.TryRemove(userId, out removedUser);
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

        public async Task<List<Beta.User>> GetUsersAsync(int top = 150, bool useSelect = true)
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

        public async Task<List<Beta.User>> GetNonGuestUsersAsync(int top = 999, bool useSelect = true)
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

        public async Task<List<Beta.AppRoleAssignment>> GetUsersAppRoleAssignmentsAsync()
        {
            List<Beta.AppRoleAssignment> allAssignments = new List<Beta.AppRoleAssignment>();

            try
            {
                Beta.IUserAppRoleAssignmentsCollectionPage assignments = await _graphServiceClient.Me.AppRoleAssignments.Request().GetAsync();

                if (assignments?.CurrentPage.Count > 0)
                {
                    foreach (Beta.AppRoleAssignment appRoleAssignment in assignments)
                    {
                        allAssignments.Add(appRoleAssignment);
                    }
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not retrieve the users app role assignments: {e}");
                return null;
            }

            return allAssignments;
        }

        public void PrintAppRoleAssignment(Beta.AppRoleAssignment assignment)
        {
            if (assignment != null)
            {
                Console.WriteLine($"AppRoleId-{assignment.AppRoleId}, PrincipalDisplayName- {assignment.PrincipalDisplayName}, " +
                    $"PrincipalType-{assignment.PrincipalType}, ResourceDisplayName-{assignment.ResourceDisplayName}, ResourceId-{assignment.ResourceId}, " +
                    $"PrincipalId-{assignment.PrincipalId}, Id-{assignment.Id}");
            }
            else
            {
                Console.WriteLine("The provided AppRoleAssignment is null!");
            }
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
                            _cachedUsers[user.Id] = user;
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