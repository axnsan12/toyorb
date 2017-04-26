using System;
using System.IO;

namespace ToyORB.Messages
{
    public class NameResponseMessage : Message
    {
        public static readonly MessageType MESSAGE_TYPE = MessageType.NameResponse;

        private readonly string _errorMessage;
        private readonly RemoteObjectReference _remoteObject;

        public string Host => Checked.Host;
        public int Port => Checked.Port;
        public string ServiceName => Checked.ServiceName;
        public string TypeName => Checked.TypeName;

        public bool Check()
        {
            return Checked != null;
        }

        private RemoteObjectReference Checked
        {
            get
            {
                if (!string.IsNullOrEmpty(_errorMessage))
                {
                    throw new RemoteCallException(_errorMessage);
                }

                return _remoteObject;
            }
        }

        public NameResponseMessage(string errorMessage) : base(MESSAGE_TYPE)
        {
            _errorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage));
            _remoteObject = null;
        }
        public NameResponseMessage(RemoteObjectReference remoteObject) : base(MESSAGE_TYPE)
        {
            _errorMessage = "";
            _remoteObject = remoteObject ?? throw new ArgumentNullException(nameof(remoteObject));
        }

        public override void WriteInto(BinaryWriter stream)
        {
            base.WriteInto(stream);
            stream.WriteBigEndian(_errorMessage);
            if (string.IsNullOrEmpty(_errorMessage))
                _remoteObject.WriteInto(stream);
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(_errorMessage))
                return "MethodReturnMessage{error: " + _errorMessage + "}";

            return "NameResponseMessage{" + _remoteObject + "}";
        }

        protected internal class MessageReader : MessageReader<NameResponseMessage>
        {
            public MessageReader() : base(MESSAGE_TYPE) { }

            public override NameResponseMessage ReadMessageFrom(BinaryReader stream)
            {
                string errorMessage = stream.ReadStringBigEndian();
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    return new NameResponseMessage(errorMessage);
                }

                RemoteObjectReference remoteObject = RemoteObjectReference.ReadFrom(stream);
                return new NameResponseMessage(remoteObject);
            }
        }

        protected internal static MessageReader<NameResponseMessage> Reader = new MessageReader();
    }
}
