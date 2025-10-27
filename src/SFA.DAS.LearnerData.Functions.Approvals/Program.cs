using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SFA.DAS.LearnerData.Application;
using SFA.DAS.LearnerData.Application.NServiceBus;
using SFA.DAS.LearnerData.Application.StartupExtensions;
using SFA.DAS.LearnerData.Functions.Approvals;

[assembly: NServiceBusTriggerFunction(AzureFunctionsQueueNames.ApprovalsQueue)]

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration(builder => builder.BuildDasConfiguration())
    .ConfigureNServiceBus(AzureFunctionsQueueNames.ApprovalsQueue)
    .ConfigureServices((context, services) =>
    {
        services.AddLearnerDataServices(context.Configuration);
        
        services.AddDasLogging();

        services
            .AddApplicationInsightsTelemetryWorkerService()
            .ConfigureFunctionsApplicationInsights();
    })
    .ConfigureLogging((context, logging) =>
    {
        logging.AddApplicationInsights();
        logging.SetMinimumLevel(LogLevel.Debug);
        }
     )    
    .Build();

host.Run();

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Build().Run();