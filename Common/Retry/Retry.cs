// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="Retry.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Common
{
    /// <summary>
    /// This class contains a series of methods for calling a delegate/function pointer and potentially retrying the call when certain Exceptions are thrown. Since the "Func" delegate has to be
    /// strongly defined, the public interface comes in very similar {0,1,2...} parameter flavors corresponding to the parameter count of the "Func" method.
    /// </summary>
    public partial class Retry
    {
        private const int initleepInMs = 1000;
        private const int MaxRetries = 5;

        /// <summary>
        /// Tests the given non-null exception "e" against one of either of the sets provided. Returns TRUE to indicate that retrying should occur again. If the trial index is &gt; maxRetries, false is
        /// returned. Only ONE of retryOnTheseFailures or stopRetryFailures should be provided. The other should be null. If retryOnTheseFailures is provided:
        /// - Returns true only if "e" is in the set. Otherwise "e" is thrown. If stopRetryFailures is provided:
        /// - Returns true only if "e" is NOT in the set. Otherwise "e" is thrown. Prior to returning TRUE, this method will perform an exponentially- increasing sleep based on the initial sleep value
        /// and the maxRetries. A jitter is also introduced to account for potentially-synchronized requests that could be taken out of sequence thanks to this jitter.
        /// </summary>
        /// <typeparam name="RsltT">The type of the SLT t.</typeparam>
        /// <param name="e">                   A raised exception</param>
        /// <param name="results">             The results.</param>
        /// <param name="retryOnTheseFailures">Exceptions types that will cause retrying to continue.</param>
        /// <param name="stopRetryFailures">   Exceptions types that will cause retrying to stop.</param>
        /// <param name="trialIdx">            Trial number, 1-index.</param>
        /// <param name="maxRetries">          Max number of retries</param>
        /// <param name="initialSleepMillis">  Initial sleep value, in milliseconds</param>
        /// <returns></returns>
        /// <exception cref="DcOpsServiceUtility.Exceptions.MaxRetriesReachedException">MaxRetriesReached Exception</exception>
        private static bool EvaluateCurrentState<RsltT>(
            Exception e,
            TrialResults<RsltT> results,
            ISet<Type> retryOnTheseFailures,
            ISet<Type> stopRetryFailures,
            int trialIdx,
            int maxRetries,
            int initialSleepMillis)
        {
            // These both cannot be non-null.
            if (retryOnTheseFailures != null && stopRetryFailures != null)
            {
                Exception<UnexpectedException>.Throw("Unexpected situation during a retry.");
            }

            if (stopRetryFailures != null && stopRetryFailures.Contains(e.GetType()))
            {
                // Considered a "real" exception
                throw e;
            }

            if (retryOnTheseFailures != null && !retryOnTheseFailures.Contains(e.GetType()))
            {
                // Considered a "real" exception
                throw e;
            }

            // At this point, we know the given exception is something that should cause a retry. Should we retry or have we reached the retry limit?
            if (trialIdx < maxRetries)
            {
                var sleepTimeMs = (int) Math.Pow(2.0, trialIdx - 1) * initialSleepMillis;
                var randomJitter = new Random().Next(1, 6);
                System.Threading.Thread.Sleep(sleepTimeMs + randomJitter);
                return true;
            }

            throw new MaxRetriesReachedException(maxRetries, results.Latencies);
        }

        /// <summary>
        /// A basic wrapper around the results of a "Retry"-able call. The "result" is the actual return value from a method being retried, and "latencies" stores latency values.
        /// </summary>
        /// <typeparam name="RsltT">result </typeparam>
        public class TrialResults<RsltT>
        {
            public IList<TimeSpan> Latencies = new List<TimeSpan>();
            public RsltT Result;
        }

        /// <summary>
        /// A basic wrapper around the results of a "Retry"-able call. "latencies" stores latency values.
        /// </summary>
        public class TrialResults : TrialResults<Object>
        {
            
        }
    }
}