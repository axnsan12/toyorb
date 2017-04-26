using System;
using System.Linq;
using System.Net;
using System.Reflection;
using ToyORB.Messages;
using ToyORB.Network;

namespace ToyORB.Proxy
{
    public class ServerSideProxy
    {
        public int Port { get; }
        public string ServiceName { get; }
        private IToyOrbService _proxiedService;
        private MessageServer _server;

        public ServerSideProxy(int port, string serviceName, IToyOrbService proxiedService)
        {
            _proxiedService = proxiedService ?? throw new ArgumentNullException(nameof(proxiedService));
            Port = port;
            ServiceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
        }

        public void Bind()
        {
            _server = new MessageServer(ServiceName, Port);
            _server.RegisterProcessor(new MethodCallProcessor(_proxiedService));
        }

        public void Start()
        {
            if (_server == null)
            {
                throw new InvalidOperationException("Start() can only be called after a successful call to Bind()");
            }

            while (true)
            {
                _server.ProcessNextRequest();
            }
        }

        private class MethodCallProcessor : IMessageProcessor<MethodCallMessage, MethodReturnMessage>
        {
            private readonly IToyOrbService _invocationTarget;

            public MethodCallProcessor(IToyOrbService invocationTarget)
            {
                _invocationTarget = invocationTarget ?? throw new ArgumentNullException(nameof(invocationTarget));
            }

            public Message.MessageType RequestType { get; } = MethodCallMessage.MESSAGE_TYPE;

            public Message ProcessRequest(Message request, EndPoint source)
            {
                return ProcessRequest((MethodCallMessage) request, source);
            }

            public MethodReturnMessage ProcessRequest(MethodCallMessage request, EndPoint source)
            {
                try
                {
                    Type type = _invocationTarget.GetType();
                    MethodInfo method = type.GetMethod(request.MethodName, request.Arguments.Select(arg => arg.Item2.DotnetType).ToArray());
                    object result = method.Invoke(_invocationTarget,
                        request.Arguments.Select(arg => arg.Item2.Value).ToArray());

                    bool isVoid = method.ReturnType == typeof(void);
                    return new MethodReturnMessage(isVoid, isVoid ? null : Message.DataValue.FromObject(result));
                }
                catch (Exception e)
                {
                    return new MethodReturnMessage(e.Message);
                }
            }
        }
    }
}
