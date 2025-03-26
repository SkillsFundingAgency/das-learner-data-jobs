using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using SFA.DAS.LearnerData.Application.Events;
using SFA.DAS.LearnerData.Application.OuterApi;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.LearnerData.Functions.ProcessLearners.UnitTests;

public class WhenLearnerDataTriggered
{
    [Test, MoqAutoData]
    public async Task Then_notify_apim(
        [Frozen] Mock<ILearnerDataJobsOuterApi> api,
        HandleLearnerDataEvent sut,
        LearnerDataEvent evt)
    {
        await sut.Handle(evt, null);

        api.Verify(m =>
            m.AddLearner(It.Is<LearnerDataRequest>(p =>
                p.ULN == evt.ULN && p.UKPRN == evt.UKPRN && p.Firstname == evt.Firstname &&
                p.Lastname == evt.Lastname && p.Email == evt.Email && p.DoB == evt.DoB &&
                p.StartDate == evt.StartDate && p.PlannedEndDate == evt.PlannedEndDate && 
                p.PercentageLearningToBeDelivered == evt.PercentageLearningToBeDelivered &&
                p.EpaoPrice == evt.EpaoPrice && p.TrainingPrice == evt.TrainingPrice && 
                p.AgreementId == evt.AgreementId && p.IsFlexJob == evt.IsFlexJob && 
                p.PlannedOTJTrainingHours == evt.PlannedOTJTrainingHours && p.StandardCode == evt.StandardCode)));
    }
}