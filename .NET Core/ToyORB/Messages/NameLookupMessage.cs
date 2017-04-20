using System;
using System.IO;

namespace ToyORB.Messages
{
    public class NameLookupMessage : Message
    {
        public static readonly MessageType MESSAGE_TYPE = MessageType.NameLookup;

        public string ServiceName { get; }

        public NameLookupMessage(string serviceName) : base(MESSAGE_TYPE)
        {
            ServiceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
        }

        public override void WriteInto(BinaryWriter stream)
        {
            base.WriteInto(stream);
            stream.WriteBigEndian(ServiceName);
        }

        public override string ToString()
        {
            return "NameLookupMessage{" + ServiceName + "}";
        }

        protected internal class MessageReader : MessageReader<NameLookupMessage>
        {
            public MessageReader() : base(MESSAGE_TYPE) { }

            public override NameLookupMessage ReadMessageFrom(BinaryReader stream)
            {
                return new NameLookupMessage(stream.ReadStringBigEndian());
            }
        }

        protected internal static MessageReader<NameLookupMessage> Reader = new MessageReader();
    }
}