using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Xml.Linq;
using DLQ_Message_Resubmit.Configuration;
using System.ComponentModel;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using Microsoft.Extensions.Azure;

[assembly: FunctionsStartup(typeof(DLQ_Message_Resubmit.Startup))]

namespace DLQ_Message_Resubmit
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddOptions<ServiceBusConfiguration>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection(nameof(ServiceBusConfiguration)).Bind(settings);
                });
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            var config = builder.ConfigurationBuilder.Build();
            var kvConfig = new AzureKeyVaultConfiguration();
            config.GetSection(nameof(AzureKeyVaultConfiguration)).Bind(kvConfig);

            var credential = new DefaultAzureCredential();
            builder.ConfigurationBuilder.AddAzureKeyVault(kvConfig.KeyVaultUri, credential);
        }
    }
}
