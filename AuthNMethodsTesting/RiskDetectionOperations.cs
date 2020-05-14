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
    public class RiskDetectionOperations
    {
        private Beta.GraphServiceClient _graphServiceClient;

        public RiskDetectionOperations(Beta.GraphServiceClient graphServiceClient)
        {
            this._graphServiceClient = graphServiceClient;
        }

        public async Task<List<Beta.RiskDetection>> ListRiskDetectionsAsync(int top = 299)
        {
            List<Beta.RiskDetection> allLocations = new List<Beta.RiskDetection>();
            Beta.IGraphServiceRiskDetectionsCollectionPage riskdetections = null;

            try
            {
                riskdetections = await _graphServiceClient.RiskDetections.Request().OrderBy("detectedDateTime%20desc").GetAsync();

                if (riskdetections != null)
                {
                    allLocations = await ProcessIGraphServiceRiskDetectionsCollectionPage(riskdetections, top);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not retrieve the risk detections: {e}");
                return null;
            }

            return allLocations;
        }

        public async Task<List<Beta.RiskDetection>> ListRiskDetectionsByUpnAsync(string userPrincipalName)
        {
            List<Beta.RiskDetection> allLocations = new List<Beta.RiskDetection>();
            Beta.IGraphServiceRiskDetectionsCollectionPage riskdetections = null;

            try
            {
                riskdetections = await _graphServiceClient.RiskDetections.Request()
                    .Filter($"userPrincipalName eq '{userPrincipalName}'")
                    .OrderBy("detectedDateTime%20desc")
                    .GetAsync();

                if (riskdetections != null)
                {
                    allLocations = await ProcessIGraphServiceRiskDetectionsCollectionPage(riskdetections, 499);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not retrieve the risk detections: {e}");
                return null;
            }

            return allLocations;
        }

        public async Task<Beta.RiskDetection> GetRiskDetectionByIdAsync(string riskDetectionId)
        {
            try
            {
                return await _graphServiceClient.RiskDetections[riskDetectionId].Request().GetAsync();
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

        public async Task<string> PrintRiskDetectionAsync(Beta.RiskDetection riskDetection, bool verbose = false)
        {
            string toPrint = string.Empty;
            StringBuilder more = new StringBuilder();

            if (riskDetection != null)
            {
                toPrint = $"UPN-{riskDetection.UserPrincipalName}, RiskType-{riskDetection.RiskType}, RiskLevel-{riskDetection?.RiskLevel.Value}, RiskState-{riskDetection?.RiskState.Value}, RiskDetail-{riskDetection?.RiskDetail.Value}, DetectedDateTime-{riskDetection.DetectedDateTime}";

                if (verbose)
                {
                    toPrint = toPrint + $", Id-{riskDetection.Id}, DisplayName-{riskDetection.UserDisplayName}, Source-{riskDetection.Source}, DetectionTimingType-{riskDetection.DetectionTimingType}, Activity-{riskDetection.Activity}, TokenIssuerType-{riskDetection.TokenIssuerType}, IpAddress-{riskDetection.IpAddress}";

                    more.AppendLine($"Location-> City-{riskDetection.Location.City}, State-{riskDetection.Location.State}, CountryOrRegion-{riskDetection.Location.CountryOrRegion}, GeoCoordinates-{riskDetection.Location?.GeoCoordinates}");
                    if (riskDetection.AdditionalData?.Count > 0)
                    {
                        more.AppendLine($"\tAdditional Data");

                        await riskDetection.AdditionalData?.ForEachAsync(data =>
                        {
                            more.AppendLine($"\t\t{data}");
                        });
                    }
                    more.AppendLine($"\tActivityDateTime-{riskDetection.ActivityDateTime}, LastUpdatedDateTime-{riskDetection.LastUpdatedDateTime}, UserId-{riskDetection.UserId}, RequestId-{riskDetection.RequestId}, CorrelationId-{riskDetection.CorrelationId}");
                }

                //}
            }
            else
            {
                toPrint = "The provided risk detection is null!";
            }

            return toPrint + more.ToString();
        }

        private async Task<List<Beta.RiskDetection>> ProcessIGraphServiceRiskDetectionsCollectionPage(Beta.IGraphServiceRiskDetectionsCollectionPage riskDetections, int top)
        {
            List<Beta.RiskDetection> allriskDetections = new List<Beta.RiskDetection>();

            try
            {
                if (riskDetections != null)
                {
                    do
                    {
                        // Page through results
                        foreach (var riskDetection in riskDetections.CurrentPage)
                        {
                            //Console.WriteLine($"Role:{riskDetections.DisplayName}");
                            allriskDetections.Add(riskDetection);
                        }

                        // are there more pages (Has a @odata.nextLink ?)
                        if (riskDetections.NextPageRequest != null)
                        {
                            riskDetections = await riskDetections.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            riskDetections = null;
                        }

                        Console.WriteLine($"allriskDetections.Count-{allriskDetections.Count}");
                    } while (allriskDetections.Count >= top || riskDetections != null);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not process the risk detections list: {e}");
                return null;
            }

            return allriskDetections;
        }
    }
}