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
            if (args == null) args = new object[0];

            if (method.Name == "get_ServiceType" && args.Length == 0 && method.ReturnType == typeof(string)) {
                return ServiceType;
            }

            var remoteArgs = method.GetParameters().Zip(args, (pt, pv) => Tuple.Create(pt.Name, pv)).ToList();
            var call = new MethodCallMessage(method.Name, remoteArgs);
            var result = RemoteCall(call);

            return result.Value;
        }
    }
}
