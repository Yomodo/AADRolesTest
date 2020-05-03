using Microsoft.Azure.Management.Graph.RBAC.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Extensions.Configuration;
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
        private IAuthenticated _azure;

        public AzureServiceManagement()
        {
            // Using appsettings.local.json as our configuration settings
            ARMConfig config = new ARMConfig(new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.local.json")
                            .Build());

            var credentials = SdkContext.AzureCredentialsFactory
                .FromServicePrincipal(config.ClientId,
                config.ClientSecret,
                config.TenantId,
                AzureEnvironment.AzureGlobalCloud);

            _azure = Microsoft.Azure.Management.Fluent.Azure
                .Authenticate(credentials);
        }

        public IEnumerable<ISubscription> GetAllsubscriptionsForServicePrincipal()
        {
            return _azure.Subscriptions.List();
        }

        public IEnumerable<ITenant> GetAllTenantsForServicePrincipal()
        {
            return _azure.Tenants.List();
        }

        public IEnumerable<IServicePrincipal> GetAllServicePrincipalForServicePrincipal()
        {
            return _azure.ServicePrincipals.List();
        }

        public IEnumerable<IRoleAssignment> GetAllRoleAssignmentsForServicePrincipal()
        {
            return _azure.RoleAssignments.ListByScope("/");
        }

        public IEnumerable<ITenant> GetAllTenantsForUser()
        {
            ARMConfig config = new ARMConfig(new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddEnvironmentVariables()
               .AddJsonFile("appsettings.local.json")
                           .Build());

            var credentials = SdkContext.AzureCredentialsFactory
                .FromDevice(config.ClientId,
                config.TenantId,
                AzureEnvironment.AzureGlobalCloud,
                DoDeviceCodeFlow
                );

            var azure = Microsoft.Azure.Management.Fluent.Azure
                .Authenticate(credentials);

            return azure.Tenants.List();
        }

        public async Task<IEnumerable<ITenant>> GetAllTenantsForUserUsingMsalAsync()
        {
            ARMConfig config = new ARMConfig(new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddEnvironmentVariables()
               .AddJsonFile("appsettings.local.json")
                           .Build());

            string ArmToken = await new ArmCredentials().AuthenticateUserUsingMsalAsync();
            string GraphToken = await new MSGraphCredentials().AuthenticateUserUsingMsalAsync();

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

            return tenants;
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