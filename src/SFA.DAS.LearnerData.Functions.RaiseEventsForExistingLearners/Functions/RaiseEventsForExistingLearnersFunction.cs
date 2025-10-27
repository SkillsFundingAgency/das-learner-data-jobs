using System.Diagnostics;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using SFA.DAS.LearnerData.Application.OuterApi;
using SFA.DAS.LearnerData.Application.OuterApi.Responses;
using SFA.DAS.LearnerData.Events;
using SFA.DAS.LearnerData.Messages;

namespace SFA.DAS.LearnerData.Functions.RaiseEventsForExistingLearners.Functions;

public class RaiseEventsForExistingLearnersFunction(
    ILearnerDataJobsOuterApi apiClient,
    IFunctionEndpoint functionEndpoint,
    ILogger<RaiseEventsForExistingLearnersFunction> logger)
{
    private const int BatchSize = 100;

    [Function("RaiseEventsForExistingLearners")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestData req,
        FunctionContext executionContext)
    {
        logger.LogInformation("Timer trigger received for raising events for existing learners with null ApprenticeshipId");

        try
        {
            var jobId = Guid.NewGuid().ToString();
            logger.LogInformation("Job {JobId} started", jobId);

            await ExecuteJobAsync(jobId, executionContext);
            
            logger.LogInformation("Job {JobId} completed successfully", jobId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute job with error: {ErrorMessage}", ex.Message);
            throw;
        }

        return req.CreateResponse(HttpStatusCode.NoContent);
    }

    private async Task ExecuteJobAsync(string jobId, FunctionContext executionContext)
    {
        using var activity = new Activity("RaiseEventsForExistingLearners");
        activity.SetIdFormat(ActivityIdFormat.W3C);
        activity.Start();
        activity.SetTag("jobId", jobId);
        activity.SetTag("operation", "RaiseEventsForExistingLearners");
        
        logger.LogInformation("Starting to process learners with null ApprenticeshipId for job {JobId} with correlation ID {CorrelationId}", 
            jobId, activity.Id);

        var totalProcessed = 0;
        var totalErrors = 0;
        var consecutivePageErrors = 0;
        const int maxConsecutivePageErrors = 5;
        var page = 1;

        while (true)
        {
            try
            {
                var learners = await GetLearnersPageAsync(page, BatchSize);
                
                if (learners == null || !learners.Any())
                {
                    break;
                }

                consecutivePageErrors = 0;

                foreach (var learner in learners)
                {
                    try
                    {
                        await RaiseLearnerDataEventAsync(learner, executionContext);
                        totalProcessed++;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to raise event for learner {LearnerId} (ULN: {ULN})", 
                            learner.Id, learner.Uln);
                        totalErrors++;
                    }
                }

                if (learners.Count() < BatchSize)
                {
                    break;
                }

                page++;
            }
            catch (Exception ex)
            {
                consecutivePageErrors++;
                totalErrors++;
                
                logger.LogError(ex, "Failed to process page {Page}. Consecutive page errors: {ConsecutivePageErrors}/{MaxConsecutivePageErrors}", 
                    page, consecutivePageErrors, maxConsecutivePageErrors);

                if (consecutivePageErrors >= maxConsecutivePageErrors)
                {
                    logger.LogError("Circuit breaker triggered after {ConsecutivePageErrors} consecutive page errors. Stopping job execution.", 
                        consecutivePageErrors);
                    break;
                }

                page++;
            }
        }

        logger.LogInformation("Job {JobId} completed with correlation ID {CorrelationId}. Total processed: {TotalProcessed}, Total errors: {TotalErrors}, Consecutive page errors: {ConsecutivePageErrors}", 
            jobId, activity.Id, totalProcessed, totalErrors, consecutivePageErrors);
    }

    private async Task<IEnumerable<LearnerDataApiResponse>> GetLearnersPageAsync(int page, int pageSize)
    {
        logger.LogDebug("Fetching all learners from APIM endpoint, page {Page}, pageSize {PageSize}", 
            page, pageSize);

        var response = await apiClient.GetLearnersAsync(page, pageSize, true);
        return response.Data;
    }

    private async Task RaiseLearnerDataEventAsync(LearnerDataApiResponse learner, FunctionContext executionContext)
    {
        if (learner.UpdatedDate == null || learner.UpdatedDate <= learner.CreatedDate)
        {
            logger.LogDebug("Skipping event for learner {LearnerId} (ULN: {ULN}) - UpdatedDate ({UpdatedDate}) is not greater than CreatedDate ({CreatedDate})", 
                learner.Id, learner.Uln, learner.UpdatedDate, learner.CreatedDate);
            return;
        }

        var learnerDataUpdatedEvent = new LearnerDataUpdatedEvent
        {
            LearnerId = learner.Id,
            ChangedAt = DateTime.UtcNow
        };
        
        await functionEndpoint.Publish(learnerDataUpdatedEvent, executionContext);
        
        logger.LogInformation("Published LearnerDataUpdatedEvent for learner {LearnerId} (ULN: {ULN}) - UpdatedDate ({UpdatedDate}) > CreatedDate ({CreatedDate})", 
            learner.Id, learner.Uln, learner.UpdatedDate, learner.CreatedDate);
    }
}