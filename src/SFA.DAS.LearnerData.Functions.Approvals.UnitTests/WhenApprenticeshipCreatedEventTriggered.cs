using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.LearnerData.Application.OuterApi;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.LearnerData.Functions.Approvals.UnitTests;

public class WhenApprenticeshipCreatedEventTriggered
{
    [Test, MoqAutoData]
    public async Task And_No_LearnerDataID_Then_swallow_event(
        [Frozen] Mock<ILearnerDataJobsOuterApi> api,
        HandleApprenticeshipCreatedEvent sut,
        ApprenticeshipCreatedEvent evt)
    {
        evt.LearnerDataId = null;

        await sut.Handle(evt, null);

        api.Verify(
            x => x.PatchApprenticeshipId(It.IsAny<long>(), It.IsAny<long>(),
                It.IsAny<PatchLearnerDataApprenticeshipIdRequest>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task And_LearnerDataId_Present_Then_notify_apim(
        [Frozen] Mock<ILearnerDataJobsOuterApi> api,
        HandleApprenticeshipCreatedEvent sut,
        ApprenticeshipCreatedEvent evt)
    {
        await sut.Handle(evt, null);

        api.Verify(
            x => x.PatchApprenticeshipId(evt.ProviderId, evt.LearnerDataId.Value, 
                It.Is<PatchLearnerDataApprenticeshipIdRequest>(p=> p.ApprenticeshipId == evt.ApprenticeshipId)), Times.Once);
    }
}