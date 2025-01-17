﻿using System.Net;

namespace HttpClientTestProject
{
    internal class StubOkAfterRetryDelegatingHandler : DelegatingHandler
    {
        private int _count = 0;

        private readonly int _failCount;

        private readonly HttpStatusCode _failureCode;

        public StubOkAfterRetryDelegatingHandler(HttpStatusCode failureCode = HttpStatusCode.InternalServerError, int failCount = 1)
        {
            _failureCode = failureCode;
            _failCount = failCount;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var status = _count++ >= _failCount ?
                HttpStatusCode.OK :
                _failureCode;
            var response = new HttpResponseMessage(status);
            return Task.FromResult(response);
        }
    }
}
