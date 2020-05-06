using System;
using System.Collections.Generic;

namespace Common
{
    /// <summary>
    ///     A basic wrapper around the results of a "Retry"-able call. The "result" is the actual return value from a method
    ///     being retried, and "latencies" stores latency values.
    /// </summary>
    /// <typeparam name="RsltT">The type of the result.</typeparam>
    public class TrialResults<RsltT>
    {
        /// <summary>
        ///     Gets or sets the result.
        /// </summary>
        /// <value>
        ///     The result.
        /// </value>
        public RsltT Result { get; set; }

        /// <summary>
        ///     Gets or sets the latencies.
        /// </summary>
        /// <value>
        ///     The latencies.
        /// </value>
        public IList<TimeSpan> Latencies { get; set; } = new List<TimeSpan>();
    }
}