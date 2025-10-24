using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.LearnerData.Application;
using SFA.DAS.LearnerData.Application.NServiceBus;
using SFA.DAS.LearnerData.Functions.RaiseEventsForExistingLearners;

[assembly: NServiceBusTriggerFunction(AzureFunctionsQueueNames.RaiseEventsForExistingLearnersQueue)]

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration(builder => builder.BuildDasConfiguration())
    .ConfigureNServiceBus(AzureFunctionsQueueNames.RaiseEventsForExistingLearnersQueue)
    .ConfigureServices((context, services) =>
    {
        services.AddLearnerDataServices(context.Configuration);
        
        services
            .AddApplicationInsightsTelemetryWorkerService()
            .ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();
