extern alias BetaLib;

using Common;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Beta = BetaLib.Microsoft.Graph;

namespace ARMApi
{
    public class AdministrativeUnitOperations
    {
        private Beta.GraphServiceClient _graphServiceClient;

        public AdministrativeUnitOperations(Beta.GraphServiceClient graphServiceClient)
        {
            this._graphServiceClient = graphServiceClient;
        }

        public async Task<Beta.AdministrativeUnit> GetAdministrativeUnitByIdAsync(string administrativeUnitId)
        {
            try
            {
                var AdministrativeUnits = await _graphServiceClient.AdministrativeUnits.Request().Filter($"id eq '{administrativeUnitId}'").GetAsync();
                return AdministrativeUnits?.CurrentPage?.FirstOrDefault();
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

        public String PrintAdministrativeUnit(Beta.AdministrativeUnit administrativeUnit)
        {
            string retval = string.Empty;

            if(administrativeUnit == null)
            {
                retval = "The provided AdministrativeUnit instabce is null";
            }

            retval = $"DisplayName-{administrativeUnit.DisplayName}";

            return retval;
        }
    }
}
