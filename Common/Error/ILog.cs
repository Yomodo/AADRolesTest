//---------------------------------------------------------------------------------------------------------------------
// <copyright file="ILog.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------------------------------------
namespace Common
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Interface for log utility.
    /// </summary>
    public interface ILog
    {
        /// <summary>
        /// Logs the trace message.
        /// </summary>
        /// <param name="level">  The level.</param>
        /// <param name="message">The message.</param>
        /// <param name="tagId">  The tag identifier.</param>
        void LogTraceMessage(TraceLevel level, string message, string tagId = null);

        /// <summary>
        /// Sets a correlation ID used for correlationing common logging messages from the same context (e.g. the same HTTP request). /// The given string should typically be a GUID that is
        /// unique-enough across a sufficient time period of log data.
        /// </summary>
        /// <param name="corrId">The correlation ID</param>
        void SetCorrelationId(Guid corrId);

        /// <summary>
        /// Gets a correlation ID used for correlationing common logging messages from the same context (e.g. the same HTTP request). /// Should return an empty string if no ID is set.
        /// </summary>
        Guid GetCorrelationId();

        /// <summary>
        /// Logs the message as an error
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="tagId">The tag identifier.</param>
        void LogError(string message, string tagId = null);

        /// <summary>
        /// Logs the message as an information.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="tagId">The tag identifier.</param>
        void LogInformation(string message, string tagId = null);

        /// <summary>
        /// Logs the message as a verbose message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="tagId">The tag identifier.</param>
        void LogVerbose(string message, string tagId = null);

        /// <summary>
        /// Logs the message as a warning message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="tagId">The tag identifier.</param>
        void LogWarning(string message, string tagId = null);

        /// <summary>
        /// Logs the message as a critical error.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="tagId">The tag identifier.</param>
        void LogCritical(string message, string tagId = null);
    }
}