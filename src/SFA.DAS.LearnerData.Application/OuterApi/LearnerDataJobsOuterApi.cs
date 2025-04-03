using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SFA.DAS.LearnerData.Application.OuterApi;

public class LearnerDataJobsOuterApi : ILearnerDataJobsOuterApi
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LearnerDataJobsOuterApi> _logger;

    public LearnerDataJobsOuterApi(
        HttpClient httpClient,
        IOptions<LearnerDataJobsOuterApiConfiguration> configuration,
        ILogger<LearnerDataJobsOuterApi> logger)
    {
        _httpClient = httpClient;
        var config = configuration.Value;
        httpClient.BaseAddress = new Uri(config.ApiBaseUrl);
        _logger = logger;
    }

    public Task AddOrUpdateLearner(LearnerDataRequest message)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Put, "learners");
        _logger.LogTrace("Sending learner data to inner API");
        return _httpClient.SendAsync(requestMessage);
    }
}