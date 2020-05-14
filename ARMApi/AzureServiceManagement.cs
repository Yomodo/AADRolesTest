using Microsoft.Azure.Management.Graph.RBAC.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.Azure.Management.Fluent.Azure;

namespace ARMApi
{
    public class AzureServiceManagement
    {
        private IAuthenticated _servicePrincipal;
        private IAuthenticated _User;
        private IAuthenticated _MSALUser;
        private ARMConfig _config = null;

        public AzureServiceManagement()
        {
            // Using appsettings.local.json as our configuration settings
            _config = new ARMConfig(new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.local.json")
                            .Build());

            var SPcredentials = SdkContext.AzureCredentialsFactory
                .FromServicePrincipal(_config.ClientId,
                _config.ClientSecret,
                _config.TenantId,
                AzureEnvironment.AzureGlobalCloud);

            _servicePrincipal = Microsoft.Azure.Management.Fluent.Azure
                .Authenticate(SPcredentials);

            var usercredentials = SdkContext.AzureCredentialsFactory
                .FromDevice(_config.ClientId, _config.TenantId, AzureEnvironment.AzureGlobalCloud, DoDevicecodeAuth);

            _User = Microsoft.Azure.Management.Fluent.Azure
                .Authenticate(usercredentials);

            // MSAL
            string ArmToken = new ArmCredentials().AuthenticateUserUsingMsalAsync().Result;
            string GraphToken = new MSGraphCredentials().AuthenticateUserUsingMsalAsync().Result;

            var azureUserCredentials = new AzureCredentials(
                        new TokenCredentials(ArmToken),
                        new TokenCredentials(GraphToken),
                        _config.TenantId,
                        AzureEnvironment.AzureGlobalCloud);

            var client = RestClient
                .Configure()
                .WithEnvironment(AzureEnvironment.AzureGlobalCloud)
                .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                .WithCredentials(azureUserCredentials)
                .Build();

            _MSALUser = Microsoft.Azure.Management.Fluent.Azure.Authenticate(azureUserCredentials);
        }

        private bool DoDevicecodeAuth(Microsoft.IdentityModel.Clients.ActiveDirectory.DeviceCodeResult deviceCodeResult)
        {
            Console.WriteLine(deviceCodeResult.Message);
            return true;
        }

        public async Task<IEnumerable<ISubscription>> GetAllSubscriptionsForServicePrincipalAsync()
        {
            return await _servicePrincipal.Subscriptions.ListAsync();
        }

        public async Task<IEnumerable<ITenant>> GetAllTenantsForServicePrincipalAsync()
        {
            return await _servicePrincipal.Tenants.ListAsync();
        }

        public async Task<IEnumerable<IServicePrincipal>> GetAllServicePrincipalsForServicePrincipalAsync()
        {
            return await _servicePrincipal.ServicePrincipals.ListAsync();
        }

        public async Task<IEnumerable<IRoleAssignment>> GetAllRoleAssignmentsForServicePrincipalAsync()
        {
            return await _servicePrincipal.RoleAssignments.ListByScopeAsync($"provider");
        }

        public async Task<IEnumerable<ITenant>> GetAllTenantsForUserAsync()
        {
            return await _User.Tenants.ListAsync();
        }

        public async Task<IEnumerable<ISubscription>> GetAllSubscriptionsForUserAsync()
        {
            return await _User.Subscriptions.ListAsync();
        }

        public async Task<IEnumerable<ITenant>> GetAllTenantsForUserUsingMsalAsync()
        {
            return await _MSALUser.Tenants.ListAsync();
        }

        private bool DoDeviceCodeFlow(Microsoft.IdentityModel.Clients.ActiveDirectory.DeviceCodeResult arg)
        {
            Console.WriteLine(arg.Message);
            return true;
        }

        public void PrintSubscriptionsUsingMsal()
        {
            // does not work
            ARMConfig config = new ARMConfig(new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddEnvironmentVariables()
               .AddJsonFile("appsettings.local.json")
                           .Build());

            ArmCredentials armCredentials = new ArmCredentials();

            var azureCredentials = new AzureCredentials(
                new ArmCredentials(),
                new MSGraphCredentials(),
                config.TenantId,
                AzureEnvironment.AzureGlobalCloud);

            var client = RestClient
                .Configure()
                .WithEnvironment(AzureEnvironment.AzureGlobalCloud)
                .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                .WithCredentials(azureCredentials)
                .Build();

            var azure = Microsoft.Azure.Management.Fluent.Azure
            .Authenticate(client, config.TenantId)
            .WithDefaultSubscription();

            var subscriptions = azure.Subscriptions.List();
            subscriptions.ToList().ForEach(sub => Console.WriteLine(sub.DisplayName));

            var appsecuritygroups = azure.ApplicationSecurityGroups.List();
            appsecuritygroups.ToList().ForEach(sub => Console.WriteLine(sub.Name));
        }

        public async Task<IEnumerable<ISubscription>> GetAllsubscriptionsForServicePrincipalUsingMsalAsync()
        {
            ARMConfig config = new ARMConfig(new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddEnvironmentVariables()
               .AddJsonFile("appsettings.local.json")
                           .Build());

            string ArmToken = await new ArmCredentials().AuthenticateUsingMsalAsync();
            string GraphToken = await new MSGraphCredentials().AuthenticateUsingMsalAsync();

            var azureCredentials = new AzureCredentials(
                        new TokenCredentials(ArmToken),
                        new TokenCredentials(GraphToken),
                        config.TenantId,
                        AzureEnvironment.AzureGlobalCloud);

            var client = RestClient
                .Configure()
                .WithEnvironment(AzureEnvironment.AzureGlobalCloud)
                .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                .WithCredentials(azureCredentials)
                .Build();

            var azure = Microsoft.Azure.Management.Fluent.Azure.Authenticate(client, config.TenantId);

            var tenants = azure.Tenants.List();
            tenants.ToList().ForEach(sub => Console.WriteLine(sub.TenantId));

            var subscriptions = azure.Subscriptions.List();
            subscriptions.ToList().ForEach(sub => Console.WriteLine(sub.DisplayName));

            return subscriptions;
        }
    }
}