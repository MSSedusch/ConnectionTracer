using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        class ConnectionState
        {
            public State State { get; set; }
            public String OutboundIP { get; set; }
            public string Server { get; set; }
        }

        [Flags]
        enum State
        {
            Initial,
            Connected,
            Disconnected,
            Read,
            Write
        }

        static List<Client> _Clients = new List<Client>();

        static void Main(string[] args)
        {
            //int connections = 10;
            //string remoteHost = "connectiontracersf.westeurope.cloudapp.azure.com";
            //int remotePort = 9005;
            RunAsync(args);

            Console.ReadLine();
        }

        private static async void RunAsync(string[] args)
        {
            int connections = 50000;
            string remoteHost = "server-lb.eastus2.cloudapp.azure.com";
            int remotePort = 9005;
            int runtime = -1;
            int maxConnected = 0;
            int sleepTime = 60000;
            DateTime startTime = DateTime.Now;

            //int connections = 10;
            //string remoteHost = "localhost";
            //int remotePort = 9005;

            if (args.Length > 0)
            {
                connections = int.Parse(args[0]);
                remoteHost = args[1];
                remotePort = int.Parse(args[2]);
            }
            if (args.Length >= 5)
            {
                sleepTime = int.Parse(args[4]);

            }
            if (args.Length >= 6)
            {
                runtime = int.Parse(args[5]);
            }
            Log($"Start {String.Join(" ", args)}");

            while (true)
            {
                if ((runtime > 0) && ((DateTime.Now - startTime).TotalSeconds > runtime))
                {
                    Log($"runtime reached. Stopping. Test result: max connected={maxConnected}");
                    break;
                }

                int running = _Clients.Count(cl => (cl.State & Client.ConnectionState.Disconnected) == Client.ConnectionState.Initial);
                int connected = _Clients.Count(cl => (cl.State & Client.ConnectionState.Connected) == Client.ConnectionState.Connected);

                for (int i = running; i < connections; i++)
                {
                    Client newClient = new Client();
                    _Clients.Add(newClient);
                    newClient.StartClientAsync(remoteHost, remotePort, sleepTime);
                }
                int newThreads = connections - running;
                var ipArray = _Clients.Select(val => val.OutboundIP).Distinct().ToArray();
                string ips = "[]";
                if (ipArray.Length > 0)
                {
                    ips = String.Join(",", ipArray);
                }
                var serverArray = _Clients.Select(val => val.Server).Distinct().ToArray();
                string servers = "[]";
                if (serverArray.Length > 0)
                {
                    servers = String.Join(",", serverArray);
                }

                long errorOnConnect = _Clients.Sum(cl => cl.ErrorOnConnect);
                long errorOnWrite = _Clients.Sum(cl => cl.ErrorOnWrite);
                long errorOnRead = _Clients.Sum(cl => cl.ErrorOnRead);
                long messageSent = _Clients.Sum(cl => cl.MessageSent);
                long messageRead = _Clients.Sum(cl => cl.MessageRead);

                Log($"Status: Threads: running={running} connected={connected} total={_Clients.Count} Target:{connections} new:{newThreads} outIps:{ips} servers:{servers} msg sent={messageSent} msg read={messageRead} errorOnConnect={errorOnConnect} errorOnWrite={errorOnWrite} errorOnRead={errorOnRead}");
                maxConnected = Math.Max(maxConnected, connected);

                await Task.Delay(10000);
            }
        }

        internal static void Log(string message)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("yyyyMMdd hh:mm:ss")}] {message}");
        }

        //private static Thread StartListenerAsync(string remoteHost, int remotePort, int sleepTime)
        //{
        //    Thread th = new Thread(() =>
        //    {
        //        TcpClient client = null;

        //        try
        //        {
        //            client = new TcpClient(remoteHost, remotePort);
        //        }
        //        catch (Exception ex)
        //        {
        //            client = null;
        //            errorOnConnect++;

        //            //Log($"Error on Connected: {ex.Message}");
        //        }

        //        if (client == null)
        //        {
        //            return;
        //        }

        //        connectedThreads[Thread.CurrentThread].State = State.Connected;

        //        var stream = client.GetStream();
        //        StreamWriter writer = new StreamWriter(stream);
        //        StreamReader reader = new StreamReader(stream);

        //        while (true)
        //        {
        //            try
        //            {
        //                var message = reader.ReadLine();
        //                //Log($"[{myNumber}/{_Clients.Count}] Read {message}");                        
        //                var matches = Regex.Matches(message, @".* (\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}):(\d*).* (\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}):(\d*).*\[(.*)\]");
        //                if (matches.Count == 1)
        //                {
        //                    if (matches[0].Groups.Count == 6)
        //                    {
        //                        var outIp = matches[0].Groups[1].Value;
        //                        var server = matches[0].Groups[5].Value;
        //                        connectedThreads[Thread.CurrentThread].OutboundIP = outIp;
        //                        connectedThreads[Thread.CurrentThread].Server = server;
        //                    }
        //                    else
        //                    {
        //                        Log("error extracting out IP");
        //                    }
        //                }
        //                else
        //                {
        //                    Log("error extracting out IP");
        //                }
        //                connectedThreads[Thread.CurrentThread].State |= State.Read;
        //                messageRead++;
        //            }
        //            catch (Exception ex)
        //            {
        //                errorOnRead++;
        //                //Log($"Error on Read: {ex.Message}");
        //                break;
        //            }
        //            Thread.Sleep(new Random().Next(sleepTime, 3 * sleepTime));

        //            try
        //            {
        //                writer.WriteLine("Next");
        //                writer.Flush();
        //                connectedThreads[Thread.CurrentThread].State |= State.Write;
        //                messageSent++;
        //            }
        //            catch (Exception ex)
        //            {
        //                errorOnWrite++;
        //                //Log($"Error on Write: {ex.Message}");
        //                break;
        //            }
        //        }

        //        connectedThreads[Thread.CurrentThread].State = State.Disconnected;
        //    });

        //    return th;
        //}
    }
}
