extern alias BetaLib;

using System.Collections.Generic;
using Beta = BetaLib.Microsoft.Graph;

namespace AuthNMethodsTesting.Model
{
    public class deviceRegistrationPolicy
    {
        public string id { get; set; }
        public string displayName { get; set; }
        public string description { get; set; }
        public long userDeviceQuota { get; set; }
        public string multiFactorAuthConfiguration { get; set; }
        public AzureADRegistration azureADRegistration { get; set; }
        public AzureADJoin azureADJoin { get; set; }
    }

    public class AzureADJoin
    {
        public string appliesTo { get; set; }
        public bool isAdminConfigurable { get; set; }
        public List<Beta.DirectoryObject> allowedUsers { get; set; }
        public List<Beta.DirectoryObject> allowedGroups { get; set; }
    }

    public class AzureADRegistration
    {
        public string appliesTo { get; set; }
        public bool isAdminConfigurable { get; set; }
        public List<Beta.DirectoryObject> allowedUsers { get; set; }
        public List<Beta.DirectoryObject> allowedGroups { get; set; }
    }
}