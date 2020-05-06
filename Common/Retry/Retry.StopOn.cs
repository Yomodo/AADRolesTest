// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="Retry.StopOn.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Common
{
    using System;


    /// <summary>
    /// These methods are the retry flavors that focus on performing a retry ONLY when a certain Exception type is NOT encountered. In other words, a retry occurs unconditionally unless a known
    /// Exception, provided by the client, is encountered.
    /// </summary>
    public partial class Retry
    {
        /// <summary>
        /// This is the no-parameter flavor. <see cref="WithExpBackoff_StopOn{Arg1T, RsltT}(Func{Arg1T, Task{RsltT}}, Arg1T, ISet{Type}, int, int)"/>
        /// </summary>
        public static async Task<TrialResults<RsltT>> WithExpBackoff_StopOn<RsltT>(
            Func<Task<RsltT>> methodToRetry,
            ISet<Type> stopRetryFailures = null,
            int maxRetries = MaxRetries,
            int initialSleepMillis = initleepInMs)
        {
            TrialResults<RsltT> results = new TrialResults<RsltT>();

            for (int trialCount = 1;; ++trialCount)
            {
                var stopwatch = Stopwatch.StartNew();
                try
                {
                    results.Result = await methodToRetry();
                    return results;
                }
                catch (Exception e)
                {
                    EvaluateCurrentState(
                        e, results, null, stopRetryFailures, trialCount, maxRetries, initialSleepMillis);
                }
                finally
                {
                    stopwatch.Stop();

                    // Don't add "latency" for MaxRetriesException occurring!
                    if (trialCount <= maxRetries)
                    {
                        results.Latencies.Add(TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds));
                    }
                }
            }
        }

        /// <summary>
        /// Executes the given method x "maxRetries" times. Uses exponential backoff to sleep between calls to the method. If an exception is thrown that is in stopRetryFailures, it will be rethrown.
        /// If the max number of retries are reached, a MaxRetriesReachedException is thrown. In either case, if an exception is bubbled-up to the client, the wrapped result (i.e. TrialResult.Result)
        /// should be considered uninitialized. This is the 1-parameter flavor
        /// </summary>
        /// <typeparam name="Arg1T">Type of First arg to the method</typeparam>
        /// <typeparam name="RsltT">Result type of the method</typeparam>
        /// <param name="methodToRetry">     The method to retry.</param>
        /// <param name="arg1">              The arg1.</param>
        /// <param name="stopRetryFailures"> Exceptions types that will cause retrying to stop.</param>
        /// <param name="maxRetries">        Max number of retries</param>
        /// <param name="initialSleepMillis">Initial sleep value, in milliseconds</param>
        /// <returns></returns>
        public static async Task<TrialResults<RsltT>> WithExpBackoff_StopOn<Arg1T, RsltT>(
            Func<Arg1T, Task<RsltT>> methodToRetry,
            Arg1T arg1,
            ISet<Type> stopRetryFailures = null,
            int maxRetries = MaxRetries,
            int initialSleepMillis = initleepInMs)
        {
            TrialResults<RsltT> results = new TrialResults<RsltT>();

            for (int trialCount = 1;; ++trialCount)
            {
                var stopwatch = Stopwatch.StartNew();
                try
                {
                    results.Result = await methodToRetry(arg1);
                    return results;
                }
                catch (Exception e)
                {
                    EvaluateCurrentState(
                        e, results, null, stopRetryFailures, trialCount, maxRetries, initialSleepMillis);
                }
                finally
                {
                    stopwatch.Stop();

                    // Don't add "latency" for MaxRetriesException occurring!
                    if (trialCount <= maxRetries)
                    {
                        results.Latencies.Add(TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds));
                    }
                }
            }
        }

        /// <summary>
        /// This is the 2-parameter flavor. <see cref="WithExpBackoff_StopOn{Arg1T, RsltT}(Func{Arg1T, Task{RsltT}}, Arg1T, ISet{Type}, int, int)"/>
        /// </summary>
        public static async Task<TrialResults<RsltT>> WithExpBackoff_StopOn<Arg1T, Arg2T, RsltT>(
            Func<Arg1T, Arg2T, Task<RsltT>> methodToRetry,
            Arg1T arg1,
            Arg2T arg2,
            ISet<Type> stopRetryFailures = null,
            int maxRetries = MaxRetries,
            int initialSleepMillis = initleepInMs)
        {
            TrialResults<RsltT> results = new TrialResults<RsltT>();

            for (int trialCount = 1;; ++trialCount)
            {
                var stopwatch = Stopwatch.StartNew();
                try
                {
                    results.Result = await methodToRetry(arg1, arg2);
                    return results;
                }
                catch (Exception e)
                {
                    EvaluateCurrentState(
                        e, results, null, stopRetryFailures, trialCount, maxRetries, initialSleepMillis);
                }
                finally
                {
                    stopwatch.Stop();

                    // Don't add "latency" for MaxRetriesException occurring!
                    if (trialCount <= maxRetries)
                    {
                        results.Latencies.Add(TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds));
                    }
                }
            }
        }

        /// <summary>
        /// This is the 3-parameter flavor. <see cref="WithExpBackoff_StopOn{Arg1T, RsltT}(Func{Arg1T, Task{RsltT}}, Arg1T, ISet{Type}, int, int)"/>
        /// </summary>
        public static async Task<TrialResults<RsltT>> WithExpBackoff_StopOn<Arg1T, Arg2T, Arg3T, RsltT>(
            Func<Arg1T, Arg2T, Arg3T, Task<RsltT>> methodToRetry,
            Arg1T arg1,
            Arg2T arg2,
            Arg3T arg3,
            ISet<Type> stopRetryFailures = null,
            int maxRetries = MaxRetries,
            int initialSleepMillis = initleepInMs)
        {
            TrialResults<RsltT> results = new TrialResults<RsltT>();

            for (int trialCount = 1;; ++trialCount)
            {
                var stopwatch = Stopwatch.StartNew();
                try
                {
                    results.Result = await methodToRetry(arg1, arg2, arg3);
                    return results;
                }
                catch (Exception e)
                {
                    EvaluateCurrentState(
                        e, results, null, stopRetryFailures, trialCount, maxRetries, initialSleepMillis);
                }
                finally
                {
                    stopwatch.Stop();

                    // Don't add "latency" for MaxRetriesException occurring!
                    if (trialCount <= maxRetries)
                    {
                        results.Latencies.Add(TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds));
                    }
                }
            }
        }

        /// <summary>
        /// This is the 4-parameter flavor. <see cref="WithExpBackoff_StopOn{Arg1T, RsltT}(Func{Arg1T, Task{RsltT}}, Arg1T, ISet{Type}, int, int)"/>
        /// </summary>
        public static async Task<TrialResults<RsltT>> WithExpBackoff_StopOn<Arg1T, Arg2T, Arg3T, Arg4T, RsltT>(
            Func<Arg1T, Arg2T, Arg3T, Arg4T, Task<RsltT>> methodToRetry,
            Arg1T arg1,
            Arg2T arg2,
            Arg3T arg3,
            Arg4T arg4,
            ISet<Type> stopRetryFailures = null,
            int maxRetries = MaxRetries,
            int initialSleepMillis = initleepInMs)
        {
            TrialResults<RsltT> results = new TrialResults<RsltT>();

            for (int trialCount = 1;; ++trialCount)
            {
                var stopwatch = Stopwatch.StartNew();
                try
                {
                    results.Result = await methodToRetry(arg1, arg2, arg3, arg4);
                    return results;
                }
                catch (Exception e)
                {
                    EvaluateCurrentState(
                        e, results, null, stopRetryFailures, trialCount, maxRetries, initialSleepMillis);
                }
                finally
                {
                    stopwatch.Stop();

                    // Don't add "latency" for MaxRetriesException occurring!
                    if (trialCount <= maxRetries)
                    {
                        results.Latencies.Add(TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds));
                    }
                }
            }
        }

        /// <summary>
        /// No param, convenience method that takes just a single Exception instead of a set.
        /// </summary>
        /// <see cref="WithExpBackoff_StopOn{Arg1T, RsltT}(Func{Arg1T, Task{RsltT}}, Arg1T, ISet{Type}, int, int)"/>
        public static async Task<TrialResults<RsltT>> WithExpBackoff_StopOn<RsltT>(
            Func<Task<RsltT>> methodToRetry,
            Type stopRetryFailure,
            int maxRetries = MaxRetries,
            int initialSleepMillis = initleepInMs)
        {
            return await WithExpBackoff_StopOn(
                       methodToRetry, new HashSet<Type> {stopRetryFailure}, maxRetries, initialSleepMillis);
        }

        /// <summary>
        /// One param, convenience method that takes just a single Exception instead of a set.
        /// </summary>
        /// <see cref="WithExpBackoff_StopOn{Arg1T, RsltT}(Func{Arg1T, Task{RsltT}}, Arg1T, ISet{Type}, int, int)"/>
        public static async Task<TrialResults<RsltT>> WithExpBackoff_StopOn<Arg1T, RsltT>(
            Func<Arg1T, Task<RsltT>> methodToRetry,
            Arg1T arg1,
            Type stopRetryFailure,
            int maxRetries = MaxRetries,
            int initialSleepMillis = initleepInMs)
        {
            return await WithExpBackoff_StopOn(
                       methodToRetry, arg1, new HashSet<Type> {stopRetryFailure}, maxRetries, initialSleepMillis);
        }

        /// <summary>
        /// Two param, convenience method that takes just a single Exception instead of a set.
        /// </summary>
        /// <see cref="WithExpBackoff_StopOn{Arg1T, RsltT}(Func{Arg1T, Task{RsltT}}, Arg1T, ISet{Type}, int, int)"/>
        public static async Task<TrialResults<RsltT>> WithExpBackoff_StopOn<Arg1T, Arg2T, RsltT>(
            Func<Arg1T, Arg2T, Task<RsltT>> methodToRetry,
            Arg1T arg1,
            Arg2T arg2,
            Type stopRetryFailure,
            int maxRetries = MaxRetries,
            int initialSleepMillis = initleepInMs)
        {
            return await WithExpBackoff_StopOn(
                       methodToRetry, arg1, arg2, new HashSet<Type> {stopRetryFailure}, maxRetries, initialSleepMillis);
        }

        /// <summary>
        /// Three param, convenience method that takes just a single Exception instead of a set.
        /// </summary>
        /// <see cref="WithExpBackoff_StopOn{Arg1T, RsltT}(Func{Arg1T, Task{RsltT}}, Arg1T, ISet{Type}, int, int)"/>
        public static async Task<TrialResults<RsltT>> WithExpBackoff_StopOn<Arg1T, Arg2T, Arg3T, RsltT>(
            Func<Arg1T, Arg2T, Arg3T, Task<RsltT>> methodToRetry,
            Arg1T arg1,
            Arg2T arg2,
            Arg3T arg3,
            Type stopRetryFailure,
            int maxRetries = MaxRetries,
            int initialSleepMillis = initleepInMs)
        {
            return await WithExpBackoff_StopOn(
                       methodToRetry, arg1, arg2, arg3, new HashSet<Type> {stopRetryFailure}, maxRetries, initialSleepMillis);
        }

        /// <summary>
        /// Four param, convenience method that takes just a single Exception instead of a set.
        /// </summary>
        /// <see cref="WithExpBackoff_StopOn{Arg1T, RsltT}(Func{Arg1T, Task{RsltT}}, Arg1T, ISet{Type}, int, int)"/>
        public static async Task<TrialResults<RsltT>> WithExpBackoff_StopOn<Arg1T, Arg2T, Arg3T, Arg4T, RsltT>(
            Func<Arg1T, Arg2T, Arg3T, Arg4T, Task<RsltT>> methodToRetry,
            Arg1T arg1,
            Arg2T arg2,
            Arg3T arg3,
            Arg4T arg4,
            Type stopRetryFailure,
            int maxRetries = MaxRetries,
            int initialSleepMillis = initleepInMs)
        {
            return await WithExpBackoff_StopOn(
                       methodToRetry, arg1, arg2, arg3, arg4, new HashSet<Type> {stopRetryFailure}, maxRetries, initialSleepMillis);
        }

        /// <summary>
        /// This is a specialized overloaded method that allows clients to provide a PREDICATE for testing if we should stop retrying based on a given Exception.
        /// </summary>
        public static async Task<TrialResults<RsltT>> WithExpBackoff_StopOn<RsltT>(
            Func<Task<RsltT>> methodToRetry,
            Predicate<Exception> shouldStopOn = null,
            int maxRetries = MaxRetries,
            int initialSleepMillis = initleepInMs)
        {
            TrialResults<RsltT> results = new TrialResults<RsltT>();

            for (int trialCount = 1;; ++trialCount)
            {
                var stopwatch = Stopwatch.StartNew();
                try
                {
                    results.Result = await methodToRetry();
                    return results;
                }
                catch (Exception e)
                {
                    if (shouldStopOn(e))
                    {
                        throw e;
                    }

                    EvaluateCurrentState(
                        e, results, null, null, trialCount, maxRetries, initialSleepMillis);
                }
                finally
                {
                    stopwatch.Stop();

                    // Don't add "latency" for MaxRetriesException occurring!
                    if (trialCount <= maxRetries)
                    {
                        results.Latencies.Add(TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds));
                    }
                }
            }
        }
    }
}