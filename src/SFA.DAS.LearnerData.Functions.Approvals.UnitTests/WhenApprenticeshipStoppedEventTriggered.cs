using AutoFixture.NUnit3;
using Microsoft.VisualBasic;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.LearnerData.Application.Models;
using SFA.DAS.LearnerData.Application.OuterApi;
using SFA.DAS.LearnerData.Events;
using SFA.DAS.LearnersData.Functions.Approvals;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.LearnerData.Functions.Approvals.UnitTests;
    public class WhenApprenticeshipStoppedEventTriggered
    {
        [Test, MoqAutoData]
        public async Task And_No_LearnerDataID_Then_swallow_event(
        [Frozen] Mock<ILearnerDataJobsOuterApi> api,
        HandleApprenticeshipStoppedEvent sut,
        ApprenticeshipStoppedEvent evt)
        {
            evt.LearnerDataId = null;

            await sut.Handle(evt, null);

            api.Verify(
                x => x.PatchApprenticeshipId(It.IsAny<long>(), It.IsAny<long>(),
                    It.IsAny<PatchLearnerDataApprenticeshipIdRequest>()), Times.Never);
        }

        [Test, MoqAutoData]
        public async Task And__Present_Then_notify_apim(
            [Frozen] Mock<ILearnerDataJobsOuterApi> api,
            HandleApprenticeshipStoppedEvent sut,
            ApprenticeshipStoppedEvent evt)
        {
            api.Setup(t => t.GetLearnerById(It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync(new GetLearnerDataResponse()
            {
                     ApprenticeshipId = evt.ApprenticeshipId,                    
            });

            await sut.Handle(evt, null);
            

            api.Verify(
                x => x.PatchApprenticeshipId(evt.ProviderId, evt.LearnerDataId.Value,
                    It.Is<PatchLearnerDataApprenticeshipIdRequest>(p => p.ApprenticeshipId == null)), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task And_No_Matching_ApprenticehsipId_Then_swallow_event(
        [Frozen] Mock<ILearnerDataJobsOuterApi> api,
        HandleApprenticeshipStoppedEvent sut,
        ApprenticeshipStoppedEvent evt)
        {
            await sut.Handle(evt, null);

            api.Verify(
                x => x.PatchApprenticeshipId(It.IsAny<long>(), It.IsAny<long>(),
                    It.IsAny<PatchLearnerDataApprenticeshipIdRequest>()), Times.Never);
        }

        [Test, MoqAutoData]
        public async Task And_No_ApprenticeShipID_Then_swallow_event(
        [Frozen] Mock<ILearnerDataJobsOuterApi> api,
        HandleApprenticeshipStoppedEvent sut,
        ApprenticeshipStoppedEvent evt)
        {
            evt.ApprenticeshipId = 0;

            await sut.Handle(evt, null);

            api.Verify(
                x => x.PatchApprenticeshipId(It.IsAny<long>(), It.IsAny<long>(),
                    It.IsAny<PatchLearnerDataApprenticeshipIdRequest>()), Times.Never);
        }

        [Test, MoqAutoData]
        public async Task And_Not_IsWithdrawn_From_Start_Course_Then_swallow_event(
            [Frozen] Mock<ILearnerDataJobsOuterApi> api,
            HandleApprenticeshipStoppedEvent sut,
            ApprenticeshipStoppedEvent evt)
        {
            evt.IsWithDrawnAtStartOfCourse = false;

            await sut.Handle(evt, null);

            api.Verify(
                x => x.PatchApprenticeshipId(It.IsAny<long>(), It.IsAny<long>(),
                    It.IsAny<PatchLearnerDataApprenticeshipIdRequest>()), Times.Never);
        }
}
