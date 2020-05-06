// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="UnexpectedException.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace Common
{
    using System;

    /// <summary>
    /// An exception of this type will typically result in an InternalServerError 
    /// at service layer. This exception type should be thrown when a general, 
    /// unpredictable runtime error occurs that is not caused by incorrect user input.
    /// </summary>
    public class UnexpectedException : Exception
    {
        /// <summary>
        /// Parameterless constructor.
        /// </summary>
        public UnexpectedException()
        {
        }

        /// <summary>
        /// Creates a new exception with the given message.
        /// </summary>
        public UnexpectedException(string message)
            :
            base(message)
        {
        }

        /// <summary>
        /// Creates a new exception with the given message and an 
        /// inner exception that caused this new exception.
        /// </summary>
        public UnexpectedException(string message, Exception e)
            :
            base(message, e)
        {
        }
    }
}