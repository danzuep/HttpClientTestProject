using System.Net;
using System.Text.Json;

namespace HttpClientTestProject
{
    internal sealed class StubDelegatingHandler : DelegatingHandler
    {
        private static readonly HttpStatusCode[] _transientErrors =
            [ HttpStatusCode.RequestTimeout, HttpStatusCode.ServiceUnavailable ];

        private int _callCount = 0;

        private readonly HttpContent _httpContent;

        public StubDelegatingHandler(HttpContent httpContent) =>
            _httpContent = httpContent;

        public static StubDelegatingHandler Create<T>(T response)
        {
            var serializedContent = JsonSerializer.Serialize(response);
            var stringContent = new StringContent(serializedContent);
            var handler = new StubDelegatingHandler(stringContent);
            return handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpStatusCode = _callCount < _transientErrors.Length ?
                _transientErrors[_callCount++] : HttpStatusCode.OK;
            var response = new HttpResponseMessage(httpStatusCode);
            response.Content = _httpContent;
            return Task.FromResult(response);
        }
    }
}
