using System;
using System.IO;
using System.Net.Sockets;
using ToyORB.Messages;

namespace ToyORB.Network
{
    public class Requestor<TReq, TRes> where TReq : Message where TRes : Message
    {
        private readonly string _host;
        private readonly int _port;

        public Requestor(string host, int port)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _port = port;
        }

        public TRes MakeRequest(TReq request)
        {
            using (var socket = new TcpClient(AddressFamily.InterNetwork))
            {
                socket.ConnectAsync(_host, _port).Wait();

                Console.WriteLine($"Writing request to socket {request}");
                var sendStream = new BinaryWriter(socket.GetStream());
                request.WriteInto(sendStream);
                sendStream.Flush();

                var recvStream = new BinaryReader(socket.GetStream());
                Message response = Message.ReadFrom(recvStream);
                Console.WriteLine($"Read response from socket {response}");

                return (TRes) response;
            }
        }
    }
}