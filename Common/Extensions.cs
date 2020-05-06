extern alias BetaLib;

// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="Extensions.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Beta = BetaLib.Microsoft.Graph;

namespace Common
{
    using Microsoft.Graph;
    using System;

    /// <summary>
    /// Static class for general extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Adds a LINQ support for ToHashSet method.
        /// </summary>
        /// <typeparam name="T">The type of object in hash set.</typeparam>
        /// <param name="source">The IEnumerable to convert to a hash set.</param>
        /// <returns>HashSet of items in source.</returns>
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }

        /// <summary>
        /// Enumerates over an IEnumerable and performs the specified action on each element
        /// </summary>
        /// <typeparam name="T">The type param of the IEnumerable</typeparam>
        /// <param name="source">The IEnumerable to enumerate</param>
        /// <param name="action">The action to perform on each element in the IEnumerable</param>
        /// <returns>The original IEnumerable</returns>
        public static async Task ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                await Task.Run(() => { action(item); }).ConfigureAwait(false);
            }

        }

        public static async Task ForEachAsync<T>(this List<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
                await Task.Run(() => { action(item); }).ConfigureAwait(false);
        }

        /// <summary>
        /// Split an IEnumerable<T> into fixed-sized chunks (return an IEnumerable<IEnumerable<T>> where the inner sequences are of fixed
        /// https://stackoverflow.com/questions/13709626/split-an-ienumerablet-into-fixed-sized-chunks-return-an-ienumerableienumerab
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="batchSize"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
        {
            using (var enumerator = source.GetEnumerator())
                while (enumerator.MoveNext())
                    yield return YieldBatchElements(enumerator, batchSize - 1);
        }

        private static IEnumerable<T> YieldBatchElements<T>(IEnumerator<T> source, int batchSize)
        {
            yield return source.Current;
            for (int i = 0; i < batchSize && source.MoveNext(); i++)
                yield return source.Current;
        }

        public static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Removes extra spaces in the string provided.
        /// </summary>
        /// <param name="str">The string to clean.</param>
        /// <returns>A string without the extra spaces.</returns>
        public static string RemoveExtraSpaces(this string str)
        {
            return Regex.Replace(str, @"\s+", " ", RegexOptions.Multiline);
        }

        /// <summary>
        /// Removes the line breaks in the string provided.
        /// </summary>
        /// <param name="str">The string to work on.</param>
        /// <returns>A string without any line breaks</returns>
        public static string RemoveLineBreaks(this string str)
        {
            return str.Replace(Environment.NewLine, string.Empty);
        }

        /// <summary>
        /// Prints a timespan in a pretty format
        /// </summary>
        /// <param name="span">The TimeSpan.</param>
        /// <returns>A string with formatted timespan</returns>
        public static string ToPrettyFormat(this TimeSpan span)
        {
            var sb = new StringBuilder();
            if (span.Days > 0)
            {
                sb.AppendFormat("{0} day{1} ", span.Days, span.Days > 1 ? "s" : string.Empty);
            }

            if (span.Hours > 0)
            {
                sb.AppendFormat("{0} hour{1} ", span.Hours, span.Hours > 1 ? "s" : string.Empty);
            }

            if (span.Minutes > 0)
            {
                sb.AppendFormat("{0} minute{1} ", span.Minutes, span.Minutes > 1 ? "s" : string.Empty);
            }

            if (span.Seconds > 0)
            {
                sb.AppendFormat("{0} second{1} ", span.Seconds, span.Seconds > 1 ? "s" : string.Empty);
            }

            if (span.Milliseconds > 0)
            {
                sb.AppendFormat("{0} millisecond{1} ", span.Milliseconds, span.Milliseconds > 1 ? "s" : string.Empty);
            }

            return sb.ToString();
        }

        public static string GetResponseString(this HttpResponseMessage response)
        {
            if (response != null && response.Content != null)
            {
                return response.Content.ReadAsStringAsync().Result;
            }

            return null;
        }

        public static async Task<HttpResponseMessage> PatchAsync(this HttpClient client, string requestUrl, HttpContent content)
        {
            return await PatchAsync(client, new Uri(requestUrl), content);
        }

        public static async Task<HttpResponseMessage> PatchAsync(this HttpClient client, Uri requestUri, HttpContent content)
        {
            return await PatchAsync(client, requestUri, content, new CancellationToken(false));
        }

        public static async Task<HttpResponseMessage> PatchAsync(this HttpClient client, Uri requestUri, HttpContent content, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri)
            {
                Content = content
            };
            return await client.SendAsync(request, cancellationToken);
        }

        public static async Task AuthenticateClient(this IAuthenticationProvider authenticationProvider, HttpClient client)
        {
            HttpRequestMessage message = new HttpRequestMessage();
            await authenticationProvider.AuthenticateRequestAsync(message);
            client.DefaultRequestHeaders.Authorization = message.Headers.Authorization;
        }

        public static T ToEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> of a provided Dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public static string ToPrintableString<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null || dictionary.Count == 0)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();

            dictionary.ToList().ForEach((pair) => sb.AppendLine($"{pair.Key}-{pair.Value}"));

            return sb.ToString();
        }

        public static async Task<string> ToString<T>(this IList<T> list)
        {
            if (list == null || list.Count == 0)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();

            await list.ForEach((item) => sb.AppendLine($"{item}"));

            return sb.ToString();
        }

        public static string ProcessHttpResponse(this HttpResponseMessage httpResponseMessage)
        {
            using (httpResponseMessage)
            {
                string responseString = (httpResponseMessage.Content != null) ? httpResponseMessage.GetResponseString() : string.Empty;

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    Console.WriteLine($"HttpResponse -{HttpHelper.GetFormattedJson(responseString)}");
                    return responseString;
                }
                else
                {
                    ColorConsole.WriteLine(ConsoleColor.Red, $"Http call failed with response code {httpResponseMessage.StatusCode}. Http response is \n {HttpHelper.GetFormattedJson(responseString)}");
                }
            }

            return string.Empty;
        }

        public static async Task<HttpClient> GetHttpClientForMSGraphAsync(this Beta.GraphServiceClient graphServiceClient)
        {
            HttpClient httpClient = new HttpClient();

            await graphServiceClient.AuthenticationProvider.AuthenticateClient(httpClient);

            return httpClient;
        }

        //public static async Task<HttpClient> GetHttpClientForMSGraphAsync(this GraphServiceClient graphServiceClient)
        //{
        //    HttpClient httpClient = new HttpClient();

        //    await graphServiceClient.AuthenticationProvider.AuthenticateClient(httpClient);

        //    return httpClient;
        //}

       
    }
}