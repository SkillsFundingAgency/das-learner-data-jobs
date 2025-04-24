using Microsoft.Extensions.Logging;
using SFA.DAS.LearnerData.Application.OuterApi;
using SFA.DAS.LearnerData.Events;

namespace SFA.DAS.LearnerData.Functions.ProcessLearners;

public class HandleLearnerDataEvent(ILearnerDataJobsOuterApi outerApi, ILogger<HandleLearnerDataEvent> log) : IHandleMessages<LearnerDataEvent>
{
    public async Task Handle(LearnerDataEvent message, IMessageHandlerContext context)
    {
        log.LogTrace("NServiceBus sending LearnerDataRequest");
        var request = new LearnerDataRequest
        {
            ULN = message.ULN,
            UKPRN = message.UKPRN,
            FirstName = message.FirstName,
            LastName = message.LastName,
            Email = message.Email,
            DoB = message.DoB,
            StartDate = message.StartDate,
            PlannedEndDate = message.PlannedEndDate,
            PercentageLearningToBeDelivered = message.PercentageLearningToBeDelivered,
            EpaoPrice = message.EpaoPrice,
            TrainingPrice = message.TrainingPrice,
            AgreementId = message.AgreementId,
            IsFlexJob = message.IsFlexJob,
            PlannedOTJTrainingHours = message.PlannedOTJTrainingHours,
            StandardCode = message.StandardCode,
            ConsumerReference = message.ConsumerReference,
            CorrelationId = message.CorrelationId,
            ReceivedDate = message.ReceivedDate,
            AcademicYear = message.AcademicYear
        };

        await outerApi.AddOrUpdateLearner(request);
        log.LogTrace("NServiceBus sent LearnerDataRequest");
    }
}
