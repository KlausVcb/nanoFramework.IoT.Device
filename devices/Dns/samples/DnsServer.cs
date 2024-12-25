// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Text;
using System.Threading;
using Iot.Device.DnsProtocol.Entities;
using Iot.Device.DnsProtocol.EventArgs;
using Iot.Device.MulticastDns;
using nanoFramework.WebServer;

namespace MulticastDns.Samples
{
    internal class DnsServerSample
    {
        public static void RunSample()
        {
            // Instantiate the MulticastDnsService
            using DnsServer dnsServer = new();

            // After resolving the domain, the IP address of the device is sent back to the browser
            // We'll serve some text back to show this is actually working
            using WebServer webServer = new(80, HttpProtocol.Http);

            // Register the event handler that will receive mDNS messages
            dnsServer.MessageReceived += MulticastDnsService_MessageReceived;

            // Register the event handler that will treat the HTTP requests
            webServer.CommandReceived += WebServer_CommandReceived;

            // Start the MulticastDnsService
            dnsServer.Start();
            // Start the webserver
            webServer.Start();

            Debug.WriteLine("All ready! Feel free to surf to http://nanodevice.local in your favorite browser...");

            Thread.Sleep(Timeout.Infinite);
        }

        private static void WebServer_CommandReceived(object obj, WebServerEventArgs e)
        {
            byte[] bytes = Encoding.UTF8.GetBytes("Hello there!");
            e.Context.Response.OutputStream.Write(bytes, 0, bytes.Length);
        }

        private static void MulticastDnsService_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.Message != null)
                foreach (Question question in e.Message.GetQuestions())
                {
                    Debug.WriteLine($"Question: {question.Domain} - {question.QueryType}");
                }
        }
    }
}
