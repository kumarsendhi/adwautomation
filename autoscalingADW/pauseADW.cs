using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using autoscalingADW.shared;
using System.Net;
using System.Net.Http;

namespace autoscalingADW
{
    public static class pauseADW
    {
        [FunctionName("pauseADW")]
        public static async Task<HttpResponseMessage> pause(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get",  Route = "pauseADW")] HttpRequestMessage req,
            ILogger log, ExecutionContext context)
        {
            log.LogInformation("pause the resumed ADW");

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
                string startupPath = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
                string dwuConfigFilePath = startupPath + "\\" + dwuConfigFile;
                var dwuConfigManager = new DwuConfigManager(dwuConfigFilePath);

                // Create a DataWarehouseManagementClient
                var dwClient = DwClientFactory.Create(resourceId.ToString(), context);
                // Get database information
                var dbInfo = dwClient.GetDatabase();
                dynamic dbInfoObject = JsonConvert.DeserializeObject(dbInfo);
                var currentStatus = dbInfoObject.properties.status.ToString();
                //logEntity.DwuBefore = currentDwu;
                log.LogInformation($"Current Status is {currentStatus}");

                if (currentStatus != "Online")
                {
                    return req.CreateResponse(HttpStatusCode.BadRequest, "Bad Operation");
                }

                HttpResponseMessage res = dwClient.Pause();
                if (res.StatusCode != HttpStatusCode.Accepted)
                {
                    return req.CreateResponse(HttpStatusCode.InternalServerError, "internal error");
                }
                return req.CreateResponse(HttpStatusCode.Accepted, "ADW paused");


            }
            catch (Exception ex)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "Bad Operation");
            }
        }
    }
}
