using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFA.DAS.Configuration.AzureTableStorage;

namespace SFA.DAS.LearnerData.Application;

public static class ConfigurationExtensions
{
    const string EncodingConfigKey = "SFA.DAS.Encoding";

    public static IConfiguration BuildDasConfiguration(this IConfigurationBuilder configBuilder)
    {
        configBuilder
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddEnvironmentVariables();

        configBuilder.AddJsonFile("local.settings.json", optional: true);

        var config = configBuilder.Build();

        configBuilder.AddAzureTableStorage(options =>
        {
#if DEBUG
            options.ConfigurationKeys = config["Values:ConfigNames"].Split(",");
            options.StorageConnectionString = config["Values:ConfigurationStorageConnectionString"];
            options.EnvironmentName = config["Values:EnvironmentName"];
#else
            options.ConfigurationKeys = config["ConfigNames"].Split(",");
            options.StorageConnectionString = config["ConfigurationStorageConnectionString"];
            options.EnvironmentName = config["EnvironmentName"];
#endif
            options.PreFixConfigurationKeys = false;
            options.ConfigurationKeysRawJsonResult = [EncodingConfigKey];
        });

        return configBuilder.Build();
    }
}