extern alias BetaLib;

using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beta = BetaLib.Microsoft.Graph;

namespace AppRolesTesting
{
    public class UserOperations
    {
        private Beta.GraphServiceClient _graphServiceClient;

        public UserOperations(Beta.GraphServiceClient graphServiceClient)
        {
            this._graphServiceClient = graphServiceClient;
        }

        public async Task<List<Beta.User>> GetUsersAsync(int top = 15)
        {
            List<Beta.User> allUsers = new List<Beta.User>();

            try
            {
                Beta.IGraphServiceUsersCollectionPage users = await _graphServiceClient.Users.Request().Top(top).GetAsync();

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

        public async Task<List<Beta.User>> GetNonGuestUsersAsync(int top = 15)
        {
            List<Beta.User> allUsers = new List<Beta.User>();

            try
            {
                Beta.IGraphServiceUsersCollectionPage users = await _graphServiceClient.Users.Request().Top(top).Filter("userType eq 'Member'").GetAsync();

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