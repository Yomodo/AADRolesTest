// --------------------------------------------------------------------------------------
// <copyright file="JsonContent.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------

using System;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Common
{
    /// <summary>
    /// A Json content HttpContent wrapper
    /// </summary>
    /// <seealso cref="StringContent" />
    public class JsonContent : StringContent
    {
        public JsonContent(string json) : base(json, Encoding.UTF8, "application/json")
        {
        }
    }
}