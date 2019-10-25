using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClientCore
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
        static bool verbose = false;

        static void Main(string[] args)
        {
            Log("#################### Starting yeah ##############################");
            try
            {
                RunAsync().Wait();
            }
            catch (Exception ex)
            {

                Console.Write(ex.ToString());
                throw;
            }
        }

        private static async Task RunAsync()
        {
            int connections = 50000;
            string remoteHost = "server-lb.westeurope.cloudapp.azure.com";
            int remotePort = 9005;
            int runtime = -1;
            int maxConnected = 0;
            int sleepTime = 60000;
            DateTime startTime = DateTime.Now;

            //int connections = 10;
            //string remoteHost = "localhost";
            //int remotePort = 9005;            

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

        internal static void Log(string message, bool msgVerbose = false)
        {
            if ((!msgVerbose) || (msgVerbose && verbose))
            {
                Console.WriteLine($"[{DateTime.Now.ToString("yyyyMMdd hh:mm:ss")}] {message}");
            }
        }
    }
}
