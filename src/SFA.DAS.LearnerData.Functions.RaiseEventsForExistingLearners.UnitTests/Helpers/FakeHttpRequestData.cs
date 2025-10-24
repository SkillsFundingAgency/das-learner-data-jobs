using System.Security.Claims;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace SFA.DAS.LearnerData.Functions.RaiseEventsForExistingLearners.UnitTests.Helpers;

public class FakeHttpRequestData(FunctionContext functionContext, Uri url, Stream? body = null)
    : HttpRequestData(functionContext)
{
    public override HttpResponseData CreateResponse()
    {
        return new FakeHttpResponseData(FunctionContext);
    }

    public override Stream Body { get; } = body ?? new MemoryStream();
    public override HttpHeadersCollection Headers { get; } = [];
    public override IReadOnlyCollection<IHttpCookie> Cookies { get; } = new List<IHttpCookie>();
    public override Uri Url { get; } = url;
    public override IEnumerable<ClaimsIdentity> Identities { get; } = new List<ClaimsIdentity>();
    public override string Method { get; } = "GET";
}