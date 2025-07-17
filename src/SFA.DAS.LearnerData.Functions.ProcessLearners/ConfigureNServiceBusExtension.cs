using System.Net;
using Microsoft.Extensions.Hosting;
using SFA.DAS.LearnerData.Application.NServiceBus;

namespace SFA.DAS.LearnerData.Functions.ProcessLearners;

public static class ConfigureNServiceBusExtension
{
    public static IHostBuilder ConfigureNServiceBus(this IHostBuilder hostBuilder, string endpointName)
    {
        hostBuilder.UseNServiceBus((config, endpointConfiguration) =>
        {
            endpointConfiguration.Transport.SubscriptionRuleNamingConvention = AzureRuleNameShortener.Shorten;
            endpointConfiguration.AdvancedConfiguration.EnableInstallers();
            endpointConfiguration.AdvancedConfiguration.SendFailedMessagesTo($"{endpointName}-error");
            endpointConfiguration.AdvancedConfiguration.UseMessageConventions();
            endpointConfiguration.AdvancedConfiguration.UseNewtonsoftJsonSerializer();

            var decodedLicence = WebUtility.HtmlDecode(config["NServiceBusLicense"]);
            if (!string.IsNullOrWhiteSpace(decodedLicence)) endpointConfiguration.AdvancedConfiguration.License(decodedLicence);
        });

        return hostBuilder;
    }
}