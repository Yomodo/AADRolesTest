extern alias BetaLib;

using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beta = BetaLib.Microsoft.Graph;

namespace AuthNMethodsTesting
{
    public class NamedLocationOperations
    {
        private Beta.GraphServiceClient _graphServiceClient;

        public NamedLocationOperations(Beta.GraphServiceClient graphServiceClient)
        {
            this._graphServiceClient = graphServiceClient;
        }

        public async Task<List<Beta.NamedLocation>> ListNamedLocationsAsync()
        {
            List<Beta.NamedLocation> allLocations = new List<Beta.NamedLocation>();
            Beta.IConditionalAccessRootNamedLocationsCollectionPage locations = null;

            try
            {
                locations = await _graphServiceClient.Identity.ConditionalAccess.NamedLocations.Request().GetAsync();

                if (locations != null)
                {
                    allLocations = await ProcessINamedLocationRootPoliciesCollectionPage(locations);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not retrieve the named locations: {e}");
                return null;
            }

            return allLocations;
        }

        public string PrintNamedLocation(Beta.NamedLocation namedLocation)
        {
            string toPrint = string.Empty;

            if (namedLocation != null)
            {
                toPrint = $"DisplayName-{namedLocation.DisplayName}, Id- {namedLocation.Id}";
            }
            else
            {
                Console.WriteLine("The provided named location is null!");
            }

            return toPrint;
        }

        public async Task<Beta.NamedLocation> GetNamedLocationByIdAsync(string locationId)
        {
            try
            {
                var location = await _graphServiceClient.Identity.ConditionalAccess.NamedLocations[locationId].Request().GetAsync();
                return location;
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

        private async Task<List<Beta.NamedLocation>> ProcessINamedLocationRootPoliciesCollectionPage(Beta.IConditionalAccessRootNamedLocationsCollectionPage namedLocations)
        {
            List<Beta.NamedLocation> allnamedLocations = new List<Beta.NamedLocation>();

            try
            {
                if (namedLocations != null)
                {
                    do
                    {
                        // Page through results
                        foreach (var namedLocation in namedLocations.CurrentPage)
                        {
                            //Console.WriteLine($"Role:{namedLocations.DisplayName}");
                            allnamedLocations.Add(namedLocation);
                        }

                        // are there more pages (Has a @odata.nextLink ?)
                        if (namedLocations.NextPageRequest != null)
                        {
                            namedLocations = await namedLocations.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            namedLocations = null;
                        }
                    } while (namedLocations != null);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not process the named locations List list: {e}");
                return null;
            }

            return allnamedLocations;
        }
    }
}