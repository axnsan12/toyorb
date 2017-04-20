using System;
using System.IO;

namespace ToyORB.Messages
{
    public class ServiceRegistrationMessage : Message
    {
        public static readonly MessageType MESSAGE_TYPE = MessageType.NameRegister;

        public string ServiceName { get; }
        public string ServiceType { get; }
        public int Port { get; }

        public ServiceRegistrationMessage(string serviceName, string serviceType, int port) : base(MESSAGE_TYPE)
        {
            ServiceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            Port = port;
        }

        public override void WriteInto(BinaryWriter stream)
        {
            base.WriteInto(stream);
            stream.WriteBigEndian(ServiceName);
            stream.WriteBigEndian(ServiceType);
            stream.WriteBigEndian(Port);
        }

        public override string ToString()
        {
            return $"ServiceRegistrationMessage{{{ServiceName}, {ServiceType}, {Port}}}";
        }

        protected internal class MessageReader : MessageReader<ServiceRegistrationMessage>
        {
            public MessageReader() : base(MESSAGE_TYPE) { }

            public override ServiceRegistrationMessage ReadMessageFrom(BinaryReader stream)
            {
                string serviceName = stream.ReadStringBigEndian();
                string serviceType = stream.ReadStringBigEndian();
                int port = stream.ReadInt32BigEndian();
                return new ServiceRegistrationMessage(serviceName, serviceType, port);
            }
        }

        protected internal static MessageReader<ServiceRegistrationMessage> Reader = new MessageReader();
    }
}