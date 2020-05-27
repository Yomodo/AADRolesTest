extern alias BetaLib;

using Common;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Beta = BetaLib.Microsoft.Graph;

namespace Common
{
    internal class PolicyOperations
    {
        private Beta.GraphServiceClient _graphServiceClient;

        public PolicyOperations(Beta.GraphServiceClient graphServiceClient)
        {
            this._graphServiceClient = graphServiceClient;
        }

        public async Task<List<Beta.ActivityBasedTimeoutPolicy>> ListActivityBasedTimeoutPoliciesAsync()
        {
            List<Beta.ActivityBasedTimeoutPolicy> allActivityBasedTimeoutPolicies = new List<Beta.ActivityBasedTimeoutPolicy>();

            Beta.IPolicyRootActivityBasedTimeoutPoliciesCollectionPage activityBasedTimeoutPolicies = null;

            try
            {
                activityBasedTimeoutPolicies = await _graphServiceClient.Policies.ActivityBasedTimeoutPolicies.Request().GetAsync();

                if (activityBasedTimeoutPolicies != null)
                {
                    allActivityBasedTimeoutPolicies = await ProcessIPolicyRootActivityBasedTimeoutPoliciesCollectionPage(activityBasedTimeoutPolicies);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not retrieve the activity based timeout policy list: {e}");
                return null;
            }

            return allActivityBasedTimeoutPolicies;
        }

        public async Task<Beta.ActivityBasedTimeoutPolicy> CreateActivityBasedTimeoutPolicyAsync()
        {
            Beta.ActivityBasedTimeoutPolicy newActivityBasedTimeoutPolicy = null;

            //// TODO
            //string displayname = $"Activity based timeout policy created on {DateTime.Now.ToString("F")}";

            //IEnumerable<Beta.StsPolicy> policies = 
            //try
            //{
            //    Beta.ActivityBasedTimeoutPolicy newGroup = new Beta.ActivityBasedTimeoutPolicy
            //    {
            //        Description = displayname + " for testing",
            //        DisplayName = displayname,
            //        IsOrganizationDefault = false,
            //        Definition = ""
            //    };

            //    newActivityBasedTimeoutPolicy = await _graphServiceClient.Policies.ActivityBasedTimeoutPolicies.Request().AddAsync(newGroup);
            //}
            //catch (ServiceException e)
            //{
            //    Console.WriteLine("We could not create the activity based timeout policy: " + e.Error.Message);
            //    return null;
            //}

            return await Task.FromResult(newActivityBasedTimeoutPolicy);
        }


        public void PrintActivityBasedTimeoutPolicy(Beta.ActivityBasedTimeoutPolicy activityBasedTimeoutPolicy)
        {
            if (activityBasedTimeoutPolicy != null)
            {
                //GroupSettingTemplate groupSettingTemplate = await GetGroupSettingTemplateByIdAsync(activityBasedTimeoutPolicy.TemplateId);
                ColorConsole.WriteLine(ConsoleColor.Green, $"Id-{activityBasedTimeoutPolicy.Id}, " +
                    $"DisplayName-{activityBasedTimeoutPolicy.DisplayName}, " +
                    $"Description-{activityBasedTimeoutPolicy.Description}," +
                    $"IsOrganizationDefault-{activityBasedTimeoutPolicy.IsOrganizationDefault}," +
                    $"\nDefinition-{activityBasedTimeoutPolicy.Definition}");               
            }
            else
            {
                ColorConsole.WriteLine(ConsoleColor.Green, $"The provided Activity Based Timeout Policy is null");
            }
        }

        private async Task<List<Beta.ActivityBasedTimeoutPolicy>> ProcessIPolicyRootActivityBasedTimeoutPoliciesCollectionPage(Beta.IPolicyRootActivityBasedTimeoutPoliciesCollectionPage activityBasedTimeoutPolicies)
        {
            List<Beta.ActivityBasedTimeoutPolicy> allActivityBasedTimeoutPolicies = new List<Beta.ActivityBasedTimeoutPolicy>();

            try
            {
                if (activityBasedTimeoutPolicies != null)
                {
                    do
                    {
                        // Page through results
                        foreach (var roleAssignment in activityBasedTimeoutPolicies.CurrentPage)
                        {
                            allActivityBasedTimeoutPolicies.Add(roleAssignment);
                        }

                        // are there more pages (Has a @odata.nextLink ?)
                        if (activityBasedTimeoutPolicies.NextPageRequest != null)
                        {
                            activityBasedTimeoutPolicies = await activityBasedTimeoutPolicies.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            activityBasedTimeoutPolicies = null;
                        }
                    } while (activityBasedTimeoutPolicies != null);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not process the activity based timeout policy list: {e}");
                return null;
            }

            return allActivityBasedTimeoutPolicies;
        }
    }
}
