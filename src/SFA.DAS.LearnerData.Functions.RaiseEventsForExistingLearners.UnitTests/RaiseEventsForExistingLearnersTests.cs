using System.Net;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.LearnerData.Application.OuterApi;
using SFA.DAS.LearnerData.Application.OuterApi.Responses;
using SFA.DAS.LearnerData.Events;
using SFA.DAS.LearnerData.Functions.RaiseEventsForExistingLearners.Functions;
using SFA.DAS.LearnerData.Functions.RaiseEventsForExistingLearners.UnitTests.Helpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.LearnerData.Functions.RaiseEventsForExistingLearners.UnitTests;

public class RaiseEventsForExistingLearnersTests
{
    [Test, MoqAutoData]
    public async Task And_No_Learners_Then_Return_NoContent(
        [Frozen] Mock<ILearnerDataJobsOuterApi> apiClient,
        [Frozen] Mock<IFunctionEndpoint> functionEndpoint,
        [Frozen] Mock<ILogger<RaiseEventsForExistingLearnersFunction>> logger,
        RaiseEventsForExistingLearnersFunction sut)
    {
        // Arrange
        var mockFunctionContext = new Mock<FunctionContext>();
        var requestData = new FakeHttpRequestData(mockFunctionContext.Object, new Uri("https://test"));
        
        var emptyResponse = new GetLearnersApiResponse
        {
            Data = new List<LearnerDataApiResponse>(),
            Page = 1,
            PageSize = 100,
            TotalItems = 0,
            TotalPages = 0
        };
        
        apiClient.Setup(x => x.GetLearnersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            .ReturnsAsync(emptyResponse)
            .Verifiable();
        
        // Act
        var result = await sut.Run(requestData, mockFunctionContext.Object);
        
        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        apiClient.Verify();
        functionEndpoint.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<FunctionContext>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task And_Single_Page_Of_Learners_Then_Publish_Events_And_Return_NoContent(
        [Frozen] Mock<ILearnerDataJobsOuterApi> apiClient,
        [Frozen] Mock<IFunctionEndpoint> functionEndpoint,
        [Frozen] Mock<ILogger<RaiseEventsForExistingLearnersFunction>> logger,
        RaiseEventsForExistingLearnersFunction sut,
        List<LearnerDataApiResponse> learners)
    {
        // Arrange
        var mockFunctionContext = new Mock<FunctionContext>();
        var requestData = new FakeHttpRequestData(mockFunctionContext.Object, new Uri("https://test"));
        
        var response = new GetLearnersApiResponse
        {
            Data = learners,
            Page = 1,
            PageSize = 100,
            TotalItems = learners.Count,
            TotalPages = 1
        };
        
        apiClient.Setup(x => x.GetLearnersAsync(1, 100, true))
            .ReturnsAsync(response)
            .Verifiable();
        
        apiClient.Setup(x => x.GetLearnersAsync(2, 100, true))
            .ReturnsAsync(new GetLearnersApiResponse { Data = new List<LearnerDataApiResponse>() });
        
        // Act
        var result = await sut.Run(requestData, mockFunctionContext.Object);
        
        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        apiClient.Verify();
        functionEndpoint.Verify(x => x.Publish(It.IsAny<LearnerDataEvent>(), It.IsAny<FunctionContext>(), It.IsAny<CancellationToken>()), Times.Exactly(learners.Count));
    }

    [Test, MoqAutoData]
    public async Task And_Multiple_Pages_Of_Learners_Then_Publish_All_Events_And_Return_NoContent(
        [Frozen] Mock<ILearnerDataJobsOuterApi> apiClient,
        [Frozen] Mock<IFunctionEndpoint> functionEndpoint,
        [Frozen] Mock<ILogger<RaiseEventsForExistingLearnersFunction>> logger,
        RaiseEventsForExistingLearnersFunction sut)
    {
        // Arrange
        var mockFunctionContext = new Mock<FunctionContext>();
        var requestData = new FakeHttpRequestData(mockFunctionContext.Object, new Uri("https://test"));
        
        var firstPageLearners = new List<LearnerDataApiResponse>();
        for (int i = 0; i < 100; i++)
        {
            firstPageLearners.Add(new LearnerDataApiResponse { Id = i + 1, Uln = 1000000000 + i });
        }
        
        var secondPageLearners = new List<LearnerDataApiResponse>();
        for (int i = 0; i < 50; i++)
        {
            secondPageLearners.Add(new LearnerDataApiResponse { Id = i + 101, Uln = 1000000100 + i });
        }
        
        var firstPageResponse = new GetLearnersApiResponse
        {
            Data = firstPageLearners,
            Page = 1,
            PageSize = 100,
            TotalItems = firstPageLearners.Count + secondPageLearners.Count,
            TotalPages = 2
        };
        
        var secondPageResponse = new GetLearnersApiResponse
        {
            Data = secondPageLearners,
            Page = 2,
            PageSize = 100,
            TotalItems = firstPageLearners.Count + secondPageLearners.Count,
            TotalPages = 2
        };
        
        apiClient.Setup(x => x.GetLearnersAsync(1, 100, true))
            .ReturnsAsync(firstPageResponse)
            .Verifiable();
        
        apiClient.Setup(x => x.GetLearnersAsync(2, 100, true))
            .ReturnsAsync(secondPageResponse)
            .Verifiable();
        
        // Act
        var result = await sut.Run(requestData, mockFunctionContext.Object);
        
        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        apiClient.Verify(x => x.GetLearnersAsync(1, 100, true), Times.Once);
        apiClient.Verify(x => x.GetLearnersAsync(2, 100, true), Times.Once);
        functionEndpoint.Verify(x => x.Publish(It.IsAny<LearnerDataEvent>(), It.IsAny<FunctionContext>(), It.IsAny<CancellationToken>()), 
            Times.Exactly(firstPageLearners.Count + secondPageLearners.Count));
    }

    [Test, MoqAutoData]
    public async Task And_Learner_Data_Event_Is_Correctly_Mapped_From_Api_Response(
        [Frozen] Mock<ILearnerDataJobsOuterApi> apiClient,
        [Frozen] Mock<IFunctionEndpoint> functionEndpoint,
        [Frozen] Mock<ILogger<RaiseEventsForExistingLearnersFunction>> logger,
        RaiseEventsForExistingLearnersFunction sut,
        LearnerDataApiResponse learner)
    {
        // Arrange
        var mockFunctionContext = new Mock<FunctionContext>();
        var requestData = new FakeHttpRequestData(mockFunctionContext.Object, new Uri("https://test"));
        
        var response = new GetLearnersApiResponse
        {
            Data = new List<LearnerDataApiResponse> { learner },
            Page = 1,
            PageSize = 100,
            TotalItems = 1,
            TotalPages = 1
        };
        
        apiClient.Setup(x => x.GetLearnersAsync(1, 100, true))
            .ReturnsAsync(response);
        
        apiClient.Setup(x => x.GetLearnersAsync(2, 100, true))
            .ReturnsAsync(new GetLearnersApiResponse { Data = new List<LearnerDataApiResponse>() });
        
        LearnerDataEvent? publishedEvent = null;
        functionEndpoint.Setup(x => x.Publish(It.IsAny<LearnerDataEvent>(), It.IsAny<FunctionContext>(), It.IsAny<CancellationToken>()))
            .Callback<object, FunctionContext, CancellationToken>((evt, ctx, ct) => publishedEvent = evt as LearnerDataEvent);
        
        // Act
        var result = await sut.Run(requestData, mockFunctionContext.Object);
        
        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        publishedEvent.Should().NotBeNull();
        publishedEvent!.ULN.Should().Be(learner.Uln);
        publishedEvent.UKPRN.Should().Be(learner.Ukprn);
        publishedEvent.FirstName.Should().Be(learner.FirstName);
        publishedEvent.LastName.Should().Be(learner.LastName);
        publishedEvent.Email.Should().Be(learner.Email);
        publishedEvent.DoB.Should().Be(learner.Dob);
        publishedEvent.StartDate.Should().Be(learner.StartDate);
        publishedEvent.PlannedEndDate.Should().Be(learner.PlannedEndDate);
        publishedEvent.PercentageLearningToBeDelivered.Should().Be(learner.PercentageLearningToBeDelivered);
        publishedEvent.EpaoPrice.Should().Be(learner.EpaoPrice);
        publishedEvent.TrainingPrice.Should().Be(learner.TrainingPrice);
        publishedEvent.AgreementId.Should().Be(learner.AgreementId);
        publishedEvent.IsFlexiJob.Should().Be(learner.IsFlexiJob);
        publishedEvent.PlannedOTJTrainingHours.Should().Be(learner.PlannedOTJTrainingHours);
        publishedEvent.StandardCode.Should().Be(learner.StandardCode);
        publishedEvent.ConsumerReference.Should().Be(learner.ConsumerReference);
        publishedEvent.AcademicYear.Should().Be(learner.AcademicYear);
        publishedEvent.CorrelationId.Should().NotBeEmpty();
        publishedEvent.ReceivedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test, MoqAutoData]
    public async Task And_Api_Throws_Exception_On_First_Call_Then_Log_Error_And_Continue_To_Next_Page(
        [Frozen] Mock<ILearnerDataJobsOuterApi> apiClient,
        [Frozen] Mock<IFunctionEndpoint> functionEndpoint,
        [Frozen] Mock<ILogger<RaiseEventsForExistingLearnersFunction>> logger,
        RaiseEventsForExistingLearnersFunction sut,
        Exception apiException)
    {
        // Arrange
        var mockFunctionContext = new Mock<FunctionContext>();
        var requestData = new FakeHttpRequestData(mockFunctionContext.Object, new Uri("https://test"));
        
        apiClient.Setup(x => x.GetLearnersAsync(1, 100, true))
            .ThrowsAsync(apiException);
        
        apiClient.Setup(x => x.GetLearnersAsync(2, 100, true))
            .ReturnsAsync(new GetLearnersApiResponse { Data = new List<LearnerDataApiResponse>() });
        
        // Act
        var result = await sut.Run(requestData, mockFunctionContext.Object);
        
        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        functionEndpoint.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<FunctionContext>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task And_Individual_Learner_Event_Publish_Fails_Then_Log_Error_And_Continue(
        [Frozen] Mock<ILearnerDataJobsOuterApi> apiClient,
        [Frozen] Mock<IFunctionEndpoint> functionEndpoint,
        [Frozen] Mock<ILogger<RaiseEventsForExistingLearnersFunction>> logger,
        RaiseEventsForExistingLearnersFunction sut,
        List<LearnerDataApiResponse> learners,
        Exception publishException)
    {
        // Arrange
        var mockFunctionContext = new Mock<FunctionContext>();
        var requestData = new FakeHttpRequestData(mockFunctionContext.Object, new Uri("https://test"));
        
        var response = new GetLearnersApiResponse
        {
            Data = learners,
            Page = 1,
            PageSize = 100,
            TotalItems = learners.Count,
            TotalPages = 1
        };
        
        apiClient.Setup(x => x.GetLearnersAsync(1, 100, true))
            .ReturnsAsync(response);
        
        apiClient.Setup(x => x.GetLearnersAsync(2, 100, true))
            .ReturnsAsync(new GetLearnersApiResponse { Data = new List<LearnerDataApiResponse>() });
        
        functionEndpoint.Setup(x => x.Publish(It.IsAny<LearnerDataEvent>(), It.IsAny<FunctionContext>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(publishException);
        
        // Act
        var result = await sut.Run(requestData, mockFunctionContext.Object);
        
        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test, MoqAutoData]
    public async Task And_Page_Processing_Fails_Then_Log_Error_And_Continue_To_Next_Page(
        [Frozen] Mock<ILearnerDataJobsOuterApi> apiClient,
        [Frozen] Mock<IFunctionEndpoint> functionEndpoint,
        [Frozen] Mock<ILogger<RaiseEventsForExistingLearnersFunction>> logger,
        RaiseEventsForExistingLearnersFunction sut,
        List<LearnerDataApiResponse> secondPageLearners,
        Exception pageException)
    {
        // Arrange
        var mockFunctionContext = new Mock<FunctionContext>();
        var requestData = new FakeHttpRequestData(mockFunctionContext.Object, new Uri("https://test"));
        
        var secondPageResponse = new GetLearnersApiResponse
        {
            Data = secondPageLearners,
            Page = 2,
            PageSize = 100,
            TotalItems = secondPageLearners.Count,
            TotalPages = 2
        };
        
        var emptyThirdPageResponse = new GetLearnersApiResponse
        {
            Data = new List<LearnerDataApiResponse>(),
            Page = 3,
            PageSize = 100,
            TotalItems = secondPageLearners.Count,
            TotalPages = 2
        };
        
        apiClient.Setup(x => x.GetLearnersAsync(1, 100, true))
            .ThrowsAsync(pageException);
        
        apiClient.Setup(x => x.GetLearnersAsync(2, 100, true))
            .ReturnsAsync(secondPageResponse);
        
        apiClient.Setup(x => x.GetLearnersAsync(3, 100, true))
            .ReturnsAsync(emptyThirdPageResponse);
        
        // Act
        var result = await sut.Run(requestData, mockFunctionContext.Object);
        
        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        functionEndpoint.Verify(x => x.Publish(It.IsAny<LearnerDataEvent>(), It.IsAny<FunctionContext>(), It.IsAny<CancellationToken>()), 
            Times.Exactly(secondPageLearners.Count));
    }

    [Test, MoqAutoData]
    public async Task And_Exact_Batch_Size_Learners_Then_Continue_To_Next_Page(
        [Frozen] Mock<ILearnerDataJobsOuterApi> apiClient,
        [Frozen] Mock<IFunctionEndpoint> functionEndpoint,
        [Frozen] Mock<ILogger<RaiseEventsForExistingLearnersFunction>> logger,
        RaiseEventsForExistingLearnersFunction sut)
    {
        // Arrange
        var firstBatchLearners = new List<LearnerDataApiResponse>();
        for (int i = 0; i < 100; i++)
        {
            firstBatchLearners.Add(new LearnerDataApiResponse { Id = i + 1, Uln = 1000000000 + i });
        }
        
        var secondBatchLearners = new List<LearnerDataApiResponse>();
        for (int i = 0; i < 50; i++)
        {
            secondBatchLearners.Add(new LearnerDataApiResponse { Id = i + 101, Uln = 1000000100 + i });
        }
        
        var mockFunctionContext = new Mock<FunctionContext>();
        var requestData = new FakeHttpRequestData(mockFunctionContext.Object, new Uri("https://test"));
        
        var firstPageResponse = new GetLearnersApiResponse
        {
            Data = firstBatchLearners,
            Page = 1,
            PageSize = 100,
            TotalItems = firstBatchLearners.Count + secondBatchLearners.Count,
            TotalPages = 2
        };
        
        var secondPageResponse = new GetLearnersApiResponse
        {
            Data = secondBatchLearners,
            Page = 2,
            PageSize = 100,
            TotalItems = firstBatchLearners.Count + secondBatchLearners.Count,
            TotalPages = 2
        };
        
        apiClient.Setup(x => x.GetLearnersAsync(1, 100, true))
            .ReturnsAsync(firstPageResponse)
            .Verifiable();
        
        apiClient.Setup(x => x.GetLearnersAsync(2, 100, true))
            .ReturnsAsync(secondPageResponse)
            .Verifiable();
        
        // Act
        var result = await sut.Run(requestData, mockFunctionContext.Object);
        
        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        apiClient.Verify(x => x.GetLearnersAsync(1, 100, true), Times.Once);
        apiClient.Verify(x => x.GetLearnersAsync(2, 100, true), Times.Once);
        functionEndpoint.Verify(x => x.Publish(It.IsAny<LearnerDataEvent>(), It.IsAny<FunctionContext>(), It.IsAny<CancellationToken>()), 
            Times.Exactly(firstBatchLearners.Count + secondBatchLearners.Count));
    }

    [Test, MoqAutoData]
    public async Task And_Api_Throws_Exception_On_Consecutive_Pages_Then_Circuit_Breaker_Triggers_After_5_Errors(
        [Frozen] Mock<ILearnerDataJobsOuterApi> apiClient,
        [Frozen] Mock<IFunctionEndpoint> functionEndpoint,
        [Frozen] Mock<ILogger<RaiseEventsForExistingLearnersFunction>> logger,
        RaiseEventsForExistingLearnersFunction sut,
        Exception apiException)
    {
        // Arrange
        var mockFunctionContext = new Mock<FunctionContext>();
        var requestData = new FakeHttpRequestData(mockFunctionContext.Object, new Uri("https://test"));
        
        for (int i = 1; i <= 5; i++)
        {
            apiClient.Setup(x => x.GetLearnersAsync(i, 100, true))
                .ThrowsAsync(apiException);
        }
        
        apiClient.Setup(x => x.GetLearnersAsync(6, 100, true))
            .ReturnsAsync(new GetLearnersApiResponse { Data = new List<LearnerDataApiResponse>() });
        
        // Act
        var result = await sut.Run(requestData, mockFunctionContext.Object);
        
        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        apiClient.Verify(x => x.GetLearnersAsync(It.IsAny<int>(), 100, true), Times.Exactly(5));
        functionEndpoint.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<FunctionContext>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}