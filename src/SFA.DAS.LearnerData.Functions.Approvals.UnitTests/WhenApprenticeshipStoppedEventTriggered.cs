using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.LearnerData.Application.OuterApi;
using SFA.DAS.LearnersData.Functions.Approvals;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.LearnerData.Functions.Approvals.UnitTests;
    public class WhenApprenticeshipStoppedEventTriggered
    {        

        [Test, MoqAutoData]
        public async Task And__Present_Then_notify_apim(
            [Frozen] Mock<ILearnerDataJobsOuterApi> api,
            HandleApprenticeshipStoppedEvent sut,
            ApprenticeshipStoppedEvent evt)
        {

            await sut.Handle(evt, null);
            

            api.Verify(
                x => x.PatchApprenticeshipStop(evt.ProviderId, (long)evt.LearnerDataId,
                    It.Is<ApprenticeshipStopRequest>(p=>p.ApprenticeshipId == evt.ApprenticeshipId && p.IsWithDrawnAtStartOfCourse == evt.IsWithDrawnAtStartOfCourse )), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task And_No_Matching_ApprenticehsipId_Then_swallow_event(
        [Frozen] Mock<ILearnerDataJobsOuterApi> api,
        HandleApprenticeshipStoppedEvent sut,
        ApprenticeshipStoppedEvent evt)
        {
            await sut.Handle(evt, null);

        api.Verify(
          x => x.PatchApprenticeshipStop(evt.ProviderId, (long)evt.LearnerDataId,
              It.Is<ApprenticeshipStopRequest>(p => p.ApprenticeshipId == 1 && p.IsWithDrawnAtStartOfCourse == evt.IsWithDrawnAtStartOfCourse)), Times.Never);

        }

        [Test, MoqAutoData]
        public async Task And_No_ApprenticehsipId_Then_swallow_event(
           [Frozen] Mock<ILearnerDataJobsOuterApi> api,
           HandleApprenticeshipStoppedEvent sut,
           ApprenticeshipStoppedEvent evt)
        {
            await sut.Handle(evt, null);

            api.Verify(
              x => x.PatchApprenticeshipStop(evt.ProviderId, (long)evt.LearnerDataId,
                  It.Is<ApprenticeshipStopRequest>(p => p.ApprenticeshipId == 0 && p.IsWithDrawnAtStartOfCourse == evt.IsWithDrawnAtStartOfCourse)), Times.Never);

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
                x => x.PatchApprenticeshipStop(evt.ProviderId, evt.LearnerDataId.Value,
                    It.Is<ApprenticeshipStopRequest>(p => p.ApprenticeshipId == 1 && p.IsWithDrawnAtStartOfCourse == false)), Times.Never);
        }
}
