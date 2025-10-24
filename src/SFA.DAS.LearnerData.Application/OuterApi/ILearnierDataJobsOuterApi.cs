using SFA.DAS.LearnerData.Application.Models;

namespace SFA.DAS.LearnerData.Application.OuterApi;
public interface ILearnerDataJobsOuterApi
{
    Task AddOrUpdateLearner(LearnerDataRequest message);
    Task PatchApprenticeshipId(long providerId, long learnerDataId, PatchLearnerDataApprenticeshipIdRequest message);
    Task PatchApprenticeshipStop(long providerId, long learnerDataId, ApprenticeshipStopRequest message);
}