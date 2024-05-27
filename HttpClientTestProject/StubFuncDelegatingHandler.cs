using System.Net;

namespace HttpClientTestProject
{
    internal sealed class StubFuncDelegatingHandler : DelegatingHandler
    {
        private static readonly Task<HttpResponseMessage> _okResponse = Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handlerFunc;

        public StubFuncDelegatingHandler() =>
            _handlerFunc = (request, cancellationToken) => _okResponse;

        public StubFuncDelegatingHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handlerFunc) =>
            _handlerFunc = handlerFunc;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            _handlerFunc(request, cancellationToken);
    }
}
