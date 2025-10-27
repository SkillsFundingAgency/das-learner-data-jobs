using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.LearnerData.Application.OuterApi;
using SFA.DAS.LearnerData.Application.OuterApi.Requests;
using SFA.DAS.LearnerData.Events;
using SFA.DAS.LearnerData.Functions.ProcessLearners.Functions;
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
        var request = new LearnerDataRequest();

        api.Setup(x => x.AddOrUpdateLearner(It.IsAny<LearnerDataRequest>())).Callback((LearnerDataRequest p) =>
        {
            request = p;
        });

        await sut.Handle(evt, null);

        request.Should().BeEquivalentTo(evt);
    }
}