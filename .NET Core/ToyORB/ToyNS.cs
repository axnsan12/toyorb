using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using ToyORB.Messages;
using ToyORB.Network;
using ToyORB.Proxy;

namespace ToyORB
{
    public class ToyNS
    {
        public const int DefaultPort = 5353;
        public const string DefaultHostname = "localhost";

        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }

        private const int MinObjPort = 1111, MaxObjPort = 1211;

        private static ServerSideProxy BindAvailablePort(string serviceName, IToyOrbService serviceImpl)
        {
            var random = new Random();
            var ports = Enumerable.Range(MinObjPort, MaxObjPort).OrderBy(p => random.NextDouble());

            foreach (int port in ports)
            {
                try
                {
                    var proxy = new ServerSideProxy(port, serviceName, serviceImpl);
                    proxy.Bind();
                    return proxy;
                }
                catch (SocketException e)
                {
                    Console.WriteLine(e);
                }
            }

            throw new IOException("failed to find available port");
        }

        public static void RegisterAndStart(string serviceName, IToyOrbService serviceImpl, string nsHost = DefaultHostname, int nsPort = DefaultPort)
        {
            ServerSideProxy serviceProxy = BindAvailablePort(serviceName, serviceImpl);

            var request = new ServiceRegistrationMessage(serviceName, serviceImpl.ServiceType, serviceProxy.Port);
            var requestor = new Requestor<ServiceRegistrationMessage, NameResponseMessage>(nsHost, nsPort);
            NameResponseMessage response = requestor.MakeRequest(request);
            response.Check();

            serviceProxy.Start();
        }

        public static T GetServiceReference<T>(string serviceName) where T : IToyOrbService
        {
            return default(T);
        }
    }
}