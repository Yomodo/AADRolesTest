// --------------------------------------------------------------------------------------
// <copyright file="IHttpClient.cs" company="Microsoft">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------

namespace Common
{
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// A wrapper around the HttpClient mainly used for Dependency Injection
    /// purpose across several classes which take dependency on the HttpClient
    /// </summary>
    public interface IHttpClient
    {
        /// <summary>
        /// Send a request with the specified HttpRequestMessage 
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The HttpResponseMessage</returns>
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
    }
}
