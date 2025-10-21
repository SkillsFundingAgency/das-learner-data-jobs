using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.LearnerData.Application.Models;
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

    public async Task AddOrUpdateLearner(LearnerDataRequest message)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"providers/{message.UKPRN}/learners")
        {
            Content = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json")
        };
        _logger.LogTrace("Sending learner data to inner API");
        var response = await _httpClient.SendAsync(requestMessage);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Unsuccessful status code returned from API {0}", response.StatusCode);
            throw new HttpRequestException("Unsuccessful status code returned when adding or updating learner data", null, response.StatusCode);
        }
    }

    public async Task<GetLearnerDataResponse> GetLearnerById(long providerId, long learnerDataId)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"providers/{providerId}/learners/{learnerDataId}");
        
        _logger.LogTrace("Getting learner data from inner API");
        var response = await _httpClient.SendAsync(requestMessage);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Unsuccessful status code returned from API {0}", response.StatusCode);
            throw new HttpRequestException("Unsuccessful status code returned when adding or updating learner data", null, response.StatusCode);
        }
        
        string json = await response.Content.ReadAsStringAsync();
        var learner = JsonConvert.DeserializeObject<GetLearnerDataResponse>(json);

        return learner??new GetLearnerDataResponse();
    }

    public async Task PatchApprenticeshipId(long providerId, long learnerDataId, PatchLearnerDataApprenticeshipIdRequest message)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Patch, $"providers/{providerId}/learners/{learnerDataId}/apprenticeshipId")
        {
            Content = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json")
        };
        _logger.LogTrace("Sending ApprenticeshipId Patch to outer API");
        var response = await _httpClient.SendAsync(requestMessage);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Unsuccessful status code returned from API {0}", response.StatusCode);
            throw new HttpRequestException("Unsuccessful status code returned when updating ApprenticeshipId on learner data record", null, response.StatusCode);
        }
    }
}