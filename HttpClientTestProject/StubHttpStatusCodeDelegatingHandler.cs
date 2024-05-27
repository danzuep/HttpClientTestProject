using System.Net;

namespace HttpClientTestProject
{
    internal sealed class StubHttpStatusCodeDelegatingHandler : DelegatingHandler
    {
        private static readonly Task<HttpResponseMessage> _okResponse = Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        private readonly HttpStatusCode _httpStatusCode;

        public StubHttpStatusCodeDelegatingHandler(HttpStatusCode httpStatusCodeHandledByPolicy = HttpStatusCode.OK) => _httpStatusCode = httpStatusCodeHandledByPolicy;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            _httpStatusCode == HttpStatusCode.OK ? _okResponse : Task.FromResult(new HttpResponseMessage(_httpStatusCode));
    }
}
