using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.LearnerData.Application.OuterApi;

namespace SFA.DAS.LearnerData.Functions.RaiseEventsForExistingLearners;

public class ServicesRegistration
{
    private readonly IServiceCollection _services;
    private readonly IConfiguration _configuration;

    public ServicesRegistration(IServiceCollection services, IConfiguration configuration)
    {
        _services = services;
        _configuration = configuration;
    }

    public void Register()
    {
        _services.AddHttpClient();
        _services.AddSingleton<ILearnerDataJobsOuterApi, LearnerDataJobsOuterApi>();
    }
}
