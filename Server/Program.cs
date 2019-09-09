using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        static List<Server> _Servers = new List<Server>();
        static DateTime LastProbeTime = DateTime.MinValue;

        static void Main(string[] args)
        {
            Log("Hello World!");
            //int port = 9005;
            //if (args.Length > 0)

            RunAsync();

            if (args.Length > 0)
            {
                int probePort = int.Parse(args[0]);
                OpenProbeAsync(probePort);
            }

            WriteLogAsync().Wait();
            Console.WriteLine("Done waiting");
            Console.ReadLine();
            Console.WriteLine("Read done");

        }

        internal static async Task WriteLogAsync()
        {
            while (true)
            {
                long connectedServers;
                lock (_Servers)
                {
                    connectedServers = _Servers.Count(server => server.Connected);
                }
                Log($"Status: connected: {connectedServers} total:{_Servers.Count} last probe time: {(DateTime.Now - LastProbeTime).TotalSeconds} seconds ago");
                await Task.Delay(10000);
            }
        }

        internal static void Log(string message)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("yyyyMMdd hh:mm:ss")}] {message}");
        }

        private static async void OpenProbeAsync(int probePort)
        {
            Log("Opening probe port on " + probePort);
            var ipEndpoint = new IPEndPoint(IPAddress.Any, probePort);
            var listener = new TcpListener(ipEndpoint) { ExclusiveAddressUse = false };
            listener.Start();

            while (true)
            {
                //Log("Probe started on port " + probePort);
                await listener.AcceptTcpClientAsync();
                LastProbeTime = DateTime.Now;
                //Log("Probe port pocked");
            }
        }

        protected static async Task RunAsync()
        {
            var ipEndpoint = new IPEndPoint(IPAddress.Any, 9005);
            var listener = new TcpListener(ipEndpoint) { ExclusiveAddressUse = false };
            listener.Start();

            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                //Program.Log("New Connection");
                Server newServer = new Server(client);
                lock (_Servers)
                {
                    _Servers.Add(newServer);
                }
                newServer.StartConnection();
            }
        }
    }
}
