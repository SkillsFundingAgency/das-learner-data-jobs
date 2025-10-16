using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.LearnerData.Application.OuterApi;

namespace SFA.DAS.LearnersData.Functions.Approvals;
    public class HandleApprenticeshipStoppedEvent(ILearnerDataJobsOuterApi outerApi, ILogger<HandleApprenticeshipStoppedEvent> log) : IHandleMessages<ApprenticeshipStoppedEvent>
    {
        public async Task Handle(ApprenticeshipStoppedEvent message, IMessageHandlerContext context)
        {
           log.LogInformation("Inside Handle apprenticeship stopped events");
            if (message.ApprenticeshipId == 0)
            {
                log.LogTrace("No patch of ApprenticeshipId required");
                return;
            }
            if (message.LearnerDataId is null)
            {
                log.LogTrace("Learner Data Id is required");
                return;
            }
            if(!message.IsWithDrawnAtStartOfCourse)
            {
                log.LogTrace("Apprentice is not withdrawn from start");
                return;
            }

            var learner = await outerApi.GetLearnerById(message.ProviderId, message.LearnerDataId);
            if (message.ApprenticeshipId == learner.ApprenticeshipId && message.IsWithDrawnAtStartOfCourse)
            {
                log.LogTrace("NServiceBus sending PatchLearnerDataApprenticeshipIdRequest");
                var request = new PatchLearnerDataApprenticeshipIdRequest
                {
                    ApprenticeshipId = null
                };

                await outerApi.PatchApprenticeshipId(message.ProviderId,(long)message.LearnerDataId, request);
                log.LogTrace("NServiceBus sent PatchLearnerDataApprenticeshipIdRequest");
            }

            log.LogTrace("NServiceBus not sent PatchLearnerDataApprenticeshipIdRequest");
            return;
        }
    }