using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using ToyORB.Messages;

namespace ToyORB.Network
{
    public class MessageServer
    {
        private readonly TcpListener _serverListener;

        public MessageServer(string serviceName, int port)
        {
            Message.InitReaders();
            _serverListener = new TcpListener(IPAddress.Any, port);
            _serverListener.Start();
            Console.WriteLine($"{serviceName} server listening on port {port}");
        }

        public void RegisterProcessor<TReq, TRes>(IMessageProcessor<TReq, TRes> processor) where TReq : Message where TRes : Message
        {
            _messageProcessors[processor.RequestType] = processor;
        }

        private readonly Dictionary<Message.MessageType, IMessageProcessor> _messageProcessors = new Dictionary<Message.MessageType, IMessageProcessor>();

        public void ProcessNextRequest()
        {
            using (TcpClient socket = _serverListener.AcceptTcpClientAsync().Result)
            {
                var recvStream = new BinaryReader(socket.GetStream());
                Message request = Message.ReadFrom(recvStream);
                Console.WriteLine($"Read request from socket {request}");

                IMessageProcessor processor = _messageProcessors[request.Type];
                Message response = processor.ProcessRequest(request, socket.Client.RemoteEndPoint);

                Console.WriteLine($"Writing response to socket {response}");
                var sendStream = new BinaryWriter(socket.GetStream());
                response.WriteInto(sendStream);
                sendStream.Flush();
            }
        }

    }
}
