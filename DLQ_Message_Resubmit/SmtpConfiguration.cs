using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLQ_Message_Resubmit
{
    public class SmtpConfiguration
    {
        public string ApiKey { get; set; }

        public int Port { get; set; }

        public string ServerAddress { get; set; }

        public string UserName { get; set; }
        
    }
}
