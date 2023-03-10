using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Azure.Messaging.ServiceBus;
using DLQ2Topic;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus.Administration;

namespace DLQ_Message_Resubmit
{
    public class DLQ2Topic
    {
        [FunctionName("DLQ2Topic")]
        public void Run([TimerTrigger("0 */2 * * * *")] TimerInfo myTimer, ILogger log)
        {
            DLQToTopic(log);
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }

        public async Task DLQToTopic(ILogger log)
        {
            ServiceBusClient client;
            ServiceBusAdministrationClient mgmtClient;
            
            string connectionString = "Endpoint=sb://asb-dev-test.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=Eim+waSQdvqq4+Trc1ygJegBJ34cJ5e+d2zddXg5W0c=";
            string topicName = "LeadsCreate";
            string subscription = "bhprodleadhydratorcreate";
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
