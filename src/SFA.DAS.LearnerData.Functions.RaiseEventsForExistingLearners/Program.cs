using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SFA.DAS.LearnerData.Application.OuterApi;
using SFA.DAS.LearnerData.Functions.RaiseEventsForExistingLearners;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var servicesRegistration = new ServicesRegistration(services, context.Configuration);
        servicesRegistration.Register();
    })
    .Build();

var logger = host.Services.GetRequiredService<ILogger<RaiseEventsForExistingLearnersJob>>();
var outerApiService = host.Services.GetRequiredService<ILearnerDataJobsOuterApi>();

logger.LogInformation("Starting one-off job to raise events for existing learners with null ApprenticeshipId");

try
{
    var job = new RaiseEventsForExistingLearnersJob(outerApiService, logger);
    await job.ExecuteAsync();
    
    logger.LogInformation("One-off job completed successfully");
}
catch (Exception ex)
{
    logger.LogError(ex, "One-off job failed with error: {ErrorMessage}", ex.Message);
    throw;
}
