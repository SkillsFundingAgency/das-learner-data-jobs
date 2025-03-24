using RestEase;

namespace SFA.DAS.LearnerData.Application.OuterApi;

public interface ILearnerDataJobsOuterApi
{
    [Post("/learners")]
    Task AddLearner([Body] LearnerDataRequest message);
}
