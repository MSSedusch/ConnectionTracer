using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Client
{
    class Client
    {
        [Flags]
        public enum ConnectionState
        {
            Initial         = 0,
            Connected       = 1,
            Disconnected    = 2,
            Read            = 4,
            Write           = 8
        }

        public long ErrorOnConnect { get; set; }
        public long ErrorOnRead { get; set; }
        public long ErrorOnWrite { get; set; }
        public long MessageRead { get; set; }
        public long MessageSent { get; set; }
        public string OutboundIP { get; set; }
        public string Server { get; set; }
        public ConnectionState State { get; set; }

        public Client()
        {
            this.State = ConnectionState.Initial;
        }


        public async void StartClientAsync(string remoteHost, int remotePort, int sleepTime)
        {
            TcpClient client;

            try
            {
                client = new TcpClient();
                await client.ConnectAsync(remoteHost, remotePort);
            }
            catch (Exception ex)
            {
                client = null;
                ErrorOnConnect++;

                //Log($"Error on Connected: {ex.Message}");
            }

            if (client == null)
            {
                State = ConnectionState.Disconnected;
                return;
            }

            State = ConnectionState.Connected;

            var stream = client.GetStream();
            StreamWriter writer = new StreamWriter(stream);
            StreamReader reader = new StreamReader(stream);

            while (true)
            {
                try
                {
                    var message = await reader.ReadLineAsync();
                    //Log($"[{myNumber}/{_Clients.Count}] Read {message}");                        
                    var matches = Regex.Matches(message, @".* (\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}):(\d*).* (\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}):(\d*).*\[(.*)\]");
                    if (matches.Count == 1)
                    {
                        if (matches[0].Groups.Count == 6)
                        {
                            var outIp = matches[0].Groups[1].Value;
                            var server = matches[0].Groups[5].Value;
                            this.OutboundIP = outIp;
                            this.Server = server;
                        }
                        else
                        {
                            Program.Log("error extracting out IP");
                        }
                    }
                    else
                    {
                        Program.Log("error extracting out IP");
                    }
                    State |= ConnectionState.Read;
                    MessageRead++;
                }
                catch (Exception ex)
                {
                    ErrorOnRead++;
                    //Log($"Error on Read: {ex.Message}");
                    break;
                }

                await Task.Delay(new Random().Next(sleepTime, 3 * sleepTime));

                try
                {
                    await writer.WriteLineAsync("Next");
                    await writer.FlushAsync();
                    State |= ConnectionState.Write;
                    MessageSent++;
                }
                catch (Exception ex)
                {
                    ErrorOnWrite++;
                    //Log($"Error on Write: {ex.Message}");
                    break;
                }
            }

            State = ConnectionState.Disconnected;
        }
    }
}