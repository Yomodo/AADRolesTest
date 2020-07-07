extern alias BetaLib;

using Common;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Beta = BetaLib.Microsoft.Graph;

namespace OfficeApis
{
    public class CommunicationsOperations
    {
        private Beta.GraphServiceClient _graphServiceClient;

        public CommunicationsOperations(Beta.GraphServiceClient graphServiceClient)
        {
            this._graphServiceClient = graphServiceClient;
        }

        public async Task<Beta.Presence> GetMyPresenceAsync()
        {
            return await _graphServiceClient.Me.Presence.Request().GetAsync();
        }
    }
}
