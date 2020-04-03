// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="HttpHeaders.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Common
{
    /// <summary>
    /// Http headers constants
    /// </summary>
    public static class HttpHeaders
    {
        /// <summary>
        /// Http Request Header that specifies authorization token.
        /// </summary>
        public const string Authorization = "Authorization";

        /// <summary>
        /// The correlation identifier
        /// </summary>
        public const string CorrelationId = "CorrelationId";

        /// <summary>
        /// The client request identifier
        /// </summary>
        public const string ClientRequestId = "client-request-id";

        /// <summary>
        /// This header can be included in the synthetic request to indicate that.
        /// </summary>
        public const string SyntheticRequest = "stx";

        // Consistent with Azure
        public const string TrackingId = "x-ms-request-id";

        /// <summary>
        /// The total count header for Json responses
        /// </summary>
        public const string TotalCount = "X-total-count";

        /// <summary>
        /// The content type options
        /// </summary>
        public const string ContentTypeOptions = "X-Content-Type-Options";

        /// <summary>
        /// The strict transport security header
        /// </summary>
        public const string StrictTransportSecurity = "Strict-Transport-Security";
    }
}