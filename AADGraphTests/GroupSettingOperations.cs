using Common;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AADGraphTesting
{
    /// <summary>
    /// Wraps group settings exposed at the tenant level
    /// https://docs.microsoft.com/en-us/azure/active-directory/users-groups-roles/groups-settings-cmdlets
    /// </summary>
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
                ColorConsole.WriteLine(ConsoleColor.Green, $"Description-{groupSettingTemplate.Description}");

                groupSettingTemplate.Values.ForEach(x =>
                {
                    ColorConsole.WriteLine(ConsoleColor.Cyan, $"\tName-{x.Name}[{x.Type}], DefaultValue-{x.DefaultValue}, Description-{x.Description}");
                });
            }
            else
            {
                ColorConsole.WriteLine(ConsoleColor.Red, $"The provided Group setting template is null");
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

        public async Task<GroupSetting> GetGroupSettingByIdAsync(string groupSettingId)
        {
            try
            {
                var groupSetting = await _graphServiceClient.GroupSettings[groupSettingId].Request().GetAsync();
                return groupSetting;
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

        public async Task<GroupSetting> GetGroupSettingByTemplateIdAsync(string groupSettingTemplateId)
        {

            // NOT SUPPORTED
            try
            {
                var groupSetting = await _graphServiceClient.GroupSettings.Request().Filter($"templateId eq '{groupSettingTemplateId}'").GetAsync();
                return groupSetting.FirstOrDefault();
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

        public async Task<GroupSetting> AddGroupSettingAsync(GroupSetting groupSetting)
        {
            GroupSetting newGroupSettingObject = null;

            try
            {
                newGroupSettingObject = await _graphServiceClient.GroupSettings.Request().AddAsync(groupSetting);
            }
            catch (ServiceException e)
            {
                Console.WriteLine("We could not add a new Group setting: " + e.Error.Message);
                return null;
            }

            return newGroupSettingObject;
        }

        public async Task<GroupSetting> UpdateGroupSettingAsync(GroupSetting groupSetting, string settingValueName, string settingValue)
        {
            GroupSetting updatedGroupSettingObject = null;

            var settingToUpdate = groupSetting.Values.FirstOrDefault(x => x.Name == settingValueName);

            try
            {
                if (settingToUpdate != null)
                {
                    settingToUpdate.Value = settingValue;
                }

                updatedGroupSettingObject = await _graphServiceClient.GroupSettings[groupSetting.Id].Request().UpdateAsync(groupSetting);
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not update the Group setting with Id-{groupSetting.Id}: " + e.Error.Message);
                return null;
            }

            return updatedGroupSettingObject;
        }

        public async Task<GroupSetting> UpdateGroupSettingAsync(string groupSettingId, string settingValueName, string settingValue)
        {
            return await this.UpdateGroupSettingAsync(await this.GetGroupSettingByIdAsync(groupSettingId), settingValueName, settingValue);
        }

        public async Task DeleteGroupSettingAsync(GroupSetting groupSetting)
        {
            try
            {
                await _graphServiceClient.GroupSettings[groupSetting.Id].Request().DeleteAsync();
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"Could not delete the groupSetting with Id-{groupSetting.Id}: {e}");
            }
        }

        public async Task PrintGroupSettingsAsync(GroupSetting groupSetting)
        {
            if (groupSetting != null)
            {
                GroupSettingTemplate groupSettingTemplate = await GetGroupSettingTemplateByIdAsync(groupSetting.TemplateId);
                ColorConsole.WriteLine(ConsoleColor.Green, $"DisplayName-{groupSetting.DisplayName}, TemplateId-{groupSetting.TemplateId}, Id-{groupSetting.Id} ");
                ColorConsole.WriteLine(ConsoleColor.Green, $"Description -{ groupSettingTemplate.Description}");
                groupSetting.Values.ForEach(x =>
                {
                    ColorConsole.WriteLine(ConsoleColor.Cyan, $"\tName-{x.Name}, Value-{x.Value.ToString()}");
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