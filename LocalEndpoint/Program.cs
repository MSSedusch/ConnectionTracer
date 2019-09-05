using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LocalEndpoint
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            RunAsync();
            Console.ReadLine();

        }

        protected static async Task RunAsync()
        {
            var ipEndpoint = new IPEndPoint(IPAddress.Any, 9005);

            var listener = new TcpListener(ipEndpoint) { ExclusiveAddressUse = false };

            listener.Start();

            listener.BeginAcceptTcpClient(OnNewConnection, listener);
        }

        private static void OnNewConnection(IAsyncResult ar)
        {
            Console.WriteLine("New Connection");
            var listener = (TcpListener)ar.AsyncState;
            var client = listener.EndAcceptTcpClient(ar);

            listener.BeginAcceptTcpClient(OnNewConnection, listener);


            try
            {
                var sourcePort = ((IPEndPoint)client.Client.RemoteEndPoint).Port;
                var sourceIp = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();

                var stream = client.GetStream();
                StreamWriter writer = new StreamWriter(stream);
                StreamReader reader = new StreamReader(stream);

                while (true)
                {
                    writer.WriteLine($"Hello {sourceIp}:{sourcePort}");
                    Console.WriteLine($"[Server] Hello {sourceIp}:{sourcePort}");
                    writer.Flush();
                    
                    var message = reader.ReadLine();
                    Console.WriteLine($"[Server] Response {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
