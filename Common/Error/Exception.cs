// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="Exception.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Common
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// This is our main class used for throwing exceptions. It represents a common point for us to handling logging of errors. The typical way of using this class is:
    ///
    /// Exception <see cref="ArgumentException"/>.throw("foo", Logger)
    ///
    /// Clients have the option of providing an ILog to perform consistent logging.
    /// </summary>
    public static class Exception<TException> where TException : Exception, new()
    {
        /// <summary>
        /// Throw a new exception of the given type with a given inner exception If the logger is provided, a logging message will occur
        /// </summary>
        public static void Throw(
            string message,
            Exception innerException,
            ILog logger = null)
        {
            ThrowInternal(logger, message, innerException);
        }

        /// <summary>
        /// Throw a new exception of the given type. If the logger is provided, a logging message will occur.
        /// </summary>
        public static void Throw(
            string message = null,
            ILog logger = null)
        {
            ThrowInternal(logger, message);
        }

        /// <summary>
        /// Throw a new exception of the given type. This variable parameter flavor will allow you to provide dynamic args to the Exception type being created in the event that the Exception type's
        /// constructor doesn't follow the typical (string message, Exception innerException) ordering.
        ///
        /// For example, the MaxRetriesException receives args as (int, List).
        /// </summary>
        public static void Throw(
            ILog logger,
            params object[] args)
        {
            ThrowInternal(logger, args);
        }

        /// <summary>
        /// Throw a new exception of the given type with a given inner exception If the logger is provided, a logging message will occur.
        ///
        /// This is a predicate flavor in which the predicate is evaluated, and if true, the exception will be thrown.
        /// </summary>
        public static void ThrowOn(
            Func<bool> predicate,
            string message,
            Exception innerException,
            ILog logger = null)
        {
            if (predicate())
            {
                ThrowInternal(logger, message, innerException);
            }
        }

        /// <summary>
        /// Throw a new exception of the given type. If the logger is provided, a logging message will occur.
        ///
        /// This is a predicate flavor in which the predicate is evaluated, and if true, the exception will be thrown.
        /// </summary>
        public static void ThrowOn(
            Func<bool> predicate,
            string message = null,
            ILog logger = null)
        {
            if (predicate())
            {
                ThrowInternal(logger, message);
            }
        }

        /// <summary>
        /// This is the internal method that does all of the work for the public methods. Creates an exception type using reflection. Attempts to perform uniform logging if the given logger is non-null.
        /// </summary>
        private static void ThrowInternal(
            ILog logger,
            params object[] args)
        {
            TException toThrow =
                Activator.CreateInstance(
                    typeof(TException),
                    args) as TException;

            if (logger != null)
            {
                logger.LogError($"{toThrow}");

                // Worst-case placeholders if we can't parse the relevant information.
                var position = "?";
                var className = "EXCEPTION";
                {
                    // Make sure we don't look back on a method call from within this class
                    var st = new StackTrace();
                    foreach (var sf in st.GetFrames())
                    {
                        className = sf.GetMethod().DeclaringType.Name;

                        if (className != typeof(Exception<TException>).Name)
                        {
                            position = sf.GetMethod().Name;
                            break;
                        }
                    }
                }

                logger.LogTraceMessage(
                    TraceLevel.Error,
                    $"Exception raised in \"{position}\". " +
                    $"Message: {toThrow.Message}. " +
                    $"Trace: {toThrow.StackTrace}",
                    className);
            }

            throw toThrow;
        }
    }
}