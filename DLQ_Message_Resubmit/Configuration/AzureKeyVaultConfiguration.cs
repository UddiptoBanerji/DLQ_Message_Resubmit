using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLQ_Message_Resubmit.Configuration
{
    public class AzureKeyVaultConfiguration
    {
        public string KeyVaultName { get; set; }
        public string TenentId { get; set; }
        public Uri KeyVaultUri => new Uri($"https://{KeyVaultName}.vault.azure.net");
    }
}
