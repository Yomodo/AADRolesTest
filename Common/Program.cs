﻿namespace SampleInviteApp
{
    using Microsoft.Identity.Client;
    using Newtonsoft.Json;
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;

    // Dummy Program.cs, DONT EXECUTE !
    class Program
    {
        /// <summary>
        /// Microsoft Graph resource.
        /// </summary>
        private static readonly string GraphResource = "https://graph.microsoft.com";

        /// <summary>
        /// Microsoft Graph invite endpoint.
        /// </summary>
        private static readonly string InviteEndPoint = "https://graph.microsoft.com/v1.0/invitations";

        /// <summary>
        ///  Authentication endpoint to get token.
        /// </summary>
        private static string AadInstance = "https://login.microsoftonline.com/{0}/v2.0";

        /// <summary>
        /// This is the tenantid of the tenant you want to invite users to.
        /// </summary>
        private static readonly string TenantID = "";

        /// <summary>
        /// This is the application id of the application that is registered in the above tenant.
        /// The required scopes are available in the below link.
        /// https://developer.microsoft.com/graph/docs/api-reference/v1.0/api/invitation_post
        /// </summary>
        private static readonly string TestAppClientId = "";

        /// <summary>
        /// Client secret of the application.
        /// </summary>
        private static readonly string TestAppClientSecret = @"";

        /// <summary>
        /// This is the email address of the user you want to invite.
        /// </summary>
        private static readonly string InvitedUserEmailAddress = @"";

        /// <summary>
        /// This is the display name of the user you want to invite.
        /// </summary>
        private static readonly string InvitedUserDisplayName = @"";

        /// <summary>
        /// Main method.
        /// </summary>
        /// <param name="args">Optional arguments</param>
        private static void Main(string[] args)
        {
            Invitation invitation = CreateInvitation();
            SendInvitation(invitation);
        }

        /// <summary>
        /// Create the invitation object.
        /// </summary>
        /// <returns>Returns the invitation object.</returns>
        private static Invitation CreateInvitation()
        {
            // Set the invitation object.
            Invitation invitation = new Invitation();
            invitation.InvitedUserDisplayName = InvitedUserDisplayName;
            invitation.InvitedUserEmailAddress = InvitedUserEmailAddress;
            invitation.InviteRedirectUrl = "https://www.microsoft.com";
            invitation.SendInvitationMessage = true;
            return invitation;
        }

        /// <summary>
        /// Send the guest user invite request.
        /// </summary>
        /// <param name="invitation">Invitation object.</param>
        private static void SendInvitation(Invitation invitation)
        {
            string accessToken = GetAccessToken();

            HttpClient httpClient = GetHttpClient(accessToken);

            // Make the invite call.
            HttpContent content = new StringContent(JsonConvert.SerializeObject(invitation));
            content.Headers.Add("ContentType", "application/json");
            var postResponse = httpClient.PostAsync(InviteEndPoint, content).Result;
            string serverResponse = postResponse.Content.ReadAsStringAsync().Result;
            Console.WriteLine(serverResponse);
        }

        /// <summary>
        /// Get the HTTP client.
        /// </summary>
        /// <param name="accessToken">Access token</param>
        /// <returns>Returns the Http Client.</returns>
        private static HttpClient GetHttpClient(string accessToken)
        {
            // setup http client.
            HttpClient httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(300);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            httpClient.DefaultRequestHeaders.Add("client-request-id", Guid.NewGuid().ToString());
            Console.WriteLine(
                "CorrelationID for the request: {0}",
                httpClient.DefaultRequestHeaders.GetValues("client-request-id").Single());
            return httpClient;
        }

        /// <summary>
        /// Get the access token for our application to talk to Microsoft Graph.
        /// </summary>
        /// <returns>Returns the access token for our application to talk to Microsoft Graph.</returns>
        private static string GetAccessToken()
        {
            string accessToken = null;

            // Get the access token for our application to talk to Microsoft Graph.
            try
            {
                string Authority = string.Format(CultureInfo.InvariantCulture, AadInstance, TenantID);
                string[] scopes = new string[] { $"{GraphResource}/.default" };

                var app = ConfidentialClientApplicationBuilder.Create(TestAppClientId)
                       .WithAuthority(Authority)
                       .WithClientSecret(TestAppClientSecret)
                       .Build();

                AuthenticationResult testAuthResult = null;
                try
                {
                    testAuthResult = app.AcquireTokenForClient(scopes).ExecuteAsync().Result;
                }
                catch (MsalServiceException ex)
                {
                    Console.WriteLine("An exception was thrown while fetching the token: {0}.", ex);
                    throw;
                }

                accessToken = testAuthResult.AccessToken;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An exception was thrown while fetching the token: {0}.", ex);
                throw;
            }

            return accessToken;
        }
    }

    /// <summary>
    /// Invitation class.
    /// </summary>
    public class Invitation
    {
        /// <summary>
        /// Gets or sets display name.
        /// </summary>
        public string InvitedUserDisplayName { get; set; }

        /// <summary>
        /// Gets or sets display name.
        /// </summary>
        public string InvitedUserEmailAddress { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Invitation Manager should send the email to InvitedUser.
        /// </summary>
        public bool SendInvitationMessage { get; set; }

        /// <summary>
        /// Gets or sets invitation redirect URL
        /// </summary>
        public string InviteRedirectUrl { get; set; }
    }
}