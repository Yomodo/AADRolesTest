// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MaxRetriesReachedException.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Common
{
    using System;

    /// <summary>
    /// A simple Exception type that is used to indicate to clients of the "Retry" class that the max number of retry attempts has been reached.
    ///
    /// Clients can access the threshold value in addition to having a pre-populated Message() string.
    /// </summary>
    public class MaxRetriesReachedException : TimeoutException
    {
        /// <summary>
        /// Parameterless constructor. Give a generic "max retries met" message, Set the MaxRetriesValue property to -1.
        /// </summary>
        public MaxRetriesReachedException()
            :
            base("The maximum number of retries were met for a particular operation")
        {
            MaxRetriesValue = -1;
            Latencies = new List<TimeSpan>();
        }

        /// <summary>
        /// Construct the exception with the given maximum threshold that was reached. This sets a default Exception.message that indicates that the maximum number of retries was reached and provides
        /// the given value in the message.
        /// </summary>
        /// <param name="maxValue"> The threshold that caused the exception</param>
        /// <param name="latencies">The latencies.</param>
        public MaxRetriesReachedException(int maxValue, IList<TimeSpan> latencies)
            : base($"The maximum number of retries were met ({maxValue}).")
        {
            MaxRetriesValue = maxValue;
            Latencies = latencies;
        }

        public int MaxRetriesValue { get; }

        public IList<TimeSpan> Latencies { get; }
    }
}