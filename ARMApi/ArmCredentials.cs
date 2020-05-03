using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.Rest;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace ARMApi
{
    public class ArmCredentials : ServiceClientCredentials
    {
        private string AuthenticationToken { get; set; }

        public override void InitializeServiceClient<T>(ServiceClient<T> client)
        {
            AuthenticationToken = AuthenticateUsingMsalAsync().Result;
        }

        public override async Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (AuthenticationToken == null)
            {
                throw new InvalidOperationException("Token Provider Cannot Be Null");
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AuthenticationToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //request.Version = new Version(apiVersion);
            await base.ProcessHttpRequestAsync(request, cancellationToken);
        }

        public async Task<string> AuthenticateUsingMsalAsync()
        {
            ARMConfig config = new ARMConfig(new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddEnvironmentVariables()
              .AddJsonFile("appsettings.local.json")
                          .Build());

            string[] ARMScope = new string[] { $"{config.ArmEndPoint}/.default" };

            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(config.ClientId)
                .WithAuthority(new System.Uri($"{config.Instance}{config.TenantId}"))
                .WithClientSecret(config.ClientSecret)
                .Build();

            //var ARMresult = AsyncHelper.RunSync<AuthenticationResult>(async () => await app.AcquireTokenForClient(ARMScope).ExecuteAsync());
            var ARMresult = await app.AcquireTokenForClient(ARMScope).ExecuteAsync();

            if (ARMresult == null)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token for ARM");
            }

            return ARMresult.AccessToken;
        }

        public async Task<string> AuthenticateUserUsingMsalAsync()
        {
            ARMConfig config = new ARMConfig(new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddEnvironmentVariables()
              .AddJsonFile("appsettings.local.json")
                          .Build());

            string[] ARMScope = new string[] { $"{config.ArmEndPoint}/.default" };

            IPublicClientApplication app = PublicClientApplicationBuilder.Create(config.ClientId)
                .WithAuthority(new System.Uri($"{config.Instance}{config.TenantId}"))
                .WithDefaultRedirectUri()
                .Build();


            var accounts = await app.GetAccountsAsync();
            AuthenticationResult ARMresult;

            try
            {
                ARMresult = await app.AcquireTokenSilent(ARMScope, accounts.FirstOrDefault())
                            .ExecuteAsync();
            }
            catch (MsalUiRequiredException)
            {
                ARMresult = await app.AcquireTokenInteractive(ARMScope)
                            .ExecuteAsync();
            }

            if (ARMresult == null)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token for ARM");
            }

            return ARMresult.AccessToken;
        }
    }
}