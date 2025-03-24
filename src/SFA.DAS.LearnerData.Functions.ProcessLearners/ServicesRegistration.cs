using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using RestEase.HttpClientFactory;
using SFA.DAS.Http.Configuration;
using SFA.DAS.LearnerData.Application;
using SFA.DAS.LearnerData.Application.OuterApi;

namespace SFA.DAS.LearnerData.Functions.ProcessLearners;

public class ServicesRegistration(IServiceCollection services, IConfiguration configuration)
{
    public IServiceCollection Register()
    {
        services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), configuration));
        services.AddOptions();
        services.Configure<LearnerDataJobsOuterApiConfiguration>(configuration.GetSection(nameof(LearnerDataJobsOuterApiConfiguration)));
        services.AddSingleton(cfg => cfg.GetService<IOptions<LearnerDataJobsOuterApiConfiguration>>().Value);

        services.AddSingleton<IApimClientConfiguration>(x => x.GetRequiredService<LearnerDataJobsOuterApiConfiguration>());
        services.AddTransient<Http.MessageHandlers.DefaultHeadersHandler>();
        services.AddTransient<Http.MessageHandlers.LoggingMessageHandler>();
        services.AddTransient<Http.MessageHandlers.ApimHeadersHandler>();

        var url = services
            .BuildServiceProvider()
            .GetRequiredService<LearnerDataJobsOuterApiConfiguration>()
            .ApiBaseUrl;

        services.AddRestEaseClient<ILearnerDataJobsOuterApi>(url)
            .AddHttpMessageHandler<Http.MessageHandlers.DefaultHeadersHandler>()
            .AddHttpMessageHandler<Http.MessageHandlers.ApimHeadersHandler>()
            .AddHttpMessageHandler<Http.MessageHandlers.LoggingMessageHandler>();

        return services;
    }
}