// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.DnsProtocol.Enum;

namespace Iot.Device.DnsProtocol.EventArgs
{
    /// <summary>
    /// The EventArgs used to pass the status of the service.
    /// </summary>
    public class DnsStatusEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DnsStatusEventArgs" /> class.
        /// </summary>
        /// <param name="status">The communicated status of the service.</param>
        /// <param name="message">The optional message accompanying the status.</param>
        public DnsStatusEventArgs(DnsStatus status, string message = "")
        {
            Status = status;
            Message = message;
        }

        /// <summary>
        /// Gets the communicated status of the service.
        /// </summary>
        public DnsStatus Status { get; }

        /// <summary>
        /// Gets the optional message accompanying the status.
        /// </summary>
        public string Message { get; }
    }
}
