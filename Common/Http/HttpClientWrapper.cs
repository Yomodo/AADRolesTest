// --------------------------------------------------------------------------------------
// <copyright file="HttpClientHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------

namespace Common
{
    using System.Net.Http;
    using System.Threading.Tasks;

    public class HttpClientWrapper : IHttpClient
    {
        // The HttpClient object
        public readonly HttpClient _client;

        public HttpClientWrapper(HttpClient client)
        {
            this._client = client;
        }

        /// <summary>
        /// Send the HttpRequest as a asynchronous operation
        /// </summary>
        /// <param name="request">The HttpRequestMessage</param>
        /// <returns>HttpResponseObject</returns>
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return await _client.SendAsync(request);
        }
    }
}
