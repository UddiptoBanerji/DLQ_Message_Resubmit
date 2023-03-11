namespace DLQ_Message_Resubmit.Configuration
{
    public class ServiceBusConfiguration
    {
        public string ConnectionString { get; set; }
        public string TopicName { get; set; }
        public string SubscriptionName { get; set; }
    }
}
