using System.Net;

namespace HttpClientTestProject
{
    internal class StubOkAfterRetryDelegatingHandler : DelegatingHandler
    {
        private int _count = 0;

        private readonly int _failCount;

        public StubOkAfterRetryDelegatingHandler(int failCount) => _failCount = failCount;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var status = _count++ >= _failCount ?
                HttpStatusCode.OK :
                HttpStatusCode.InternalServerError;
            var response = new HttpResponseMessage(status);
            return Task.FromResult(response);
        }
    }
}
