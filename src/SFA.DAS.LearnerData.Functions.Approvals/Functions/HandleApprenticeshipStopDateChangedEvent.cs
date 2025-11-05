using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.LearnerData.Application.OuterApi;

namespace SFA.DAS.LearnerData.Functions.Approvals.Functions;
public class HandleApprenticeshipStopDateChangedEvent(ILearnerDataJobsOuterApi outerApi, 
    ILogger<HandleApprenticeshipStopDateChangedEvent> log) 
    : IHandleMessages<ApprenticeshipStopDateChangedEvent>
{
    public async Task Handle(ApprenticeshipStopDateChangedEvent message, IMessageHandlerContext context)
    {
        log.LogInformation("Handling ApprenticeshipStopDateChangedEvent");

        if (message.LearnerDataId is null)
        {
            log.LogInformation("Learner Data Id is required");
            return;
        }
        
        log.LogInformation("NServiceBus  sending ApprenticeshipStoppedRequest from ApprenticeshipStopDateChangedEvent");
        await outerApi.PatchApprenticeshipStopDateChanged(message.ProviderId, (long)message.LearnerDataId,
                new ApprenticeshipStopRequest()
                {
                    IsWithDrawnAtStartOfCourse = message.IsWithDrawnAtStartOfCourse,
                    StopDate = message.StopDate,
                    ApprenticeshipId = message.ApprenticeshipId,
                });

        log.LogInformation("NServiceBus sent ApprenticeshipStoppedRequest from ApprenticeshipStopDateChangedEvent");
        return;
    }
}