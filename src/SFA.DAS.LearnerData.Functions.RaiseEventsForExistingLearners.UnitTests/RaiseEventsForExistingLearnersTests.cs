using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.LearnerData.Application.OuterApi;
using SFA.DAS.LearnerData.Application.OuterApi.Responses;
using SFA.DAS.LearnerData.Functions.RaiseEventsForExistingLearners.Functions;
using SFA.DAS.Testing.AutoFixture;
using System.Net;
using SFA.DAS.LearnerData.Functions.RaiseEventsForExistingLearners.UnitTests.Helpers;

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
}