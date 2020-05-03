using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace ARMApi
{
    public class ARMConfig
    {
        private readonly IConfiguration _config;

        public ARMConfig(IConfiguration config)
        {
            _config = config;
        }

        public string SubscriptionId
        {
            get { return _config["SubscriptionId"]; }
        }

        public string GraphApiUrl
        {
            get { return _config["GraphApiUrl"]; }
        }

        public string AccountName
        {
            get { return _config["AccountName"]; }
        }

        public string TenantId
        {
            get { return _config["TenantId"]; }
        }

        public string ClientId
        {
            get { return _config["ClientId"]; }
        }

        public string ClientSecret
        {
            get { return _config["ClientSecret"]; }
        }

        public Uri ArmAadAudience
        {
            get { return new Uri(_config["ArmAadAudience"]); }
        }

        public Uri Instance
        {
            get { return new Uri(_config["Instance"]); }
        }

        public Uri ArmEndPoint
        {
            get { return new Uri(_config["ArmEndPoint"]); }
        }

        public string RedirectUri
        {
            get { return _config["RedirectUri"]; }
        }
    }

}
