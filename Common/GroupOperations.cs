extern alias BetaLib;

using Microsoft.Graph;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Beta = BetaLib.Microsoft.Graph;

namespace Common
{
    public class GroupOperations
    {
        private Beta.GraphServiceClient _graphServiceClient;
        private ConcurrentDictionary<string, Beta.Group> _cachedGroups;
        private const string select = "Id,DisplayName,MailNickname,MailEnabled,SecurityEnabled,OnPremisesSamAccountName,Visibility,CreatedDateTime,GroupTypes,Classification,Description,preferredDataLocation";

        public GroupOperations(Beta.GraphServiceClient graphServiceClient)
        {
            this._graphServiceClient = graphServiceClient;
            _cachedGroups = new ConcurrentDictionary<string, Beta.Group>();
        }

        public async Task<List<Beta.Group>> ListGroupsAsync(bool expandMembers = false)
        {
            List<Beta.Group> allgroups = new List<Beta.Group>();

            Beta.IGraphServiceGroupsCollectionPage groupspage = null;

            if (!expandMembers)
            {
                groupspage = await _graphServiceClient.Groups.Request().GetAsync();
            }
            else
            {
                groupspage = await _graphServiceClient.Groups.Request().Expand("members").GetAsync();
            }
            allgroups = await ProcessIGraphServiceGroupsCollectionPage(groupspage);

            return allgroups;
        }

        public async Task<List<Beta.Group>> ListDynamicGroupsAsync(bool expandMembers = false)
        {
            List<Beta.Group> allgroups = new List<Beta.Group>();

            Beta.IGraphServiceGroupsCollectionPage groupspage = null;

            if (!expandMembers)
            {
                groupspage = await _graphServiceClient.Groups.Request().Filter("groupTypes/Any(x:x eq 'DynamicMembership')").GetAsync();
            }
            else
            {
                groupspage = await _graphServiceClient.Groups.Request().Filter("groupTypes/Any(x:x eq 'DynamicMembership')").Expand("members").GetAsync();
            }
            allgroups = await ProcessIGraphServiceGroupsCollectionPage(groupspage);

            return allgroups;
        }

        public async Task<Tuple<List<Beta.Group>, string>> ListGroupsForDeltaAsync(bool selectMembers)
        {
            Beta.IGroupDeltaCollectionPage groupspage = null;

            if (!selectMembers)
            {
                groupspage = await _graphServiceClient.Groups.Delta().Request().Select($"{select}").GetAsync();
            }
            else
            {
                groupspage = await _graphServiceClient.Groups.Delta().Request().Select($"{select},members,owners").GetAsync();
            }

            Tuple<List<Beta.Group>, string> allgroupswithDelta = await ProcessIGroupDeltaCollectionPage(groupspage);

            return allgroupswithDelta;
        }

        public async Task<Tuple<List<Beta.Group>, string>> ListGroupsForDeltaAsync(string deltaLink)
        {
            if (string.IsNullOrWhiteSpace(deltaLink))
            {
                throw new ArgumentNullException(nameof(deltaLink));
            }

            Beta.IGroupDeltaCollectionPage groupspage = new Beta.GroupDeltaCollectionPage();
            groupspage.InitializeNextPageRequest(this._graphServiceClient, deltaLink);

            groupspage = await groupspage.NextPageRequest.GetAsync();

            Tuple<List<Beta.Group>, string> allgroupswithDelta = await ProcessIGroupDeltaCollectionPage(groupspage);

            return allgroupswithDelta;
        }

        private async Task<List<Beta.Group>> ProcessIGraphServiceGroupsCollectionPage(Beta.IGraphServiceGroupsCollectionPage groupsCollectionPage)
        {
            List<Beta.Group> allGroups = new List<Beta.Group>();

            try
            {
                if (groupsCollectionPage != null)
                {
                    do
                    {
                        // Page through results
                        foreach (var group in groupsCollectionPage.CurrentPage)
                        {
                            Beta.IGroupMembersCollectionWithReferencesPage members = group.Members;
                            Beta.IGroupOwnersCollectionWithReferencesPage owners = group.Owners;

                            _cachedGroups[group.Id] = group;
                            allGroups.Add(group);
                        }

                        // are there more pages (Has a @odata.nextLink ?)
                        if (groupsCollectionPage.NextPageRequest != null)
                        {
                            groupsCollectionPage = await groupsCollectionPage.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            groupsCollectionPage = null;
                        }
                    } while (groupsCollectionPage != null);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not process the groups list: {e}");
                return null;
            }

            return allGroups;
        }

        private async Task<Tuple<List<Beta.Group>, string>> ProcessIGroupDeltaCollectionPage(Beta.IGroupDeltaCollectionPage groupsCollectionPage)
        {
            List<Beta.Group> allGroups = new List<Beta.Group>();
            string deltaLink = string.Empty;

            try
            {
                if (groupsCollectionPage != null)
                {
                    do
                    {
                        // Page through results
                        foreach (var group in groupsCollectionPage.CurrentPage)
                        {
                            _cachedGroups[group.Id] = group;
                            allGroups.Add(group);
                        }

                        // are there more pages (Has a @odata.nextLink ?)
                        if (groupsCollectionPage.NextPageRequest != null)
                        {
                            groupsCollectionPage = await groupsCollectionPage.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            deltaLink = (string)groupsCollectionPage.AdditionalData["@odata.deltaLink"];
                            groupsCollectionPage = null;
                        }
                    } while (groupsCollectionPage != null);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"We could not process the groups delta list: {e}");
                return null;
            }

            return new Tuple<List<Beta.Group>, string>(allGroups, deltaLink);
        }

        public string PrintGroupBasic(Beta.Group group)
        {
            string toPrint = string.Empty;

            if (group != null)
            {
                toPrint = $"DisplayName-{group.DisplayName}, MailNickname- {group.MailNickname}";
            }
            else
            {
                Console.WriteLine("The provided group is null!");
            }

            return toPrint;
        }

        public async Task<string> PrintGroupDetails(Beta.Group group, bool verbose = false, bool printMembership = false)
        {
            StringBuilder sb = new StringBuilder();

            if (group != null)
            {
                string toprint = $"DisplayName-{group.DisplayName}, MailNickname- {group.MailNickname}, Id-{group.Id}";

                if (group.GroupTypes != null)
                {
                    toprint = toprint + $"GroupTypes -{ String.Join(",", group?.GroupTypes.ToList())}";
                }

                if (group.MailEnabled.HasValue)
                {
                    toprint = toprint + $"MailEnabled-{group.MailEnabled}, mail-{group.Mail} ";
                }

                if (group.SecurityEnabled.HasValue)
                {
                    toprint = toprint + $"SecurityEnabled-{group.SecurityEnabled}, ";
                }

                if (!string.IsNullOrWhiteSpace(group.MembershipRule))
                {
                    toprint = toprint + $"\nMembershipRule-{group.MembershipRule}, membershipRuleProcessingState-{group.MembershipRuleProcessingState}, renewedDateTime-{group.RenewedDateTime} ";
                }

                sb.AppendLine(toprint);

                if (verbose)
                {
                    sb.AppendLine($"\n AllowExternalSenders- {group?.AllowExternalSenders},Visibility-{group?.Visibility}, CreatedDateTime- {group.CreatedDateTime}" +
                    $",Classification-{group?.Classification}, Description-{group?.Description}," +
                        $"preferredDataLocation--{group?.PreferredDataLocation}, ");

                    if (group.ProxyAddresses != null && group.ProxyAddresses.Count() > 0)
                    {
                        toprint = toprint + $"\nProxyAddresses-{ String.Join(",", group?.ProxyAddresses.ToList())}";
                    }

                    if (!string.IsNullOrWhiteSpace(group.OnPremisesSamAccountName))
                    {
                        toprint = toprint + $"OnPremisesSamAccountName-{group.OnPremisesSamAccountName}, securityIdentifier, onPremisesSecurityIdentifier, onPremisesSyncEnabled, onPremisesDomainName, onPremisesNetBiosName";
                    }

                    // TODO: Many more properties to add

                    if (group?.AdditionalData?.Count > 0)
                    {
                        sb.AppendLine($"\n\t AdditionalData");

                        group.AdditionalData.ForEach(data =>
                        {
                            sb.AppendLine($"\t\t{data}");
                        });
                    }
                }

                if (printMembership)
                {
                    var owners = await GetGroupOwnersAsync(group);

                    if (owners.Count() > 0)
                    {
                        sb.AppendLine($"-------------Group owners ({owners.Count()})----------------");
                        owners.ForEach(x => sb.AppendLine($"\t{x.Id}, {x.DisplayName} "));
                        sb.AppendLine("-----------------------------------------");
                    }

                    var members = await GetGroupMembersAsync(group);

                    if (members.Count() > 0)
                    {
                        sb.AppendLine($"-------------Group members({members.Count()})----------------");
                        members.ForEach(y => sb.AppendLine($"\t{y.Id}, {y.DisplayName} "));
                        sb.AppendLine("-----------------------------------------");
                    }
                }
            }
            else
            {
                Console.WriteLine("The provided group is null!");
            }

            return sb.ToString();
        }

        public async Task<Beta.Group> CreateUnifiedGroupAsync(
            string tenantDomain = "kkaad.onmicrosoft.com",
            IEnumerable<Beta.User> membersToAddList = null,
            IEnumerable<Beta.User> ownersToAddList = null)
        {
            RandomStrings randstring = new RandomStrings(16);
            randstring.GetRandom(); // Initialize

            IEnumerable<Beta.User> owners = ownersToAddList;
            IEnumerable<Beta.User> members = membersToAddList;
            UserOperations userOperations = new UserOperations(_graphServiceClient);

            if (membersToAddList == null)
            {
                membersToAddList = await userOperations.GetUsersAsync();
                members = GenericUtility<Beta.User>.GetaRandomNumberOfItemsFromList(membersToAddList, 15);
            }

            if (ownersToAddList == null)
            {
                ownersToAddList = await userOperations.GetNonGuestUsersAsync();
                owners = GenericUtility<Beta.User>.GetaRandomNumberOfItemsFromList(ownersToAddList, 2);
            }

            Beta.Group newGroupObject = null;

            string displayname = $"My Unified group created on {DateTime.Now.ToString("F")} for testing";
            string mailNickName = randstring.GetRandom();
            string upn = $"{mailNickName}@{tenantDomain}";

            if (string.IsNullOrWhiteSpace(mailNickName))
            {
                throw new ArgumentNullException("mainickname was not randomly generated");
            }

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
                    MembersReference = members.Select(u => $"https://graph.microsoft.com/v1.0/users/{u.Id}").ToArray()
                };

                newGroupObject = await _graphServiceClient.Groups.Request().AddAsync(newGroup);
            }
            catch (ServiceException e)
            {
                Console.WriteLine("We could not add a new group: " + e.Error.Message);
                return null;
            }

            return newGroupObject;
        }


        public async Task<Beta.Group> CreateDynamicGroupAsync(
            string tenantDomain = "kkaad.onmicrosoft.com",
            IEnumerable<Beta.User> ownersToAddList = null)
        {
            IEnumerable<Beta.User> owners = ownersToAddList;
            UserOperations userOperations = new UserOperations(_graphServiceClient);

            if (ownersToAddList == null)
            {
                ownersToAddList = await userOperations.GetNonGuestUsersAsync();
                owners = GenericUtility<Beta.User>.GetaRandomNumberOfItemsFromList(ownersToAddList, 2);
            }

            RandomStrings randstring = new RandomStrings(16);
            randstring.GetRandom(); // Initialize


            Beta.Group newGroupObject = null;

            string displayname = $"My dynamic group created on {DateTime.Now.ToString("F")} for testing";
            string mailNickName = randstring.GetRandom();
            string upn = $"{mailNickName}@{tenantDomain}";

            if (string.IsNullOrWhiteSpace(mailNickName))
            {
                throw new ArgumentNullException("mainickname was not randomly generated");
            }

            try
            {
                GroupExtended newGroup = new GroupExtended
                {
                    GroupTypes = new List<string> { "Unified", "DynamicMembership" },
                    Description = displayname,
                    DisplayName = "My_Guest_Users",
                    MailEnabled = true,
                    MembershipRule = "user.userType eq \"Guest\"",
                    MembershipRuleProcessingState = "on",
                    SecurityEnabled = true,
                    MailNickname = mailNickName,
                    Visibility = "Public",
                    OwnersReference = owners.Select(u => $"https://graph.microsoft.com/v1.0/users/{u.Id}").ToArray()
                };

                newGroupObject = await _graphServiceClient.Groups.Request().AddAsync(newGroup);
            }
            catch (ServiceException e)
            {
                Console.WriteLine("We could not add a new dynamic group: " + e.Error.Message);
                return null;
            }

            return newGroupObject;
        }

        public async Task<Beta.Group> CreateDistributionGroupAsync(
           string tenantDomain = "kkaad.onmicrosoft.com")
        {
            Beta.Group newGroupObject = null;

            string displayname = $"My distribution group created on {DateTime.Now.ToString("F")} for testing";
            string mailNickName = new RandomStrings(16).GetRandom();

            try
            {
                Beta.Group newGroup = new Beta.Group
                {
                    GroupTypes = new List<string> { "Unified" },
                    Description = displayname,
                    DisplayName = displayname,
                    MailEnabled = true,
                    SecurityEnabled = false,
                    MailNickname = mailNickName,
                    Visibility = "Public"
                };

                newGroupObject = await _graphServiceClient.Groups.Request().AddAsync(newGroup);
            }
            catch (ServiceException e)
            {
                Console.WriteLine("We could not create the distribution group: " + e.Error.Message);
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

        public async Task<Beta.Group> GetGroupByIdAsync(string groupId, bool expandMembers = false)
        {
            if (_cachedGroups.ContainsKey(groupId))
            {
                return _cachedGroups[groupId];
            }

            try
            {
                Beta.Group group = null;

                if (!expandMembers)
                {
                    group = await _graphServiceClient.Groups[groupId].Request().GetAsync();
                }
                else
                {
                    group = await _graphServiceClient.Groups[groupId].Request().Expand("members").GetAsync();
                }

                this._cachedGroups[group.Id] = group;
                return group;
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

        public async Task<Beta.Group> GetGroupByMailNickNameAsync(string mailNickName)
        {
            var groups = await _graphServiceClient.Groups.Request().Filter($"mailNickName eq '{mailNickName}'").GetAsync();
            return groups.FirstOrDefault();
        }

        public async Task AddOwnerToGroupAsync(Beta.Group group, Beta.User owner)
        {
            await _graphServiceClient.Groups[group.Id].Owners.References.Request().AddAsync(owner);
            RemoveCachedGroupInstance(group);
        }

        public async Task RemoveGroupOwnerAsync(Beta.Group group, Beta.User owner)
        {
            await _graphServiceClient.Groups[group.Id].Owners[owner.Id].Reference.Request().DeleteAsync();
            RemoveCachedGroupInstance(group);
        }

        public async Task AddMemberToGroup(Beta.Group group, Beta.User member)
        {
            await _graphServiceClient.Groups[group.Id].Members.References.Request().AddAsync(member);
            RemoveCachedGroupInstance(group);
        }

        public async Task RemoveGroupMemberAsync(Beta.Group group, Beta.User member)
        {
            await _graphServiceClient.Groups[group.Id].Members[member.Id].Reference.Request().DeleteAsync();
            RemoveCachedGroupInstance(group);
        }

        public async Task DeleteGroupAsync(Beta.Group group)
        {
            try
            {
                RemoveCachedGroupInstance(group);

                await _graphServiceClient.Groups[group.Id].Request().DeleteAsync();
            }
            catch (ServiceException e)
            {
                Console.WriteLine($"Could not delete the group with Id-{group.Id}: {e}");
            }
        }

        private void RemoveCachedGroupInstance(Beta.Group group)
        {
            if (group != null)
            {
                Beta.Group removedGroup = null;
                _cachedGroups.TryRemove(group.Id, out removedGroup);
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
            Beta.IGroupMembersCollectionWithReferencesPage groupMemberPages = null;

            Beta.IGroupMembersCollectionWithReferencesPage members = group.Members;
            
            if (group?.Members?.Count() > 0)
            {
                groupMemberPages = group.Members;
            }
            else
            {
                try
                {
                    groupMemberPages = await _graphServiceClient.Groups[group.Id].Members.Request().GetAsync();

                }
                catch (ServiceException gex)
                {
                    if (gex.StatusCode != System.Net.HttpStatusCode.NotFound)
                    {
                        throw;
                    }
                }
            }

            try
            {
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
            Beta.IGroupOwnersCollectionWithReferencesPage groupOwnersPages = null;

            Beta.IGroupOwnersCollectionWithReferencesPage owners = group.Owners;

            if (group?.Owners?.Count() > 0)
            {
                groupOwnersPages = group.Owners;
            }
            else
            {
                try
                {
                    groupOwnersPages = await _graphServiceClient.Groups[group.Id].Owners.Request().GetAsync();

                }
                catch (ServiceException gex)
                {
                    if (gex.StatusCode != System.Net.HttpStatusCode.NotFound)
                    {
                        throw;
                    }
                }                
            }

            try
            {
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