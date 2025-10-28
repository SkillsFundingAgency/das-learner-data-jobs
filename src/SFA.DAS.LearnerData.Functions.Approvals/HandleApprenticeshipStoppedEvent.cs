using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.LearnerData.Application.OuterApi;

namespace SFA.DAS.LearnersData.Functions.Approvals;
public class HandleApprenticeshipStoppedEvent(ILearnerDataJobsOuterApi outerApi, ILogger<HandleApprenticeshipStoppedEvent> log) : IHandleMessages<ApprenticeshipStoppedEvent>
{
    public async Task Handle(ApprenticeshipStoppedEvent message, IMessageHandlerContext context)
    {
        log.LogInformation("Handling ApprenticeshipStoppedEvent");
        if (message.ApprenticeshipId == 0)
        {
            log.LogInformation("No patch of ApprenticeshipId required");
            return;
        }
        if (message.LearnerDataId is null)
        {
            log.LogInformation("Learner Data Id is required");
            return;
        }
        if (!message.IsWithDrawnAtStartOfCourse)
        {
            log.LogInformation("Apprentice is not withdrawn from start");
            return;
        }
        log.LogInformation("NServiceBus  sending ApprenticeshipStoppedRequest");
        await outerApi.PatchApprenticeshipStop(message.ProviderId, (long)message.LearnerDataId,
                new ApprenticeshipStopRequest()
                {
                    IsWithDrawnAtStartOfCourse = message.IsWithDrawnAtStartOfCourse,
                    StopDate = message.StopDate,
                    ApprenticeshipId = message.ApprenticeshipId,
                });

        log.LogInformation("NServiceBus sent ApprenticeshipStoppedRequest");
        return;
    }
}