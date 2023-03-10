using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using DLQ_Message_Resubmit;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
 

namespace DLQ2Topic
{
    public class SendDlqMessagesToTopic
    {
        ServiceBusClient _client;
        ServiceBusReceiver _receiver;
        ServiceBusSender _sender;
        ILogger _logger;
        private readonly SmtpConfiguration _smtpConfiguration;
        private const string BhNoReplyAddress = "Uddipto.Banerji@brighthorizons.com";
        private const string MailToAddress = "uddipto.banerji@accenture.com";

        public SendDlqMessagesToTopic(ServiceBusClient client, string topicName, string subscriptionName, ILogger log)
        {
            _client = client;
            
            _receiver = _client.CreateReceiver(topicName, subscriptionName, new ServiceBusReceiverOptions()
            {
                ReceiveMode = ServiceBusReceiveMode.PeekLock,
                SubQueue = SubQueue.DeadLetter
            });
            
            _sender = _client.CreateSender(topicName);
            _logger= log;
        }

        public async Task RedirectDLQMessageToTopic(string topicName, long count)
        {
            int _messageCount = 0;
            // Limit the message receiving to be 10.
            IReadOnlyList<ServiceBusReceivedMessage> dlqMessages = await _receiver.ReceiveMessagesAsync(10);
            _logger.LogInformation("Total DLQ Messages Count: " + dlqMessages.ToList().Count());
            if (dlqMessages.ToList().Count > 0)
                SendConfig(count);
            try
            {
                foreach (var dlqMessage in dlqMessages)
                {
                    var repairedMessage = new ServiceBusMessage(dlqMessage)
                    {
                        TimeToLive = new TimeSpan(1, 0, 0) // Time to live the message.
                    };

                    await _sender.SendMessageAsync(repairedMessage);
                    await _receiver.CompleteMessageAsync(dlqMessage);
                    await UploadDLQMessageToBlob(dlqMessage);
                    _messageCount++;
                }
            }
            catch (Exception ex) { throw ex; }
            _logger.LogInformation("DLQ Messages successfully proccessed: " + _messageCount);
        }

        public async Task UploadDLQMessageToBlob(ServiceBusReceivedMessage dlqMessage)
        {
            // Get a connection string to our Azure Storage account.  You can
            // obtain your connection string from the Azure Portal (click
            // Access Keys under Settings in the Portal Storage account blade)
            // or using the Azure CLI with:
            //
            //     az storage account show-connection-string --name <account_name> --resource-group <resource_group>
            //
            // And you can provide the connection string to your application
            // using an environment variable.

            string connectionString = "DefaultEndpointsProtocol=https;AccountName=dlqblob;AccountKey=M5+e+E2yd6WX5/LvZ0evysvzhhN6pcrxQLmhz0pARaiUz6HFooOGLx59oOPHLiC6vbZcNBUQi2rl+AStUtmAtA==;EndpointSuffix=core.windows.net";
            string containerName = "dlqmessage";
            string blobName = dlqMessage.MessageId;
            
            try
            {
                BlobContainerClient container = new BlobContainerClient(connectionString, containerName);
               // Get a reference to a blob named 
                BlobClient blob = container.GetBlobClient(blobName);
                DLQbMessageContent dlqmc = new DLQbMessageContent
                {
                    MessageId = dlqMessage.MessageId,
                    DeadLetterErrorDescription = dlqMessage.DeadLetterErrorDescription,
                    DeadLetterReason = dlqMessage.DeadLetterReason,
                    LockedUntil = dlqMessage.LockedUntil,
                    LockToken = dlqMessage.LockToken,
                    TimeToLive = dlqMessage.TimeToLive,
                    State = dlqMessage.State.ToString(),
                    EnqueuedTime = dlqMessage.EnqueuedTime,
                    Body = Encoding.ASCII.GetString(dlqMessage.Body),
                    ExpiresAt = dlqMessage.ExpiresAt
                };
                var dqlmsgcontent = new BinaryData(dlqmc);                 
                // Upload local file
                await blob.UploadAsync(dqlmsgcontent);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SendConfig(long dlqcount)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(BhNoReplyAddress),
                Subject = "LeadsId In DLQ",
                Body = "Number of Lead in DLQ are " + dlqcount.ToString(),                
                IsBodyHtml = true
            };
            mailMessage.To.Add(MailToAddress);
            SendEmail(mailMessage);
        }

        private void SendEmail(MailMessage message)
        {
            try
            {
                using var smtpClient = new SmtpClient(_smtpConfiguration.ServerAddress)
                {
                    Port = _smtpConfiguration.Port,
                    Credentials = new NetworkCredential(_smtpConfiguration.UserName, _smtpConfiguration.ApiKey),
                    EnableSsl = false,
                };
                //using var smtpclient = new smtpclient("smtp.sparkpostmail.com")//_smtpconfiguration.serveraddress)
                //{
                //    port = 587,
                //    credentials = new networkcredential("smtp_injection", "0b54f6c26aa64fa837f53b6c572cb7792a8c3560"),
                //    enablessl = false,
                //};

                smtpClient.Send(message);
                message.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Error in sending the email : " + ex.StackTrace.ToString());
                
            }
        }
    }
}
