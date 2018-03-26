// Copyright 2018 Louis S. Berman
//
// This file is part of LsbServerless.
//
// LsbServerless is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published 
// by the Free Software Foundation, either version 3 of the License, 
// or (at your option) any later version.
//
// LsbServerless is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with LsbServerless.  If not, see <http://www.gnu.org/licenses/>.

using Common;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NewEmployeeApp
{
    public static class NewEmployeeHandler
    {
        [FunctionName("NewEmployeeHandler")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequestMessage request,
            TraceWriter log)
        {
            log.Info("The NewEmployeeHandler was triggered");

            var eventType = request.Headers.GetValues("aeg-event-type").FirstOrDefault();

            if (eventType == "SubscriptionValidation")
            {
                var (gridEvents, response) = 
                    await request.GetGridEvents<Dictionary<string, string>>();

                if (response != null)
                    return response;

                var validationCode = gridEvents.First().Data["validationCode"];

                log.Info($"A \"SubscriptionValidation\" event was received and a \"{validationCode}\" validation code will be returned.");

                return request.CreateResponse(HttpStatusCode.OK, 
                    new { validationResponse = validationCode });
            }
            else if (eventType == "Notification")
            {
                var (gridEvents, response) = await request.GetGridEvents<EmployeeInfo>();

                if (response != null)
                    return response;

                log.Info($"A \"Notification\" event was received and deserialized into {gridEvents.GetType()} object(s).");

                try
                {
                    gridEvents.ForEach(async gridEvent => 
                        await ProcessGridEvent(gridEvent, log));

                    return request.CreateResponse(HttpStatusCode.Accepted,
                        $"{gridEvents.Count:N0} image(s) were fetched then saved to table storage for further processing by the Vision API.");
                }
                catch (Exception error)
                {
                    log.Error(error.Message);

                    return request.CreateErrorResponse(
                        HttpStatusCode.InternalServerError, error);
                }
            }
            else
            {
                return request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "An unexpected \"aeg-event-type\" value was received.");
            }
        }

        private static async Task ProcessGridEvent(
            GridEvent<EmployeeInfo> gridEvent, TraceWriter log)
        {
            const string QUEUENAME = "equipmentorders";

            var account = CloudStorageAccount.Parse(
                ConfigurationManager.AppSettings["AzureWebJobsStorage"]);

            var client = account.CreateCloudQueueClient();

            var queue = client.GetQueueReference(QUEUENAME);

            await queue.CreateIfNotExistsAsync();

            var json = JsonConvert.SerializeObject(gridEvent.Data);

            await queue.AddMessageAsync(new CloudQueueMessage(json));

            log.Info($"Posted {json} to the \"{QUEUENAME}\" queue.");
        }
    }
}