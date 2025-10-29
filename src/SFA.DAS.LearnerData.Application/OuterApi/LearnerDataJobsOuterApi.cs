using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.LearnerData.Application.OuterApi.Requests;
using SFA.DAS.LearnerData.Application.OuterApi.Responses;
using System.Text;
using System.Text.Json;

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

    public async Task PatchApprenticeshipStop(long providerId, long learnerDataId, ApprenticeshipStopRequest message)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Patch, $"providers/{providerId}/learner/{learnerDataId}/apprenticeship-stop")
        {
            Content = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json")
        };

        _logger.LogTrace("sening learnder DataId to patch to outer API");
        var response = await _httpClient.SendAsync(requestMessage);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Unsuccessful status code returned from API {0}", response.StatusCode);
            throw new HttpRequestException("Unsuccessful status code returned when getting learner data", null, response.StatusCode);
        }
    }

    public async Task<GetLearnersApiResponse> GetLearnersAsync(int page, int pageSize, bool excludeApproved = true)
    {
        var url = $"learners?page={page}&pageSize={pageSize}&excludeApproved={excludeApproved.ToString().ToLower()}";
        
        _logger.LogDebug("Fetching all learners from: {Url}", url);

        var response = await _httpClient.GetAsync(url);
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to fetch all learners. Status: {StatusCode}, Reason: {ReasonPhrase}", 
                response.StatusCode, response.ReasonPhrase);
            return new GetLearnersApiResponse();
        }

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = System.Text.Json.JsonSerializer.Deserialize<GetLearnersApiResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return apiResponse ?? new GetLearnersApiResponse();
    }
}