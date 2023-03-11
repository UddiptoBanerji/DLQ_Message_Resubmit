using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Azure.Messaging.ServiceBus;
using DLQ2Topic;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus.Administration;
using DLQ_Message_Resubmit.Configuration;
using Microsoft.Extensions.Options;

namespace DLQ_Message_Resubmit
{
    public class DLQ2Topic
    {
        private ServiceBusConfiguration _serviceBusConfiguration;

        public DLQ2Topic(IOptions<ServiceBusConfiguration> serviceBusConfiguration)
        {
            _serviceBusConfiguration = serviceBusConfiguration.Value;
        }

        [FunctionName("DLQ2Topic")]
        public async Task Run([TimerTrigger("0 */2 * * * *")] TimerInfo myTimer, ILogger log)
        {
            await DLQToTopic(log);
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }

        public async Task DLQToTopic(ILogger log)
        {
            ServiceBusClient client;
            ServiceBusAdministrationClient mgmtClient;
            
            string connectionString = _serviceBusConfiguration.ConnectionString;
            string topicName = _serviceBusConfiguration.TopicName;
            string subscription = _serviceBusConfiguration.SubscriptionName;
            client = new ServiceBusClient(connectionString);
            mgmtClient = new ServiceBusAdministrationClient(connectionString);
            var count = await GetCounterMessages(mgmtClient, topicName, subscription);

            //
            // Comment and uncomment below as needed
            //

            // Use this to move Dead-Letter Queue messages back to the topic
            var dlq = new SendDlqMessagesToTopic(client, topicName, subscription, log);
            await dlq.RedirectDLQMessageToTopic(topicName, count);

            //Use this to send (messagesToSend) test messages to the topic

            //int messagesToSend = 5;
            //var sender = new MessageSender(client, topicName);
            //sender.Send(messagesToSend).Wait();

        }
        public async Task<long> GetCounterMessages(ServiceBusAdministrationClient client, String topicName,string subscription)
        {
            var properties = await client.GetSubscriptionRuntimePropertiesAsync(topicName, subscription);
            var count = properties.Value.DeadLetterMessageCount;
            
            return count;
        }
    }
}
