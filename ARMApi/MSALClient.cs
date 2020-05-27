using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ARMApi
{
    public class MSALClient
    {
        #region constants

        /// <summary>
        /// The AAD authentication endpoint uri
        /// </summary>
        private static string AadInstance = "https://login.microsoftonline.com/{0}/v2.0";

        /// <summary>
        /// Microsoft Graph resource.
        /// </summary>
        static readonly string GraphResource = "https://graph.microsoft.com";

        /// <summary>
        /// Microsoft Graph invite endpoint.
        /// </summary>
        static readonly string InviteEndPoint = "https://graph.microsoft.com/v1.0/invitations";

        /// <summary>
        /// This is the tenantid of the tenant you want to invite users to.
        /// </summary>
        private static string TenantID = "979f4440-75dc-4664-b2e1-2cafa0ac67d1";

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
                    // Case when ex.Message contains:
                    // AADSTS70011 Invalid scope. The scope has to be of the form "https://resourceUrl/.default"
                    // Mitigation: change the scope to be as expected
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


        /// <summary>
        /// The application ID of the connector in AAD
        /// </summary>
        private static readonly string ConnectorAppId = "55747057-9b5d-4bd4-b387-abf52a8bd489";

        /// <summary>
        /// The reply address of the connector application in AAD
        /// </summary>
        //static readonly Uri ConnectorRedirectAddress = new Uri("https://login.microsoftonline.com/common/oauth2/nativeclient");

        /// <summary>
        /// The AppIdUri of the registration service in AAD
        /// </summary>
        private static readonly string RegistrationServiceAppIdUri = "https://proxy.cloudwebappproxy.net/registerapp/access_as_user";

        #endregion constants

        #region private members

        private string token;
        private string tenantID;

        #endregion private members

        public async Task GetAuthenticationToken()
        {
            string Authority = string.Format(CultureInfo.InvariantCulture, AadInstance, TenantID);
            string[] Scopes = { RegistrationServiceAppIdUri };

            IPublicClientApplication app = PublicClientApplicationBuilder.Create(ConnectorAppId)
               .WithAuthority(new System.Uri(Authority))
               .WithDefaultRedirectUri()
               .Build();

            var accounts = (await app.GetAccountsAsync()).ToList();
            AuthenticationResult authResult = null;

            // Get an access token to call the To Do list service.
            try
            {
                authResult = await app.AcquireTokenSilent(Scopes, accounts.FirstOrDefault())
                    .ExecuteAsync()
                    .ConfigureAwait(false);
            }
            catch (MsalUiRequiredException)
            {
                try
                {
                    // Force a sign-in (Prompt.SelectAccount), as the MSAL web browser might contain cookies for the current user
                    // and we don't necessarily want to re-sign-in the same user
                    authResult = await app.AcquireTokenInteractive(Scopes)
                        .WithAccount(accounts.FirstOrDefault())
                        .WithPrompt(Prompt.SelectAccount)
                        .ExecuteAsync()
                        .ConfigureAwait(false);
                }
                catch (MsalException ex)
                {
                    if (ex.ErrorCode == "access_denied")
                    {
                        // The user canceled sign in, take no action.
                    }
                    else
                    {
                        // An unexpected error occurred.
                        string message = ex.Message;
                        if (ex.InnerException != null)
                        {
                            message += "Error Code: " + ex.ErrorCode + "Inner Exception : " + ex.InnerException.Message;
                        }

                        Trace.Write(message);
                    }
                }
            }

            if (authResult == null || string.IsNullOrEmpty(authResult.AccessToken) || string.IsNullOrEmpty(authResult.TenantId))
            {
                Trace.TraceError("Authentication result, token or tenant id returned are null");
                throw new InvalidOperationException("Authentication result, token or tenant id returned are null");
            }

            token = authResult.AccessToken;
            tenantID = authResult.TenantId;
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