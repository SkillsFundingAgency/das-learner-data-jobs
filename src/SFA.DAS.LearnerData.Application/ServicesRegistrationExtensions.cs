using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using SFA.DAS.Http.Configuration;
using SFA.DAS.LearnerData.Application.OuterApi;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.LearnerData.Application;

[ExcludeFromCodeCoverage]
public static class ServicesRegistrationExtensions
{
    public static IServiceCollection AddLearnerDataServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), configuration));
        services.AddOptions();
        services.Configure<LearnerDataJobsOuterApiConfiguration>(configuration.GetSection(nameof(LearnerDataJobsOuterApiConfiguration)));
        services.AddSingleton(cfg => cfg.GetService<IOptions<LearnerDataJobsOuterApiConfiguration>>().Value);

        services.AddSingleton<IApimClientConfiguration>(x => x.GetRequiredService<LearnerDataJobsOuterApiConfiguration>());
        services.AddTransient<Http.MessageHandlers.DefaultHeadersHandler>();
        services.AddTransient<Http.MessageHandlers.LoggingMessageHandler>();
        services.AddTransient<Http.MessageHandlers.ApimHeadersHandler>();

        services.AddHttpClient<ILearnerDataJobsOuterApi, LearnerDataJobsOuterApi>() 
            .AddHttpMessageHandler<Http.MessageHandlers.DefaultHeadersHandler>()
            .AddHttpMessageHandler<Http.MessageHandlers.ApimHeadersHandler>()
            .AddHttpMessageHandler<Http.MessageHandlers.LoggingMessageHandler>();

        return services;
    }
}