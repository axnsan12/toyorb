using System;
using System.IO;
using System.Linq;
using ToyORB.Messages;
using ToyORB.Network;
using System.Reflection.Emit;
using System.Reflection;

namespace ToyORB.Proxy
{
    class ClientSideProxy : IInvocationHandler
    {
        private Requestor<MethodCallMessage, MethodReturnMessage> _requestor; 
        private string ServiceType { get; }

        public ClientSideProxy(string serviceType, string host, int port)
        {
            ServiceType = serviceType;
            _requestor = new Requestor<MethodCallMessage, MethodReturnMessage>(host, port);
        }

        protected MethodReturnMessage RemoteCall(MethodCallMessage call)
        {
            try
            {
                return _requestor.MakeRequest(call);
            }
            catch (IOException e)
            {
                throw new RemoteCallException("remote method call failed becuase of communication error", e);
            }
        }

        public object Invoke(object proxy, MethodInfo method, object[] args)
        {
            Console.WriteLine("Made it into Invoke!");
            var remoteArgs = Enumerable.Zip(method.GetParameters(), args, (pt, pv) => Tuple.Create(pt.Name, pv)).ToList();
            MethodCallMessage call = new MethodCallMessage(method.Name, remoteArgs);
            MethodReturnMessage result = RemoteCall(call);

            return result.Value;
        }
    }
}
