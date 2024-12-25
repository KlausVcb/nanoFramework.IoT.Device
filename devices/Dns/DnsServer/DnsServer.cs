// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Net;
using Iot.Device.DnsProtocol.Entities;
using System.Net.Sockets;
using System.Threading;
using Iot.Device.DnsProtocol.EventArgs;
using Iot.Device.DnsProtocol.Enum;

namespace Iot.Device.MulticastDns
{
    /// <summary>
    /// DnsServer monitors for incoming DNS questions and answers accordingly.
    /// </summary>
    public sealed class DnsServer : IDisposable
    {
        private const int DefaultDnsPort = 53;

        private bool _listening = false;
        private readonly UdpClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsServer" /> class.
        /// </summary>
        public DnsServer()
        {
            _client = new(new IPEndPoint(IPAddress.Any, DefaultDnsPort));
        }

        /// <summary>
        /// Start the worker thread that will listen for DNS packets.
        /// </summary>
        public void Start()
        {
            if (!_listening)
            {
                _listening = true;
                new Thread(Run).Start();
            }
        }

        /// <summary>
        /// Stop the worker thread that is listening for DNS packets.
        /// </summary>
        public void Stop() => _listening = false;

        /// <summary>
        /// The delegate that will be invoked when a DNS message is received.
        /// </summary>
        /// <param name="sender">The MulticastDNSService instance that received the message.</param>
        /// <param name="e">The MessageReceivedEventArgs containing the received message.</param>
        public delegate void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs e);

        /// <summary>
        /// The event that is raised when a Multicast DNS message is received.
        /// </summary>
        public event MessageReceivedEventHandler MessageReceived;

        /// <summary>
        /// The delegate that will be invoked when the status of a Multicast DNS service is changed.
        /// </summary>
        /// <param name="sender">The MulticastDNSService instance that is reporting its status.</param>
        /// <param name="e">The MulticastDnsStatusEventArgs containing the status and an optional message.</param>
        public delegate void MulticastDnsStatusChangedEventHandler(object sender, DnsStatusEventArgs e);

        /// <summary>
        /// The event that is raised when the status of a Multicast DNS service is changed.
        /// </summary>
        public event MulticastDnsStatusChangedEventHandler StatusChanged;

        private void Run()
        {
            try
            {
                IPEndPoint remoteEndpoint = new(IPAddress.Any, 0);

                byte[] buffer = new byte[2048];

                StatusChanged?.Invoke(this, new DnsStatusEventArgs(DnsStatus.Running));

                while (_listening)
                {
                    int length = _client.Receive(buffer, ref remoteEndpoint);
                    if (length == 0)
                    {
                        continue;
                    }

                    Message msg = new(buffer);

                    if (msg != null)
                    {
                        MessageReceivedEventArgs eventArgs = new(msg);

                        MessageReceived?.Invoke(this, eventArgs);

                        if (eventArgs.Response != null)
                        {
                            _client.Send(eventArgs.Response.GetBytes(), remoteEndpoint);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke(this, new DnsStatusEventArgs(DnsStatus.Error, ex.ToString()));
            }

            StatusChanged?.Invoke(this, new DnsStatusEventArgs(DnsStatus.Stopped));
        }

        /// <summary>
        /// Dispose the Multicast DNS Service which causes it to stop listening.
        /// </summary>
        public void Dispose()
        {
            Stop();

            _client.Close();
            _client.Dispose();
        }
    }
}
