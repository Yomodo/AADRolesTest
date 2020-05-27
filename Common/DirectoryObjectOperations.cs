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
    public class DirectoryObjectOperations
    {
        private Beta.GraphServiceClient _graphServiceClient;
        private UserOperations _userOperations;
        private GroupOperations _groupOperations;
        private ServicePrincipalOperations _servicePrincipalOperations;

        private ConcurrentDictionary<string, Beta.DirectoryObject> _cachedDirectoryObjects;

        public DirectoryObjectOperations(Beta.GraphServiceClient graphServiceClient, UserOperations userOperations
            , GroupOperations groupOperations, ServicePrincipalOperations servicePrincipalOperations)
        {
            this._graphServiceClient = graphServiceClient;
            this._userOperations = userOperations;
            this._groupOperations = groupOperations;
            this._servicePrincipalOperations = servicePrincipalOperations;

            _cachedDirectoryObjects = new ConcurrentDictionary<string, Beta.DirectoryObject>();
        }

        public async Task<Beta.DirectoryObject> GetDirectoryObjectByIdAsync(string directoryObjectId)
        {
            Beta.DirectoryObject directoryObject = null;

            if (_cachedDirectoryObjects.ContainsKey(directoryObjectId))
            {
                return _cachedDirectoryObjects[directoryObjectId];
            }

            try
            {

                directoryObject = await _graphServiceClient.DirectoryObjects[directoryObjectId].Request().GetAsync();


                _cachedDirectoryObjects[directoryObject.Id] = directoryObject;
            }
            catch (ServiceException sx)
            {
                if (sx.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    //ColorConsole.WriteLine(ConsoleColor.Red, $"No Directory object by id-{directoryObjectId} was found");
                    return null;
                }
                else
                {
                    throw;
                }
            }

            return directoryObject;
        }

        public string PrintDirectoryObject(Beta.DirectoryObject directoryObject, bool verbose = false)
        {
            StringBuilder sb = new StringBuilder();

            if (directoryObject != null)
            {
                if (directoryObject is Beta.User)
                {
                    sb.AppendLine($"User:{this._userOperations.PrintBetaUserDetails(directoryObject as Beta.User, verbose)}");
                }
                else if (directoryObject is Beta.Group)
                {
                    sb.AppendLine($"User:{this._groupOperations.PrintGroupBasic(directoryObject as Beta.Group)}");
                }
                else if (directoryObject is Beta.ServicePrincipal)
                {
                    sb.AppendLine($"App:{this._servicePrincipalOperations.PrintServicePrincipalBasic(directoryObject as Beta.ServicePrincipal)}");
                }
                else
                {
                    ColorConsole.WriteLine(ConsoleColor.Red, $"No provision in PrintDirectoryObject for type-{directoryObject.ODataType}");
                }
            }
            else
            {
                sb.AppendLine($"Provided directoryobject is null");
            }

            return sb.ToString();
        }
    }
}
