using SFA.DAS.Http.Configuration;
using SFA.DAS.LearnerData.Application.OuterApi;

namespace SFA.DAS.LearnerData.Application;

public class ApplicationSettings
{
    public LearnerDataJobsOuterApiConfiguration LearnerDataJobsOuterApi { get; set; } = null!;
}

public class LearnerDataJobsOuterApiConfiguration : IApimClientConfiguration
{
    public const string ConfigurationName = "LearnerDataJobsOuterApiConfiguration";
    public string ApiBaseUrl { get; set; } = null!;
    public string IdentifierUri { get; set; } = null!;
    public string SubscriptionKey { get; set; } = null!;
    public string ApiVersion { get; set; } = null!;
}

