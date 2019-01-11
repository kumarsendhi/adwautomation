using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using autoscalingADW.shared;
using System.Net;

namespace autoscalingADW
{
    public static class getadwinfo
    {
        [FunctionName("scaleup")]
        public static async Task<IActionResult> getadwinformation(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getadwinformation")] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            log.LogInformation("autoscaleup of adw initiated");

            var config = new ConfigurationBuilder()
            .SetBasePath(context.FunctionAppDirectory)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

            try
            {
                var dwLocation = config["SqlDwLocation"];
                //var tableName = config["DwScaleLogsTable"];
                var dwuConfigFile = config["DwuConfigFile"];
                var resourceId = config["ResourceId"];
                //string startupPath = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
                //string dwuConfigFilePath = startupPath + "\\" + dwuConfigFile;
                //var dwuConfigManager = new DwuConfigManager(dwuConfigFilePath);

                // Create a DataWarehouseManagementClient
                var dwClient = DwClientFactory.Create(resourceId.ToString(),context);
                // Get database information
                var dbInfo = dwClient.GetDatabase();
                dynamic dbInfoObject = JsonConvert.DeserializeObject(dbInfo);
                var currentDwu = dbInfoObject.properties.requestedServiceObjectiveName.ToString();
                //logEntity.DwuBefore = currentDwu;
                log.LogInformation($"Current DWU is {currentDwu}");

                
                return new OkObjectResult(dbInfoObject);

            }
            catch (Exception ex)
            {
                return new NotFoundResult();
            }

            
            
        }
    }
}
