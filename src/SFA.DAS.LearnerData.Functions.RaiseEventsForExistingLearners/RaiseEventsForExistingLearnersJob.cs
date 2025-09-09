using Microsoft.Extensions.Logging;
using SFA.DAS.LearnerData.Application.OuterApi;
using System.Text.Json;

namespace SFA.DAS.LearnerData.Functions.RaiseEventsForExistingLearners;

public class RaiseEventsForExistingLearnersJob
{
    private readonly ILearnerDataJobsOuterApi _outerApiService;
    private readonly ILogger<RaiseEventsForExistingLearnersJob> _logger;
    private readonly HttpClient _httpClient;
    private const int BatchSize = 100;
    private const int MaxRetries = 3;

    public RaiseEventsForExistingLearnersJob(
        ILearnerDataJobsOuterApi outerApiService, 
        ILogger<RaiseEventsForExistingLearnersJob> logger)
    {
        _outerApiService = outerApiService;
        _logger = logger;
        _httpClient = new HttpClient();
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting to process learners with null ApprenticeshipId");

        var totalProcessed = 0;
        var totalErrors = 0;
        var page = 1;
        var hasMorePages = true;

        while (hasMorePages)
        {
            try
            {
                _logger.LogInformation("Processing page {Page} with batch size {BatchSize}", page, BatchSize);
                
                var learners = await GetLearnersPageAsync(page, BatchSize);
                
                if (learners == null || !learners.Any())
                {
                    _logger.LogInformation("No more learners found. Processing complete.");
                    hasMorePages = false;
                    break;
                }

                _logger.LogInformation("Found {Count} learners on page {Page}", learners.Count(), page);

                foreach (var learner in learners)
                {
                    try
                    {
                        await RaiseLearnerDataEventAsync(learner);
                        totalProcessed++;
                        
                        if (totalProcessed % 10 == 0)
                        {
                            _logger.LogInformation("Processed {TotalProcessed} learners so far", totalProcessed);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to raise event for learner {LearnerId} (ULN: {ULN})", 
                            learner.Id, learner.Uln);
                        totalErrors++;
                    }
                }

                // If we got fewer learners than the batch size, we've reached the end
                if (learners.Count() < BatchSize)
                {
                    hasMorePages = false;
                }
                else
                {
                    page++;
                }

                // Add a small delay between batches to avoid overwhelming the system
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process page {Page}", page);
                totalErrors++;
                
                // If we get too many errors, stop processing
                if (totalErrors > 100)
                {
                    _logger.LogError("Too many errors encountered. Stopping processing.");
                    break;
                }
                
                page++;
            }
        }

        _logger.LogInformation("Job completed. Total processed: {TotalProcessed}, Total errors: {TotalErrors}", 
            totalProcessed, totalErrors);
    }

    private async Task<IEnumerable<LearnerDataApiResponse>> GetLearnersPageAsync(int page, int pageSize)
    {
        // This would need to be configured with the actual API base URL
        var apiBaseUrl = Environment.GetEnvironmentVariable("LearnerDataApiBaseUrl") ?? "https://localhost:5001";
        var url = $"{apiBaseUrl}/providers/10000001/learners?page={page}&pageSize={pageSize}&excludeApproved=true";
        
        _logger.LogDebug("Fetching learners from: {Url}", url);

        var response = await _httpClient.GetAsync(url);
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to fetch learners. Status: {StatusCode}, Reason: {ReasonPhrase}", 
                response.StatusCode, response.ReasonPhrase);
            return Enumerable.Empty<LearnerDataApiResponse>();
        }

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<LearnerDataApiResponseWrapper>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return apiResponse?.Data ?? Enumerable.Empty<LearnerDataApiResponse>();
    }

    private async Task RaiseLearnerDataEventAsync(LearnerDataApiResponse learner)
    {
        var learnerDataRequest = new LearnerDataRequest
        {
            ULN = learner.Uln,
            UKPRN = learner.Ukprn,
            FirstName = learner.FirstName,
            LastName = learner.LastName,
            Email = learner.Email,
            DoB = learner.Dob,
            StartDate = learner.StartDate,
            PlannedEndDate = learner.PlannedEndDate,
            PercentageLearningToBeDelivered = learner.PercentageLearningToBeDelivered,
            EpaoPrice = learner.EpaoPrice,
            TrainingPrice = learner.TrainingPrice,
            AgreementId = learner.AgreementId,
            IsFlexiJob = learner.IsFlexiJob,
            PlannedOTJTrainingHours = learner.PlannedOTJTrainingHours,
            StandardCode = learner.StandardCode,
            ConsumerReference = learner.ConsumerReference,
            CorrelationId = Guid.NewGuid(), // Generate new correlation ID for each event
            ReceivedDate = DateTime.UtcNow,
            AcademicYear = learner.AcademicYear
        };

        // Use the existing outer API service to raise the event
        // This will go through the normal event publishing mechanism
        await _outerApiService.AddOrUpdateLearner(learnerDataRequest);
        
        _logger.LogDebug("Raised LearnerDataEvent for learner {LearnerId} (ULN: {ULN})", 
            learner.Id, learner.Uln);
    }
}

// Response model for the API
public class LearnerDataApiResponse
{
    public long Id { get; set; }
    public long Uln { get; set; }
    public long Ukprn { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime Dob { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public int? PercentageLearningToBeDelivered { get; set; }
    public int EpaoPrice { get; set; }
    public int TrainingPrice { get; set; }
    public string AgreementId { get; set; } = string.Empty;
    public bool IsFlexiJob { get; set; }
    public int PlannedOTJTrainingHours { get; set; }
    public int StandardCode { get; set; }
    public string ConsumerReference { get; set; } = string.Empty;
    public DateTime ReceivedDate { get; set; }
    public int AcademicYear { get; set; }
    public long? ApprenticeshipId { get; set; }
}

// Wrapper for the API response
public class LearnerDataApiResponseWrapper
{
    public IEnumerable<LearnerDataApiResponse> Data { get; set; } = Enumerable.Empty<LearnerDataApiResponse>();
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int Page { get; set; }
}
