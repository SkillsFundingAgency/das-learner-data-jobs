using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.LearnerData.Application.OuterApi;

namespace SFA.DAS.LearnersData.Functions.ApprenticeshipStopBack
{
    public class HandleApprenticeshipStopBackEvent(ILearnerDataJobsOuterApi outerApi, ILogger<HandleApprenticeshipStopBackEvent> log) : IHandleMessages<ApprenticeshipStopBackEvent>
    {
        public async Task Handle(ApprenticeshipStopBackEvent message, IMessageHandlerContext context)
        {
            if (message.ApprenticeshipId == 0)
            {
                log.LogTrace("No patch of ApprenticeshipId required");
                return;
            }

            log.LogTrace("NServiceBus sending PatchLearnerDataApprenticeshipIdRequest");
            var request = new PatchLearnerDataApprenticeshipIdRequest
            {
                ApprenticeshipId = message.ApprenticeshipId
            };

            await outerApi.PatchApprenticeshipId(message.ProviderId, message.LearnerDataId.Value, request);
            log.LogTrace("NServiceBus sent PatchLearnerDataApprenticeshipIdRequest");
        }
    }
}
