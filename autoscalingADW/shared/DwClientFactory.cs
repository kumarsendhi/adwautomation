using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

using Newtonsoft.Json;
using System.Net.Http;
using System.Globalization;
using Microsoft.Azure;

namespace autoscalingADW.shared
{
    public class DwClientFactory
    {
           public static string ActiveDirectoryEndpoint { get; set; } = "https://login.windows.net/";
        public static string ResourceManagerEndpoint { get; set; } = "https://management.azure.com/";
        public static string WindowsManagementUri { get; set; } = "https://management.core.windows.net/";

        //Leave the following Ids and keys unassigned so that they won't be checked in Git. They are assigned in Azure portal.
        public static string SubscriptionId { get; set; } 
        public static string TenantId { get; set; } 
        public static string ClientId { get; set; } 
        public static string ClientKey { get; set; } 

        public static DwManagementClient Create(string resourceId, ExecutionContext context)
        {
            var config = new ConfigurationBuilder()
            .SetBasePath(context.FunctionAppDirectory)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

            SubscriptionId = config["SubscriptionId"];
            TenantId = config["TenantId"];
            ClientId = config["ClientId"];
            ClientKey = config["ClientKey"];

            var httpClient = new HttpClient();
            var authenticationContext = new AuthenticationContext(ActiveDirectoryEndpoint + TenantId);

            
            var credential = new ClientCredential(clientId: ClientId, clientSecret: ClientKey);
            var result = authenticationContext.AcquireTokenAsync(resource: WindowsManagementUri, clientCredential: credential).Result;

            if (result == null) throw new InvalidOperationException("Failed to obtain the token!");

            var token = result.AccessToken;

            var aadTokenCredentials = new TokenCloudCredentials(SubscriptionId, token);

            var client = new DwManagementClient(aadTokenCredentials, resourceId);
            return client;
        }
    }
}
