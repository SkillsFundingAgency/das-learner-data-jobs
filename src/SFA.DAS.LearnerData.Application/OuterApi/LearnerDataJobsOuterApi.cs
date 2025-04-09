using Azure.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

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
        var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"providers/{message.UKPRN}/learners")
        {
            Content = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json")
        };
        _logger.LogTrace("Sending learner data to inner API");
        return _httpClient.SendAsync(requestMessage);
    }
}