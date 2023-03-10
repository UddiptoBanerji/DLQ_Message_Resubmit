using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLQ_Message_Resubmit
{
    public class DLQbMessageContent
    {
        public string MessageId { get; set; }
        public string DeadLetterErrorDescription { get; set; }
        public string DeadLetterReason { get; set; }
        public DateTimeOffset? EnqueuedTime { get; set; }
        public DateTimeOffset? ExpiresAt { get; set; }
        public string LockToken { get; set; }
        public DateTimeOffset? LockedUntil { get; set; }
        public TimeSpan TimeToLive { get; set; }
        public string State { get; set; }        
        public string Body { get; set; }



    }
}
