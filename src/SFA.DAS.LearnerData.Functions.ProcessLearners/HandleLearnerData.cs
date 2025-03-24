using Microsoft.Extensions.Logging;
using SFA.DAS.LearnerData.Application.Events;
using SFA.DAS.LearnerData.Application.OuterApi;

namespace SFA.DAS.LearnerData.Functions.ProcessLearners;

public class HandleAccountAddedEvent(ILearnerDataJobsOuterApi outerApi, ILogger<HandleAccountAddedEvent> log) : IHandleMessages<LearnerDataEvent>
{
    public async Task Handle(LearnerDataEvent message, IMessageHandlerContext context)
    {
        log.LogTrace("NServiceBus sending LearnerDataRequest");
        var request = new LearnerDataRequest
        {
            ULN = message.ULN,
            UKPRN = message.UKPRN,
            Firstname = message.Firstname,
            Lastname = message.Lastname,
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
            StandardCode = message.StandardCode
        };
        
        await outerApi.AddLearner(request);
        log.LogTrace("NServiceBus sent LearnerDataRequest");
    }
}
