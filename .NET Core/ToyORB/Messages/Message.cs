using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ToyORB.Messages
{
    public abstract class Message
    {
        public MessageType Type { get; }

        internal Message(MessageType type)
        {
            Type = type;
        }

        public virtual void WriteInto(BinaryWriter stream)
        {
            stream.Write((byte) Type);
        }

        public abstract override string ToString();


        protected internal abstract class MessageReader<T> where T : Message
        {
            public MessageType MessageType { get; }

            protected MessageReader(MessageType type)
            {
                MessageType = type;
            }

            public abstract T ReadMessageFrom(BinaryReader stream);
        }

        private static readonly Dictionary<MessageType, dynamic> MessageReaders = new Dictionary<MessageType, dynamic>();

        public static Message ReadFrom(BinaryReader stream)
        {
            var type = (MessageType) stream.ReadByte();
            if (!MessageReaders.TryGetValue(type, out dynamic reader))
            {
                throw new InvalidOperationException("No reader registered for type " + type + ". Forget to add MessageReader mapping?");
            }

            return reader.ReadMessageFrom(stream);
        }

        private static void AddReader<T>(MessageReader<T> reader) where T : Message
        {
            MessageReaders[reader.MessageType] = reader;
        }

        public static void InitReaders()
        {
            if (MessageReaders.Count > 0)
                return;

            AddReader(MethodCallMessage.Reader);
            AddReader(MethodReturnMessage.Reader);
            AddReader(NameLookupMessage.Reader);
            AddReader(NameResponseMessage.Reader);
            AddReader(ServiceRegistrationMessage.Reader);
        }


        public enum MessageType : byte
        {
            MethodCall = 1,
            MethodReturn = 2,
            NameLookup = 3,
            NameResponse = 4,
            NameRegister = 5
        }

        public enum DataType : byte
        {
            Int = 1,
            Float = 2,
            String = 3,
            Void = 4
        }

        public class DataValue
        {
            public DataType Type { get; }

            public Type DotnetType => _dotnetTypes[Type];

            public object Value { get; }

            public DataValue(int value) : this(DataType.Int, value) { }
            public DataValue(float value) : this(DataType.Float, value) { }
            public DataValue(string value) : this(DataType.String, value) { }

            private DataValue(DataType type, object value)
            {
                if (type == DataType.Void)
                {
                    throw new ArgumentException("DataValue cannot be of void type");
                }
                Type = type;
                Value = value ?? throw new ArgumentNullException(nameof(value));
            }

            private static readonly Dictionary<DataType, Type> _dotnetTypes = new Dictionary<DataType, Type>
            {
                { DataType.Int, typeof(int) },
                { DataType.Float, typeof(float) },
                { DataType.String, typeof(string) },
                { DataType.Void, typeof(void) }
            };

            public void WriteInto(BinaryWriter stream)
            {
                stream.Write((byte) Type);
                switch (Type)
                {
                    case DataType.Int:
                        stream.WriteBigEndian((int) Value);
                        break;
                    case DataType.Float:
                        stream.WriteBigEndian((float) Value);
                        break;
                    case DataType.String:
                        stream.WriteBigEndian((string) Value);
                        break;
                    default:
                        throw new InvalidOperationException("invalid data type - only int, float, string are supported");
                }
            }
            public static DataValue ReadFrom(BinaryReader stream)
            {
                var type = (DataType) stream.ReadByte();
                switch (type)
                {
                    case DataType.Int:
                        return new DataValue(stream.ReadInt32BigEndian());
                    case DataType.Float:
                        return new DataValue(stream.ReadSingleBigEndian());
                    case DataType.String:
                        return new DataValue(stream.ReadStringBigEndian());
                    default:
                        throw new InvalidOperationException("invalid data type - only int, float, string are supported");
                }
            }

            public static DataValue FromObject(object value)
            {
                DataType type = _dotnetTypes.First(kv => kv.Value == value.GetType()).Key;
                switch (type)
                {
                    case DataType.Int:
                        return new DataValue((int) value);
                    case DataType.Float:
                        return new DataValue((float) value);
                    case DataType.String:
                        return new DataValue((string) value);
                    default:
                        throw new InvalidOperationException("invalid data type - only int, float, string are supported");
                }
            }
        }
    }
}
