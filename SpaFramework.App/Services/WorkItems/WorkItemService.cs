using Azure.Storage.Queues;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpaFramework.App.DAL;
using SpaFramework.App.Exceptions;
using SpaFramework.App.Utilities;
using SpaFramework.App.Models.Data.Accounts;
using SpaFramework.App.Models.Service.WorkItems;
using SpaFramework.App.Services.WorkItems;
using Newtonsoft.Json;
using NodaTime.Serialization.JsonNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SpaFramework.App.Services.WorkItems
{
    public class WorkItemService<TWorkItem> : IWorkItemService<TWorkItem>
        where TWorkItem : IWorkItem
    {
        protected readonly IConfiguration _configuration;
        protected readonly ILogger<WorkItemService<TWorkItem>> _logger;

        public WorkItemService(IConfiguration configuration, ILogger<WorkItemService<TWorkItem>> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private async Task<QueueClient> CreateQueueIfNotExists()
        {
            var connectionString = _configuration.GetValue<string>("AzureWebJobsStorage");
            var queueClient = new QueueClient(connectionString, typeof(TWorkItem).Name.ToLower());
            await queueClient.CreateIfNotExistsAsync();

            return queueClient;
        }

        public async Task EnqueueWorkItem(WorkItemMessage<TWorkItem> workItemMessage)
        {
            var queueClient = await CreateQueueIfNotExists();

            ++workItemMessage.Attempts;

            if (!workItemMessage.CanRequeue)
                throw new RequeueLimitException("Cannot requeue message");

            string serializedValue = JsonConvert.SerializeObject(workItemMessage, Formatting.None, SerializationUtilities.GetJsonDefaultSerializerSettings());

            // We're using the Azure.Storage.Queues package here, which replaces Microsoft.Azure.Storage.Blob package. The deprecated package would automatically base64 encode the message; this package does not. Web Jobs only seem to work with base64-encoded messages, so we have to encode it here.
            // https://github.com/Azure/azure-sdk-for-net/issues/10242
            await queueClient.SendMessageAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(serializedValue)), visibilityTimeout: workItemMessage.GetInitialVisibilityDelay(), timeToLive: workItemMessage.TimeToLive);

            _logger.LogInformation("WorkItem {WorkItemType} {EntityAction}: {Name}", typeof(TWorkItem).Name, "Enqueued", workItemMessage.WorkItem.LoggableName);
        }

        public async Task EnqueueWorkItem(TWorkItem workItem, int? initialDelaySeconds = null, int? requeueDelaySeconds = null, TimeSpan? timeToLive = null)
        {
            WorkItemMessage<TWorkItem> workItemMessage = new WorkItemMessage<TWorkItem>()
            {
                WorkItem = workItem,
                Attempts = -1,
                InitialDelaySeconds = initialDelaySeconds,
                RequeueDelaySeconds = requeueDelaySeconds,
                TimeToLive = timeToLive ?? TimeSpan.FromDays(7)
            };

            await EnqueueWorkItem(workItemMessage);
        }
    }
}
