using Common;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AADGraphTesting
{
    internal class GroupSettingOperations
    {
        private GraphServiceClient _graphServiceClient;

        public GroupSettingOperations(GraphServiceClient graphServiceClient)
        {
            this._graphServiceClient = graphServiceClient;
        }

        public async Task<List<GroupSettingTemplate>> ListGroupSettingTemplatesAsync()
        {
            List<GroupSettingTemplate> allGroupSettingTemplates = new List<GroupSettingTemplate>();

            IGraphServiceGroupSettingTemplatesCollectionPage groupSettingtemplates = null;

            try
            {
                groupSettingtemplates = await _graphServiceClient.GroupSettingTemplates.Request().GetAsync();

                if (groupSettingtemplates != null)
                {
                    allGroupSettingTemplates = await ProcessIGraphServiceGroupSettingTemplatesCollectionPage(groupSettingtemplates);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not retrieve the group setting templates list: {e}");
                return null;
            }

            return allGroupSettingTemplates;
        }

        public async Task<GroupSettingTemplate> GetGroupSettingTemplateByIdAsync(string groupSettingTemplateId)
        {
            try
            {
                var groupSettingTemplate = await _graphServiceClient.GroupSettingTemplates[groupSettingTemplateId].Request().GetAsync();
                return groupSettingTemplate;
            }
            catch (Microsoft.Graph.ServiceException gex)
            {
                if (gex.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    throw;
                }
            }
            return null;
        }

        public void PrintGroupSettingTemplates(GroupSettingTemplate groupSettingTemplate)
        {
            if (groupSettingTemplate != null)
            {
                ColorConsole.WriteLine(ConsoleColor.Green, $"Id-{groupSettingTemplate.Id}, DisplayName-{groupSettingTemplate.DisplayName}, Description-{groupSettingTemplate.Description}");
                groupSettingTemplate.Values.ForEach(x =>
                {
                    ColorConsole.WriteLine(ConsoleColor.Green, $"\tName-{x.Name}, Type-{x.Type}, Type-{x.DefaultValue}, Description-{x.Description}");
                });
            }
            else
            {
                ColorConsole.WriteLine(ConsoleColor.Green, $"The provided Group setting template is null");
            }
        }

        public async Task<List<GroupSetting>> ListGroupSettingsAsync()
        {
            List<GroupSetting> allGroupSettings = new List<GroupSetting>();

            IGraphServiceGroupSettingsCollectionPage groupSettings = null;

            try
            {
                groupSettings = await _graphServiceClient.GroupSettings.Request().GetAsync();

                if (groupSettings != null)
                {
                    allGroupSettings = await ProcessIGraphServiceGroupSettingsCollectionPagePage(groupSettings);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not retrieve the group settings list: {e}");
                return null;
            }

            return allGroupSettings;
        }

        public async Task PrintGroupSettingsAsync(GroupSetting groupSetting)
        {
            if (groupSetting != null)
            {
                GroupSettingTemplate groupSettingTemplate = await GetGroupSettingTemplateByIdAsync(groupSetting.TemplateId);
                ColorConsole.WriteLine(ConsoleColor.Green, $"Id-{groupSetting.Id}, DisplayName-{groupSetting.DisplayName}, Template-{groupSettingTemplate.DisplayName}");
                groupSetting.Values.ForEach(x =>
                {
                    ColorConsole.WriteLine(ConsoleColor.Green, $"\tName-{x.Name}, Type-{x.Value.ToString()}");
                });
            }
            else
            {
                ColorConsole.WriteLine(ConsoleColor.Green, $"The provided Group setting template is null");
            }
        }

        private async Task<List<GroupSettingTemplate>> ProcessIGraphServiceGroupSettingTemplatesCollectionPage(IGraphServiceGroupSettingTemplatesCollectionPage groupSettings)
        {
            List<GroupSettingTemplate> allGroupSettingTemplates = new List<GroupSettingTemplate>();

            try
            {
                if (groupSettings != null)
                {
                    do
                    {
                        // Page through results
                        foreach (var roleAssignment in groupSettings.CurrentPage)
                        {
                            allGroupSettingTemplates.Add(roleAssignment);
                        }

                        // are there more pages (Has a @odata.nextLink ?)
                        if (groupSettings.NextPageRequest != null)
                        {
                            groupSettings = await groupSettings.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            groupSettings = null;
                        }
                    } while (groupSettings != null);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not process the group setting templates list: {e}");
                return null;
            }

            return allGroupSettingTemplates;
        }

        private async Task<List<GroupSetting>> ProcessIGraphServiceGroupSettingsCollectionPagePage(IGraphServiceGroupSettingsCollectionPage groupSettings)
        {
            List<GroupSetting> allGroupSettings = new List<GroupSetting>();

            try
            {
                if (groupSettings != null)
                {
                    do
                    {
                        // Page through results
                        foreach (var roleAssignment in groupSettings.CurrentPage)
                        {
                            allGroupSettings.Add(roleAssignment);
                        }

                        // are there more pages (Has a @odata.nextLink ?)
                        if (groupSettings.NextPageRequest != null)
                        {
                            groupSettings = await groupSettings.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            groupSettings = null;
                        }
                    } while (groupSettings != null);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not process the group settings list: {e}");
                return null;
            }

            return allGroupSettings;
        }
    }
}