namespace SFA.DAS.LearnerData.Application.OuterApi.Responses;

public class GetLearnersApiResponse
{
    public IEnumerable<LearnerDataApiResponse> Data { get; set; } = [];
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int Page { get; set; }
}

public class LearnerDataApiResponse
{
    public long Id { get; set; }
    public long Uln { get; set; }
    public long Ukprn { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime Dob { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public int? PercentageLearningToBeDelivered { get; set; }
    public int EpaoPrice { get; set; }
    public int TrainingPrice { get; set; }
    public string AgreementId { get; set; } = string.Empty;
    public bool IsFlexiJob { get; set; }
    public int PlannedOTJTrainingHours { get; set; }
    public int StandardCode { get; set; }
    public string ConsumerReference { get; set; } = string.Empty;
    public DateTime ReceivedDate { get; set; }
    public int AcademicYear { get; set; }
    public long? ApprenticeshipId { get; set; }
}
