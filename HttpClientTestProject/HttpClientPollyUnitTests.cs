using System.Net;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.RateLimit;
using Polly.Extensions.Http;

namespace HttpClientTestProject;

public class HttpClientPollyUnitTests
{
    private static readonly string TestClient = "TestClient";
    private static readonly string TestUrl = HttpClientHelper.Localhost;

    [TestCase(HttpStatusCode.InternalServerError)]
    public async Task HttpGet_WithRetryPolicy_CheckStatus(HttpStatusCode httpStatusCodeHandledByPolicy)
    {
        // Arrange / With
        bool retryCalled = false;
        var retryPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .RetryAsync(retryCount: 1, onRetry: (_, __) => retryCalled = true);
        var httpClient = HttpClientHelper.CreateStubStatusCodeClient(httpStatusCodeHandledByPolicy);

        // Act
        var response = await retryPolicy.ExecuteAsync(() => httpClient.GetAsync(TestUrl));

        // Assert / Check
        Assert.NotNull(response);
        Assert.True(retryCalled);
        Assert.That(response.StatusCode, Is.EqualTo(httpStatusCodeHandledByPolicy));
    }

    [Test]
    public async Task HttpGet_WithRetryCountPolicy_CheckRetryStatusOk()
    {
        // Arrange / With
        int retryCount = 1;
        bool retryCalled = false;
        var retryPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .RetryAsync(retryCount, onRetry: (_, __) => retryCalled = true);
        var httpClient = HttpClientHelper.CreateStubOkAfterRetryClient(failCount: retryCount);

        // Act
        var response = await retryPolicy.ExecuteAsync(() => httpClient.GetAsync(TestUrl));

        // Assert / Check
        Assert.NotNull(response);
        Assert.True(retryCalled);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task HttpGet_WithRetryCountPolicy_CheckRetryStatusFail()
    {
        // Arrange / With
        int retryCount = 1;
        bool retryCalled = false;
        var retryPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .RetryAsync(retryCount, onRetry: (_, __) => retryCalled = true);
        var httpClient = HttpClientHelper.CreateStubOkAfterRetryClient(failCount: retryCount + 1);

        // Act
        var response = await retryPolicy.ExecuteAsync(() => httpClient.GetAsync(TestUrl));

        // Assert / Check
        Assert.NotNull(response);
        Assert.True(retryCalled);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
    }

    /// <see href="https://github.com/App-vNext/Polly/issues/555#issuecomment-451594435"/>
    [Test]
    public async Task HttpGet_WithWaitAndRetryAndCirguitBreakerPolicy_CheckSucceeds()
    {
        // Arrange / With
        IServiceCollection services = new ServiceCollection();

        int retryCount = 1;
        int handledEventsAllowedBeforeBreaking = 1;
        int numberOfExecutions = 5;
        var rateLimitPolicy = Policy.RateLimitAsync(numberOfExecutions, TimeSpan.FromSeconds(1), numberOfExecutions);

        services.AddHttpClient(TestClient)
            .AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError()
                .Or<RateLimitRejectedException>()
                .WaitAndRetryAsync(retryCount, retryAttempt =>
                {
                    Debug.WriteLine($"Attempting HTTP policy retry #{retryAttempt}");
                    return TimeSpan.MinValue;
                }))
            .AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError()
                .CircuitBreakerAsync(handledEventsAllowedBeforeBreaking, durationOfBreak: TimeSpan.FromTicks(1)))
            .AddPolicyHandler(rateLimitPolicy.AsAsyncPolicy<HttpResponseMessage>())
            .AddHttpMessageHandler(() => new StubHttpStatusCodeDelegatingHandler());

        HttpClient configuredClient = services
            .BuildServiceProvider()
            .GetRequiredService<IHttpClientFactory>()
            .CreateClient(TestClient);

        // Act
        HttpResponseMessage? response = null;
        var stopwatch = Stopwatch.StartNew();
        int retriesAttempted;
        for (retriesAttempted = 1; retriesAttempted <= numberOfExecutions; retriesAttempted++)
        {
            response = await configuredClient.GetAsync(TestUrl);
            TestContext.WriteLine($"HTTP GET retry {retriesAttempted} {response.StatusCode}, {stopwatch.ElapsedMilliseconds}ms");
        }
        stopwatch.Stop();

        // Assert / Check
        Assert.NotNull(response);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(retriesAttempted, Is.EqualTo(numberOfExecutions + retryCount));
    }
}
