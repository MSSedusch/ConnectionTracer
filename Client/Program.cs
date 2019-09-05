using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static List<TcpClient> _Clients = new List<TcpClient>();
        static Dictionary<IAsyncResult, byte[]> _Buffers = new Dictionary<IAsyncResult, byte[]>();
        static HashSet<int> _Ports = new HashSet<int>();

        static void Main(string[] args)
        {
            int connections = int.Parse(args[0]);
            string remoteHost = args[1];
            int remotePort = int.Parse(args[2]);

            Console.WriteLine("Start");
            //int connections = 10;
            //string remoteHost = "connectiontracersf.westeurope.cloudapp.azure.com";
            //int remotePort = 9005;

            //int connections = 10;
            //string remoteHost = "localhost";
            //int remotePort = 9005;

            for (int i = 0; i < connections; i++)
            {
                var myNumber = i;
                try
                {
                    TcpClient client = new TcpClient(remoteHost, remotePort);

                    Thread th = new Thread(() =>
                    {
                        var stream = client.GetStream();
                        StreamWriter writer = new StreamWriter(stream);
                        StreamReader reader = new StreamReader(stream);

                        var message = reader.ReadLine();
                        Console.WriteLine($"[{myNumber}/{_Clients.Count}] Read {message}");
                        var port = int.Parse(message.Split(':')[1]);
                        if (!_Ports.Contains(port))
                        {
                            _Ports.Add(port);
                        }
                        else
                        {
                            Console.WriteLine($"!!Port {port} reused!!");
                        }

                        writer.WriteLine("Next");
                        writer.Flush();

                        while (true)
                        {
                            message = reader.ReadLine();
                            Console.WriteLine($"[{myNumber}/{_Clients.Count}] Read {message}");

                            Thread.Sleep(5000);

                            writer.WriteLine("Next");
                            writer.Flush();
                        }
                    });
                    th.Start();
                    _Clients.Add(client);
                    //var stream = client.GetStream();
                    //byte[] buffer = new byte[1024];

                    //IAsyncResult result = stream.BeginRead(buffer, 0, 1024, OnRead, stream);
                    //_Buffers.Add(result, buffer);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }

            Console.WriteLine("Waiting");
            Console.ReadLine();
        }

        //private static void OnRead(IAsyncResult ar)
        //{
        //    var stream = (NetworkStream)ar.AsyncState;
        //    var readCount = stream.EndRead(ar);

        //    var buffer = _Buffers[ar];

        //    var message = System.Text.Encoding.ASCII.GetString(buffer, 0, readCount);
        //    Console.WriteLine($"[Clients: {_Clients.Count}] Read {message}");

        //    var port = int.Parse(message.Split(':')[1]);
        //    if (!_Ports.Contains(port))
        //    {
        //        _Ports.Add(port);
        //    }
        //    else
        //    {
        //        Console.WriteLine($"!!Port {port} reused!!");
        //    }
        //}
    }
}
