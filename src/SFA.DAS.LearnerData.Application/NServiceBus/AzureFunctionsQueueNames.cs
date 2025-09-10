namespace SFA.DAS.LearnerData.Application.NServiceBus;

public static class AzureFunctionsQueueNames
{
    public const string ProcessLearnersQueue = "SFA.DAS.LearnerData.Functions.ProcessLearners";
    public const string ApprovalsQueue = "SFA.DAS.LearnerData.Functions.Approvals";
    public const string RaiseEventsForExistingLearnersQueue = "SFA.DAS.LearnerData.Functions.ExistingLearners";
}