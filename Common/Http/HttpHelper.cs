// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="HttpHelper.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

using Newtonsoft.Json;

namespace Common
{
    /// <summary>
    /// Contains a set of helper methods to execute with HttpClient
    /// </summary>
    public class HttpHelper
    {
        private readonly ILog logger;

        public HttpHelper(ILog logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// If set to false, will not print raw http response in Logs
        /// </summary>
        public bool PrintHttpResponse { get; set; } = true;

        /// <summary>
        /// Executes a HttpClient action and casts the returned object to the type specified.
        /// </summary>
        /// <typeparam name="T">The type to which the Http response should be cast to</typeparam>
        /// <param name="client">The client.</param>
        /// <param name="httpClientAction">The HTTP client action.</param>
        /// <param name="expectedStatusCodes">The expected status codes.</param>
        /// <param name="expectedResponse">The expected response.</param>
        /// <returns>The type prepared from the http response</returns>
        /// <exception cref="HttpResponseException"></exception>
        public async Task<T> GetHttpResponseAsync<T>(HttpClient client, Func<HttpClient, Task<HttpResponseMessage>> httpClientAction, HttpStatusCode[] expectedStatusCodes, string expectedResponse = null)
        {
            bool responseReceived = false;
            Exception<ArgumentNullException>.ThrowOn(() => expectedStatusCodes == null);

            try
            {
                using (HttpResponseMessage httpResponseMessage = await this.GetRawHttpResponseAsync(client, httpClientAction))
                {
                    string responseString = (httpResponseMessage.Content != null) ? httpResponseMessage.GetResponseString() : string.Empty;
                    responseReceived = true;

                    if (httpResponseMessage.IsSuccessStatusCode)
                    {
                        if (this.PrintHttpResponse)
                        {
                            this.logger.LogTraceMessage(TraceLevel.Info, $"HttpResponse -{GetFormattedJson(responseString)}");
                        }

                        if ((expectedStatusCodes.Any() && !expectedStatusCodes.Contains(httpResponseMessage.StatusCode)) || (!string.IsNullOrEmpty(expectedResponse) && !responseString.Contains(expectedResponse)))
                        {
                            this.logger.LogWarning($"Unexpected status code or content was received: ResponseStatusCode: '{httpResponseMessage.StatusCode}' and ResponseContent: '{responseString}'. The expected status codes were '[{string.Join(", ", expectedStatusCodes)}]' and content should contain '[{expectedResponse}]'.");
                            throw new HttpResponseException(httpResponseMessage);
                        }

                        Type t = typeof(T);

                        return JsonConvert.DeserializeObject<T>(responseString);
                    }

                    string errormessage = $"Http call failed with response code {httpResponseMessage.StatusCode}. Http response is \n {GetFormattedJson(responseString)} ";
                    this.logger.LogError(errormessage);
                    throw new Exception(errormessage);
                }
            }
            catch (Exception e)
            {
                if (!responseReceived)
                {
                    this.logger.LogTraceMessage(TraceLevel.Error, e.ToString());
                }
                throw;
            }
        }

        /// <summary>
        /// Gets the raw HTTP response from the HttpClient action.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="httpClientAction">The HTTP client action.</param>
        /// <returns>A HttpResponseMessage instance</returns>
        public async Task<HttpResponseMessage> GetRawHttpResponseAsync(HttpClient client, Func<HttpClient, Task<HttpResponseMessage>> httpClientAction)
        {
            HttpResponseMessage httpResponseMessage;

            using (client)
            {
                Stopwatch watch = Stopwatch.StartNew();

                httpResponseMessage = await httpClientAction(client);

                watch.Stop();
                this.logger.LogInformation($"Http call to '{httpResponseMessage.RequestMessage.RequestUri}' finished executing in '{watch.Elapsed.ToPrettyFormat()}'.");
            }

            return httpResponseMessage;
        }


        public static string GetFormattedJson(string responseAsString)
        {
            try
            {
                return JsonConvert.SerializeObject(responseAsString, Formatting.Indented);
            }
            catch (Exception)
            {
                // Not all response is Json.. returned the original string as-is if Json serialization fails..
                return responseAsString;
            }
        }
    }
}