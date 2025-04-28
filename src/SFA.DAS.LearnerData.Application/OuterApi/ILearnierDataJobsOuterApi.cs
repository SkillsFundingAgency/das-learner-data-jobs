namespace SFA.DAS.LearnerData.Application.OuterApi;

public interface ILearnerDataJobsOuterApi
{
    Task AddOrUpdateLearner(LearnerDataRequest message);
}