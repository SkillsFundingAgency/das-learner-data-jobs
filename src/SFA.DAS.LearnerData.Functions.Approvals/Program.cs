using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.LearnerData.Application;
using SFA.DAS.LearnerData.Application.NServiceBus;
using SFA.DAS.LearnerData.Functions.Approvals;

[assembly: NServiceBusTriggerFunction(AzureFunctionsQueueNames.ProcessLearnersQueue)]

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration(builder => builder.BuildDasConfiguration())
    .ConfigureNServiceBus(AzureFunctionsQueueNames.ApprovalsQueue)
    .ConfigureServices((context, services) =>
    {
        var servicesRegistration = new ServicesRegistration(services, context.Configuration);
        servicesRegistration.Register();

        services
            .AddApplicationInsightsTelemetryWorkerService()
            .ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();


var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Build().Run();