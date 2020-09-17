extern alias BetaLib;

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using AuthNMethodsTesting.Model;
using System.Linq;
using System.Net.Http;
using Beta = BetaLib.Microsoft.Graph;
using System.Threading.Tasks;
using Common;
using Microsoft.Graph;

namespace AuthNMethodsTesting
{
    public class AlertsOperations
    {
        private Beta.GraphServiceClient _graphServiceClient;

        public AlertsOperations(Beta.GraphServiceClient graphServiceClient)
        {
            this._graphServiceClient = graphServiceClient;
        }

        public async Task<List<Beta.Alert>> ListAlertsAsync()
        {
            var alerts = await _graphServiceClient.Security.Alerts.Request().GetAsync();
            return await ProcessISecurityAlertsCollectionPage(alerts);
        }

        private async Task<List<Beta.Alert>> ProcessISecurityAlertsCollectionPage(Beta.ISecurityAlertsCollectionPage securityAlertsCollectionPage)
        {
            List<Beta.Alert> allalerts = new List<Beta.Alert>();

            try
            {
                if (securityAlertsCollectionPage != null)
                {
                    do
                    {
                        // Page through results
                        foreach (var alert in securityAlertsCollectionPage.CurrentPage)
                        {
                            allalerts.Add(alert);
                        }

                        // are there more pages (Has a @odata.nextLink ?)
                        if (securityAlertsCollectionPage.NextPageRequest != null)
                        {
                            securityAlertsCollectionPage = await securityAlertsCollectionPage.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            securityAlertsCollectionPage = null;
                        }
                    } while (securityAlertsCollectionPage != null);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not process the alerts list: {e}");
                return null;
            }

            return allalerts;
        }

        public string PrintAlert(Beta.Alert alert)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Severity-{alert.Severity}, CloudAppStates-{String.Join(",", alert.CloudAppStates.ToList())}, " +
                $"Category-{alert.Category}, Confidence-{alert.Confidence}, Description-{alert.Description}, Feedback-{alert.Feedback}," +
                $" EventDateTime-{alert.EventDateTime}, Id-{alert.Id}, DetectionIds-{String.Join(",", alert.DetectionIds.ToList())}");

            return sb.ToString();
        }
    }
}
