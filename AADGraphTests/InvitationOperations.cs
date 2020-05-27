extern alias BetaLib;

using Common;
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
    public class InvitationOperations
    {
        private Beta.GraphServiceClient _graphServiceClient;

        public InvitationOperations(Beta.GraphServiceClient graphServiceClient)
        {
            this._graphServiceClient = graphServiceClient;

        }

        public async Task<Beta.Invitation> SendInvitation(string firstName, string lastName, string emailAddress)
        {
            IList<Beta.Recipient> Ccrecepients = new List<Beta.Recipient>
                {
                new Beta.Recipient
                {
                     EmailAddress = new Beta.EmailAddress(){  Name="Kalyan Krishna", Address= "kkrishna@woodgrove.ms"}
                }};

            var invitation = new Beta.Invitation()
            {
                InvitedUserDisplayName = $"{firstName} {lastName}",
                InvitedUserEmailAddress = $"{emailAddress}",
                InvitedUserMessageInfo = new Beta.InvitedUserMessageInfo()
                {
                    CcRecipients = Ccrecepients,
                    CustomizedMessageBody = $"Hi {firstName}, \n Come Join the Woodgrove Tenant."

                },
                InvitedUserType = "Guest",
                InviteRedirectUrl = "https://aad.portal.azure.com",
                SendInvitationMessage = true,
            };

            return await _graphServiceClient.Invitations.Request().AddAsync(invitation);
        }
    }
}
