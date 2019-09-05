using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace StatelessTCP
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class StatelessTCP : StatelessService
    {
        public StatelessTCP(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            var endpoints = Context.CodePackageActivationContext.GetEndpoints()
        .Where(endpoint => endpoint.Protocol == EndpointProtocol.Tcp)
        .Select(endpoint => endpoint.Name);

            return endpoints.Select(endpoint => new ServiceInstanceListener(
                serviceContext => new TcpCommunicationListener(serviceContext, ServiceEventSource.Current, endpoint), endpoint));
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            var ipEndpoint = new IPEndPoint(IPAddress.Any, 9005);

            var listener = new TcpListener(ipEndpoint) { ExclusiveAddressUse = false };

            listener.Start();

            listener.BeginAcceptTcpClient(OnNewConnection, listener);
        }

        private void OnNewConnection(IAsyncResult ar)
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
