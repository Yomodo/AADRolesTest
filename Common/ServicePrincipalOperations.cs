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
    public class ServicePrincipalOperations
    {
        private Beta.GraphServiceClient _graphServiceClient;
        private readonly ConcurrentDictionary<string, Beta.ServicePrincipal> _cachedServicePrincipals;

        public ServicePrincipalOperations(Beta.GraphServiceClient graphServiceClient)
        {
            this._graphServiceClient = graphServiceClient;
            this._cachedServicePrincipals = new ConcurrentDictionary<string, Beta.ServicePrincipal>();
        }

        public async Task<Beta.ServicePrincipal> GetServicePrincipalByAppIdAsync(string appId)
        {
            return await GetServicePrincipalBySearchFilterAsync($"appId eq '{appId}'");
        }

        public async Task<Beta.ServicePrincipal> GetServicePrincipalByAppDisplayNameAsync(string appDisplayName)
        {
            return await GetServicePrincipalBySearchFilterAsync($"displayName eq '{appDisplayName}'");
        }

        private async Task<Beta.ServicePrincipal> GetServicePrincipalBySearchFilterAsync(string searchFilter)
        {
            Beta.ServicePrincipal servicePrincipal = null;

            try
            {
                var servicePrincipals = await _graphServiceClient.ServicePrincipals.Request().Filter(searchFilter).GetAsync();
                servicePrincipal = servicePrincipals.FirstOrDefault();
                _cachedServicePrincipals[servicePrincipal.Id] = servicePrincipal;
            }
            catch (ServiceException sx)
            {
                if (sx.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    ColorConsole.WriteLine(ConsoleColor.Red, $"No service principal matching the filter-'{searchFilter}' was found");
                }
                else
                {
                    throw;
                }
            }

            return servicePrincipal;
        }

        public async Task<List<Beta.ServicePrincipal>> GetAllServicePrincipalsAsync(int top = 999)
        {
            List<Beta.ServicePrincipal> allServicePrincipals = new List<Beta.ServicePrincipal>();
            Beta.IGraphServiceServicePrincipalsCollectionPage servicePrincipals = null;

            try
            {
                servicePrincipals = await _graphServiceClient.ServicePrincipals.Request().Top(top).GetAsync();

                if (servicePrincipals != null)
                {
                    allServicePrincipals = await ProcessIGraphServiceServicePrincipalsCollectionPage(servicePrincipals);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not retrieve the servicePrincipal's list: {e}");
                return null;
            }

            return allServicePrincipals;
        }

        private async Task<List<Beta.ServicePrincipal>> ProcessIGraphServiceServicePrincipalsCollectionPage(Beta.IGraphServiceServicePrincipalsCollectionPage servicePrincipals)
        {
            List<Beta.ServicePrincipal> allServicePrincipals = new List<Beta.ServicePrincipal>();

            try
            {
                if (servicePrincipals != null)
                {
                    do
                    {
                        // Page through results
                        foreach (var servicePrincipal in servicePrincipals.CurrentPage)
                        {
                            //Console.WriteLine($"ServicePrincipal:{servicePrincipal.DisplayName}");
                            _cachedServicePrincipals[servicePrincipal.Id] = servicePrincipal;
                            allServicePrincipals.Add(servicePrincipal);
                        }

                        // are there more pages (Has a @odata.nextLink ?)
                        if (servicePrincipals.NextPageRequest != null)
                        {
                            servicePrincipals = await servicePrincipals.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            servicePrincipals = null;
                        }
                    } while (servicePrincipals != null);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not process the servicePrincipal's list: {e}");
                return null;
            }

            return allServicePrincipals;
        }

        public async Task<Beta.ServicePrincipal> GetServicePrincipalByIdAsync(string servicePrincipalId)
        {
            Beta.ServicePrincipal servicePrincipal = null;

            if (_cachedServicePrincipals.ContainsKey(servicePrincipalId))
            {
                return _cachedServicePrincipals[servicePrincipalId];
            }

            try
            {
                servicePrincipal = await _graphServiceClient.ServicePrincipals[servicePrincipalId].Request().GetAsync();
            }
            catch (ServiceException sx)
            {
                if (sx.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    ColorConsole.WriteLine(ConsoleColor.Red, $"No service principal by id-{servicePrincipalId} was found");
                }
                else
                {
                    throw;
                }
            }

            return servicePrincipal;
        }

        public string PrintServicePrincipalBasic(Beta.ServicePrincipal servicePrincipal, bool appDetails = false)
        {
            string retVal;

            if (servicePrincipal == null)
            {
                retVal = "The provided service Principal object is null";
            }

            retVal = $"AppDisplayName-'{servicePrincipal.AppDisplayName}',ServicePrincipalType-'{servicePrincipal.ServicePrincipalType}', Id-'{servicePrincipal.Id}', DisplayName-'{servicePrincipal.DisplayName}'";

            if (appDetails)
            {
                retVal = retVal + $"AppId - {servicePrincipal.AppId}, AppOwnerOrganizationId - {servicePrincipal.AppOwnerOrganizationId}, SignInAudience-{servicePrincipal.SignInAudience}";
            }
            return retVal;
        }
    }
}