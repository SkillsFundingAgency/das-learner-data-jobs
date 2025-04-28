﻿using Microsoft.Extensions.Logging;
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
}