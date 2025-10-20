using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.LearnerData.Application.OuterApi;

namespace SFA.DAS.LearnerData.Functions.Approvals;

public class HandleApprenticeshipCreatedEvent(ILearnerDataJobsOuterApi outerApi, ILogger<HandleApprenticeshipCreatedEvent> log) : IHandleMessages<ApprenticeshipCreatedEvent>
{
    public async Task Handle(ApprenticeshipCreatedEvent message, IMessageHandlerContext context)
    {
        log.LogInformation("Handling ApprenticeshipCreatedEvent");
        log.LogWarning("Inside Handle apprenticeship created event warning.");
        if (message.LearnerDataId == null)
        {
            log.LogTrace("No patch of LearnerData required");
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
