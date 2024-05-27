using FakeItEasy;
using System.Net;
using System.Reflection;
using System.Text.Json;

namespace HttpClientTestProject;

internal static class HttpClientHelper
{
    internal static readonly string Localhost = "https://localhost";

    public static HttpMessageHandler CreateMessageHandler<T>(T result, HttpStatusCode code = HttpStatusCode.OK)
    {
        var messageHandler = A.Fake<HttpMessageHandler>();
        var content = JsonSerializer.Serialize(result);
        var response = new HttpResponseMessage
        {
            StatusCode = code,
            Content = new StringContent(content)
        };
        A.CallTo(messageHandler)
            .Where(call => call.Method.Name == "Send")
            .WithReturnType<HttpResponseMessage>()
            .Returns(response);
        A.CallTo(messageHandler)
            .Where(call => call.Method.Name == "SendAsync")
            .WithReturnType<Task<HttpResponseMessage>>()
            .Returns(Task.FromResult(response));
        return messageHandler;
    }

    public static HttpClient CreateHttpClient(HttpMessageHandler httpMessageHandler, string? baseUrl = null)
    {
        var httpClient = new HttpClient(httpMessageHandler)
        {
            BaseAddress = new(baseUrl ?? Localhost)
        };
        return httpClient;
    }

    public static HttpClient CreateStubStatusCodeClient(HttpStatusCode code = HttpStatusCode.OK, string? baseUrl = null)
    {
        var stubHttpMessageHandler = new StubHttpStatusCodeDelegatingHandler(code);
        var httpClient = CreateHttpClient(stubHttpMessageHandler, baseUrl);
        return httpClient;
    }

    public static HttpClient CreateStubOkAfterRetryClient(int failCount = 1, string? baseUrl = null)
    {
        var stubHttpMessageHandler = new StubOkAfterRetryDelegatingHandler(failCount);
        var httpClient = CreateHttpClient(stubHttpMessageHandler, baseUrl);
        return httpClient;
    }

    public static HttpClient CreateStubDelegatingClient(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handlerFunc, string? baseUrl = null)
    {
        var stubHttpMessageHandler = new StubFuncDelegatingHandler(handlerFunc);
        var httpClient = CreateHttpClient(stubHttpMessageHandler, baseUrl);
        return httpClient;
    }
}