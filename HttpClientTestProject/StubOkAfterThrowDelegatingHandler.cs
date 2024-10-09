using System.Net;

namespace HttpClientTestProject
{
    internal class StubOkAfterThrowDelegatingHandler : DelegatingHandler
    {
        private int _count = 0;

        private readonly int _failCount;

        public StubOkAfterThrowDelegatingHandler(int failCount = 1)
        {
            _failCount = failCount;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var status = _count++ >= _failCount ?
                HttpStatusCode.OK :
                throw new HttpRequestException();
            var response = new HttpResponseMessage(status);
            return Task.FromResult(response);
        }
    }
}
