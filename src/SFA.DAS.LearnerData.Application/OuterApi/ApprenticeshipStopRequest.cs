namespace SFA.DAS.LearnerData.Application.OuterApi;

public class ApprenticeshipStopRequest
{
    public long ApprenticeshipId { get; set; }

    public DateTime StopDate { get; set; }

    public DateTime AppliedOn { get; set; }

    public bool IsWithDrawnAtStartOfCourse { get; set; }

    public long? LearnerDataId { get; set; }

    public long ProviderId { get; set; }
}
