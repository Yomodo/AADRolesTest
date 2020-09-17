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
    public class SecureScoresOperations
    {
        private Beta.GraphServiceClient _graphServiceClient;

        public SecureScoresOperations(Beta.GraphServiceClient graphServiceClient)
        {
            this._graphServiceClient = graphServiceClient;
        }

        public async Task<List<Beta.SecureScore>> ListSecureScoresAsync()
        {
            var secureScores = await _graphServiceClient.Security.SecureScores.Request().GetAsync();
            return await ProcessISecuritySecureScoresCollectionPage(secureScores);
        }

        private async Task<List<Beta.SecureScore>> ProcessISecuritySecureScoresCollectionPage(Beta.ISecuritySecureScoresCollectionPage securitySecureScoresCollectionPage)
        {
            List<Beta.SecureScore> allsecureScores = new List<Beta.SecureScore>();

            try
            {
                if (securitySecureScoresCollectionPage != null)
                {
                    do
                    {
                        // Page through results
                        foreach (var secureScore in securitySecureScoresCollectionPage.CurrentPage)
                        {
                            allsecureScores.Add(secureScore);
                        }

                        // are there more pages (Has a @odata.nextLink ?)
                        if (securitySecureScoresCollectionPage.NextPageRequest != null)
                        {
                            securitySecureScoresCollectionPage = await securitySecureScoresCollectionPage.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            securitySecureScoresCollectionPage = null;
                        }
                    } while (securitySecureScoresCollectionPage != null);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not process the secureScores list: {e}");
                return null;
            }

            return allsecureScores;
        }

        public string PrintSecureScore(Beta.SecureScore secureScore, bool verbose = false)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"CurrentScore-{secureScore.CurrentScore}, MaxScore-{secureScore.MaxScore}, ActiveUserCount-{secureScore.ActiveUserCount}, LicensedUserCount-{secureScore.LicensedUserCount}, AzureTenantId-{secureScore.AzureTenantId}," +
                $" CreatedDateTime-{secureScore.CreatedDateTime}, Id-{secureScore.Id}, EnabledServices-{String.Join(",", secureScore.EnabledServices.ToList())}");


            if (verbose)
            {
                if (secureScore.ControlScores.Any())
                {
                    sb.AppendLine("----Control scores----");

                    foreach (var strlscopre in secureScore.ControlScores)
                    {
                        sb.AppendLine(PrintControlScore(strlscopre));
                    }
                    sb.AppendLine("-------");
                }

                if (secureScore.AverageComparativeScores.Any())
                {
                    sb.AppendLine("----Average Comparative Scores----");

                    foreach (var averageComparativeScore in secureScore.AverageComparativeScores)
                    {
                        sb.AppendLine(PrintAverageComparativeScore(averageComparativeScore));
                    }
                    sb.AppendLine("-------");
                }
            }

            return sb.ToString();
        }


        public string PrintControlScore(Beta.ControlScore controlscore)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"ControlCategory-{controlscore.ControlCategory}, ControlName-{controlscore.ControlName}, Score-{controlscore.Score}, Description-{controlscore.Description}");

            return sb.ToString();
        }

        public string PrintAverageComparativeScore(Beta.AverageComparativeScore averageComparativeScore)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"ControlCategory-{averageComparativeScore.AverageScore}, ControlName-{averageComparativeScore.Basis}");

            return sb.ToString();
        }
    }
}
