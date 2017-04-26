using System.Net;
using ToyORB.Messages;

namespace ToyORB.Network
{
    public interface IMessageProcessor
    {
        Message.MessageType RequestType { get; }
        Message ProcessRequest(Message request, EndPoint source);
    }

    public interface IMessageProcessor<in TReq, out TRes> : IMessageProcessor where TReq : Message where TRes : Message
    {
        TRes ProcessRequest(TReq request, EndPoint source);
    }
}
