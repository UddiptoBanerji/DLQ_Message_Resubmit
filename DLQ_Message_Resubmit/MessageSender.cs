using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DLQ2Topic
{
    public class MessageSender
    {
        ServiceBusClient _client { get; set; }
        readonly string _topic;

        public MessageSender(ServiceBusClient client, string topicName) 
        {
            _client = client;
            _topic = topicName;
        }

        public async Task Send(int count)
        {
            var sender = _client.CreateSender(_topic);
            var messages = new List<ServiceBusMessage>();

            for (int i = 0; i < count; i++) 
            {
                var message = new ServiceBusMessage($"Message {i}");
                message.TimeToLive = new TimeSpan(0, 1, 0);
                messages.Add(message);
            }

            await sender.SendMessagesAsync(messages);
            await sender.CloseAsync();
        }
    }
}
