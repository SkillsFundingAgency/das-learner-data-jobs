namespace SFA.DAS.LearnerData.Application.OuterApi;

public interface ILearnerDataJobsOuterApi
{
    Task AddLearner(LearnerDataRequest message);
}