using System.Diagnostics;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using SFA.DAS.LearnerData.Application.OuterApi;
using SFA.DAS.LearnerData.Application.OuterApi.Responses;
using SFA.DAS.LearnerData.Events;

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
                logger.LogError(ex, "Failed to process page {Page}", page);
                totalErrors++;
                page++;
            }
        }

        logger.LogInformation("Job {JobId} completed with correlation ID {CorrelationId}. Total processed: {TotalProcessed}, Total errors: {TotalErrors}", 
            jobId, activity.Id, totalProcessed, totalErrors);
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
        var learnerDataEvent = new LearnerDataEvent
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
            CorrelationId = Guid.NewGuid(),
            ReceivedDate = DateTime.UtcNow,
            AcademicYear = learner.AcademicYear
        };
        
        await functionEndpoint.Send(learnerDataEvent, executionContext);
        
        logger.LogDebug("Published LearnerDataEvent for learner {LearnerId} (ULN: {ULN})", 
            learner.Id, learner.Uln);
    }
}