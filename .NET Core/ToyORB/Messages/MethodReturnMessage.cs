using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace ToyORB.Messages
{
    public class MethodReturnMessage : Message
    {
        public static readonly MessageType MESSAGE_TYPE = MessageType.MethodReturn;

        private readonly string _errorMessage;
        private readonly bool _isVoid;
        private readonly DataValue _returnValue;

        public object Value
        {
            get
            {
                if (!string.IsNullOrEmpty(_errorMessage))
                {
                    throw new RemoteCallException(_errorMessage);
                }

                return _returnValue.Value;
            }
        }

        private MethodReturnMessage(string errorMessage, bool isVoid, DataValue returnValue) : base(MESSAGE_TYPE)
        {
            _errorMessage = errorMessage;
            _isVoid = isVoid;
            _returnValue = returnValue;
        }

        public MethodReturnMessage(string errorMessage) : this(errorMessage, false, null)
        {
            if (string.IsNullOrEmpty(errorMessage))
                throw new ArgumentException(nameof(errorMessage));
        }

        public MethodReturnMessage(bool isVoid, DataValue returnValue) : this(null, isVoid, isVoid ? null : returnValue)
        {
            if (!isVoid && returnValue == null)
                throw new ArgumentException(nameof(returnValue));
        }

        public override void WriteInto(BinaryWriter stream)
        {
            base.WriteInto(stream);
            stream.WriteBigEndian(_errorMessage ?? "");
            if (string.IsNullOrEmpty(_errorMessage))
            {
                stream.Write(_isVoid);
                if (!_isVoid)
                {
                    _returnValue.WriteInto(stream);
                }
            }
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(_errorMessage))
                return "MethodReturnMessage{error: " + _errorMessage + "}";

            return "MethodReturnMessage{" + (_isVoid ? "Void" : _returnValue.Type.ToString()) + ", " + _returnValue?.Value + "}";
        }

        protected internal class MessageReader : MessageReader<MethodReturnMessage>
        {
            public MessageReader() : base(MESSAGE_TYPE) { }

            public override MethodReturnMessage ReadMessageFrom(BinaryReader stream)
            {
                string errorMessage = stream.ReadStringBigEndian();
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    return new MethodReturnMessage(errorMessage);
                }

                bool isVoid = stream.ReadBoolean();
                DataValue retValue = isVoid ? null : DataValue.ReadFrom(stream);
                return new MethodReturnMessage(isVoid, retValue);
            }
        }

        protected internal static MessageReader<MethodReturnMessage> Reader = new MessageReader();
    }
}