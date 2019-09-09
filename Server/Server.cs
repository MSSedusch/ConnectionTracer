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
    class Server
    {
        private TcpClient client;

        public bool Connected { get; set; }

        public Server(TcpClient client)
        {
            this.client = client;
            this.Connected = true;
        }

        internal async void StartConnection()
        {
            
            try
            {
                var remotePort = ((IPEndPoint)client.Client.RemoteEndPoint).Port;
                var remoteIp = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                var localPort = ((IPEndPoint)client.Client.LocalEndPoint).Port;
                var localIp = ((IPEndPoint)client.Client.LocalEndPoint).Address.ToString();

                var stream = client.GetStream();
                StreamWriter writer = new StreamWriter(stream);
                StreamReader reader = new StreamReader(stream);

                while (true)
                {
                    string message = $"Hello {remoteIp}:{remotePort} from {localIp}:{localPort} [{Environment.MachineName}]";
                    await writer.WriteLineAsync(message);
                    //Program.Log(message);
                    await writer.FlushAsync();

                    var response = await reader.ReadLineAsync();
                    //Program.Log($"[Server] Response {response}");
                }
            }
            catch (Exception ex)
            {
                Program.Log($"Error: {ex.Message}");
                this.Connected = false;
            }
        }
    }
}