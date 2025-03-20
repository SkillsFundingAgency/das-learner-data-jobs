using SFA.DAS.Http.Configuration;

namespace SFA.DAS.LearnerData.Application;

public class ApplicationSettings
{
    public ApiOptions LearnerDataJobsOuterApi { get; set; } = null!;
}

public class ApiOptions : IApimClientConfiguration
{
    public const string LearnerDataJobsOuterApiConfiguration = "LearnerDataJobsOuterApiConfiguration";
    public string ApiBaseUrl { get; set; } = null!;
    public string IdentifierUri { get; set; } = null!;
    public string SubscriptionKey { get; set; } = null!;
    public string ApiVersion { get; set; } = null!;
}

