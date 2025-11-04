using SFA.DAS.LearnerData.Application.OuterApi.Requests;
using SFA.DAS.LearnerData.Application.OuterApi.Responses;

namespace SFA.DAS.LearnerData.Application.OuterApi;

public interface ILearnerDataJobsOuterApi
{
    Task AddOrUpdateLearner(LearnerDataRequest message);
    Task PatchApprenticeshipId(long providerId, long learnerDataId, PatchLearnerDataApprenticeshipIdRequest message);
    Task<GetLearnersApiResponse> GetLearnersAsync(int page, int pageSize, bool excludeApproved = true);
    Task PatchApprenticeshipStop(long providerId, long learnerDataId, ApprenticeshipStopRequest message);
    Task PatchApprenticeshipStopDateChanged(long providerId, long learnerDataId, ApprenticeshipStopRequest message);
}