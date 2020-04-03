extern alias BetaLib;

using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Beta = BetaLib.Microsoft.Graph;

namespace AppRolesTesting
{
    public class GroupOperations
    {
        private Beta.GraphServiceClient _graphServiceClient;

        public GroupOperations(Beta.GraphServiceClient graphServiceClient)
        {
            this._graphServiceClient = graphServiceClient;
        }

        public async Task PrintGroupDetails(Beta.Group group, bool verbose = false)
        {
            string toPrint = string.Empty;

            if (group != null)
            {
                toPrint = $"DisplayName-{group.DisplayName}, MailNickname- {group.MailNickname}, Id-{group.Id}, ,SecurityEnabled-{group.SecurityEnabled},Visibility-{group.Visibility}," +
                    $" AllowExternalSenders- {group.AllowExternalSenders}, CreatedDateTime- {group.CreatedDateTime}";
                Console.WriteLine(toPrint);

                if (verbose)
                {
                    StringBuilder more = new StringBuilder();
                    more.AppendLine($", GroupTypes-{String.Join(",", group.GroupTypes.ToList())}, Classification-{group.Classification},Description-{group.Description}," +
                        $"MailEnabled-{group.MailEnabled}, mail-{group.Mail} " +
                        $"OnPremisesSamAccountName-{group.OnPremisesSamAccountName}, " +
                        $"preferredDataLocation-{String.Join(",", group.ProxyAddresses.ToList())}, ");

                    var owners = await GetGroupOwnersAsync(group);

                    if (owners.Count() > 0)
                    {
                        more.AppendLine("-------------Group owners----------------");
                        owners.ForEach(x => more.AppendLine($"{x.Id}, {x.DisplayName} "));
                        more.AppendLine("-----------------------------------------");
                    }
                    var members = await GetGroupMembersAsync(group);

                    if (members.Count() > 0)
                    {
                        more.AppendLine("-------------Group members----------------");
                        members.ForEach(y => more.AppendLine($"{y.Id}, {y.DisplayName} "));
                        more.AppendLine("-----------------------------------------");
                    }

                    Console.WriteLine(toPrint + more.ToString());
                }
            }
            else
            {
                Console.WriteLine("The provided group is null!");
            }
        }

        public async Task<Beta.Group> CreateGroupAsync(
            string tenantDomain = "kkaad.onmicrosoft.com",
            IEnumerable<Beta.User> membersToAddList = null,
            IEnumerable<Beta.User> ownersToAddList = null)
        {
            IEnumerable<Beta.User> owners = ownersToAddList;
            IEnumerable<Beta.User> members = membersToAddList;
            UserOperations userOperations = new UserOperations(_graphServiceClient);

            if (membersToAddList == null)
            {
                membersToAddList = AsyncHelper.RunSync(async () => await userOperations.GetUsersAsync());
                members = GenericUtility<Beta.User>.GetaRandomNumberOfItemsFromList(membersToAddList, 10);
            }

            if (ownersToAddList == null)
            {
                ownersToAddList = AsyncHelper.RunSync(async () => await userOperations.GetNonGuestUsersAsync());
                owners = GenericUtility<Beta.User>.GetaRandomNumberOfItemsFromList(ownersToAddList, 2);
            }

            Beta.Group newGroupObject = null;

            string displayname = $"My test group created on {DateTime.Now.ToString("F")} for testing";
            string mailNickName = new RandomStrings(16).GetRandom();
            string upn = $"{mailNickName}@{tenantDomain}";

            try
            {
                GroupExtended newGroup = new GroupExtended
                {
                    GroupTypes = new List<string> { "Unified" },
                    //Classification = "Low",
                    Description = displayname,
                    DisplayName = displayname,
                    MailEnabled = true,
                    SecurityEnabled = true,
                    MailNickname = mailNickName,
                    Visibility = "Public",
                    OwnersReference = owners.Select(u => $"https://graph.microsoft.com/v1.0/users/{u.Id}").ToArray(),
                    MembersReference = members.Select(u => $"https://graph.microsoft.com/v1.0/users/{u.Id}").ToArray(),
                };

                newGroupObject = await _graphServiceClient.Groups.Request().AddAsync(newGroup);
            }
            catch (ServiceException e)
            {
                Console.WriteLine("We could not add a new user: " + e.Error.Message);
                return null;
            }

            return newGroupObject;
        }

        public async Task<Beta.Group> AllowExternalSendersAsync(Beta.DirectoryObject group)
        {
            return await _graphServiceClient.Groups[group.Id].Request().UpdateAsync(new Beta.Group
            {
                AllowExternalSenders = true
            });
        }

        public async Task<Beta.Group> GetGroupByIdAsync(Beta.DirectoryObject group)
        {
            var groupspage = await _graphServiceClient.Groups.Request().Filter($"id eq '{group.Id}'").GetAsync();
            return groupspage.FirstOrDefault();
        }

        public async Task<Beta.Group> GetGroupByIdAsync(string groupId)
        {
            return await _graphServiceClient.Groups[groupId].Request().GetAsync();
        }

        public async Task<Beta.Group> GetGroupByMailNickNameAsync(string mailNickName)
        {
            var groups = await _graphServiceClient.Groups.Request().Filter($"mailNickName eq '{mailNickName}'").GetAsync();
            return groups.FirstOrDefault();
        }

        public async Task<Beta.Group> AddOwnerToGroupAsync(Beta.Group group, Beta.User owner)
        {
            await _graphServiceClient.Groups[group.Id].Owners.References.Request().AddAsync(owner);
            return await GetGroupByIdAsync(group);
        }

        public async Task<Beta.Group> RemoveGroupOwnerAsync(Beta.Group group, Beta.User owner)
        {
            await _graphServiceClient.Groups[group.Id].Owners[owner.Id].Reference.Request().DeleteAsync();
            return await GetGroupByIdAsync(group);
        }

        public async Task<Beta.Group> AddMemberToGroup(Beta.Group group, Beta.User member)
        {
            await _graphServiceClient.Groups[group.Id].Members.References.Request().AddAsync(member);
            return await GetGroupByIdAsync(group);
        }

        public async Task<Beta.Group> RemoveGroupMemberAsync(Beta.Group group, Beta.User member)
        {
            await _graphServiceClient.Groups[group.Id].Members[member.Id].Reference.Request().DeleteAsync();
            return await GetGroupByIdAsync(group);
        }

        public async Task DeleteGroupAsync(Beta.Group group)
        {
            try
            {
                await _graphServiceClient.Groups[group.Id].Request().DeleteAsync();
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"Could not delete the group with Id-{group.Id}: {e}");
            }
        }

        /// <summary>
        /// All members of a group.
        /// </summary>
        /// <param name="graphServiceClient">The graph service client.</param>
        /// <param name="group">The Group.</param>
        /// <returns></returns>
        private async Task<List<Beta.User>> GetGroupMembersAsync(Beta.Group group)
        {
            List<Beta.User> allMembers = new List<Beta.User>();

            try
            {
                var groupMemberPages = await _graphServiceClient.Groups[group.Id].Members.Request().GetAsync();

                if (groupMemberPages != null)
                {
                    do
                    {
                        // Page through results
                        foreach (var user in groupMemberPages.CurrentPage)
                        {
                            allMembers.Add(user as Beta.User);
                        }

                        // are there more pages (Has a @odata.nextLink ?)
                        if (groupMemberPages.NextPageRequest != null)
                        {
                            groupMemberPages = await groupMemberPages.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            groupMemberPages = null;
                        }
                    } while (groupMemberPages != null);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not retrieve the group members: {e}");
                return null;
            }

            return allMembers;
        }

        /// <summary>
        /// All owners of a group.
        /// </summary>
        /// <param name="graphServiceClient">The graph service client.</param>
        /// <param name="group">The Group.</param>
        /// <returns></returns>
        private async Task<List<Beta.User>> GetGroupOwnersAsync(Beta.Group group)
        {
            List<Beta.User> allOwners = new List<Beta.User>();

            try
            {
                var groupOwnersPages = await _graphServiceClient.Groups[group.Id].Owners.Request().GetAsync();

                if (groupOwnersPages != null)
                {
                    do
                    {
                        // Page through results
                        foreach (var user in groupOwnersPages.CurrentPage)
                        {
                            allOwners.Add(user as Beta.User);
                        }

                        // are there more pages (Has a @odata.nextLink ?)
                        if (groupOwnersPages.NextPageRequest != null)
                        {
                            groupOwnersPages = await groupOwnersPages.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            groupOwnersPages = null;
                        }
                    } while (groupOwnersPages != null);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not retrieve the group owners: {e}");
                return null;
            }

            return allOwners;
        }
    }
}