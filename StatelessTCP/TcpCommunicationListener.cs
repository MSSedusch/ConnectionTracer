using System;
using System.Fabric;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace StatelessTCP
{
    internal class TcpCommunicationListener : ICommunicationListener
    {
        private readonly ServiceEventSource eventSource;
        private readonly ServiceContext serviceContext;
        private readonly string endpointName;
        private string listeningAddress;
        private string hostAddress;


        public TcpCommunicationListener(ServiceContext serviceContext, ServiceEventSource eventSource, string endpointName)
        {

            if (serviceContext == null)
            {
                throw new ArgumentNullException(nameof(serviceContext));
            }

            if (endpointName == null)
            {
                throw new ArgumentNullException(nameof(endpointName));
            }

            if (eventSource == null)
            {
                throw new ArgumentNullException(nameof(eventSource));
            }

            this.serviceContext = serviceContext;
            this.endpointName = endpointName;
            this.eventSource = eventSource;
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            var serviceEndpoint = this.serviceContext.CodePackageActivationContext.GetEndpoint(this.endpointName);
            var protocol = serviceEndpoint.Protocol;
            int port = serviceEndpoint.Port;


            //StatefulServiceContext statefulServiceContext = this.serviceContext as StatefulServiceContext;

            this.hostAddress = FabricRuntime.GetNodeContext().IPAddressOrFQDN;

            this.listeningAddress = string.Format(
                CultureInfo.InvariantCulture,
                "{0}://{1}:{2}",
                protocol,
                hostAddress,
                port
               );

            try
            {
                this.eventSource.Message("Starting tcp listener " + this.listeningAddress);

                return Task.FromResult(this.hostAddress);
            }
            catch (Exception ex)
            {
                this.eventSource.Message("Tcp Listener failed to open endpoint {0}. {1}", this.endpointName, ex.ToString());

                throw;
            }
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Abort()
        {
            throw new NotImplementedException();
        }
    }
}