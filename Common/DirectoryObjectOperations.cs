extern alias BetaLib;

using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beta = BetaLib.Microsoft.Graph;

namespace Common
{
    public class DirectoryObjectOperations
    {
        private Beta.GraphServiceClient _graphServiceClient;

        public DirectoryObjectOperations(Beta.GraphServiceClient graphServiceClient)
        {
            this._graphServiceClient = graphServiceClient;
        }
    }
}
