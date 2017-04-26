using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ToyORB.Messages
{
    public class MethodCallMessage : Message
    {
        public static readonly MessageType MESSAGE_TYPE = MessageType.MethodCall;

        public string MethodName { get; }
        public IReadOnlyList<Tuple<string, DataValue>> Arguments { get; }

        public MethodCallMessage(string methodName, IList<Tuple<string, object>> arguments) : base(MESSAGE_TYPE)
        {
            MethodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
            Arguments = arguments.Select(nv => Tuple.Create(nv.Item1, DataValue.FromObject(nv.Item2))).ToList();
        }

        private MethodCallMessage(string methodName, IList<Tuple<string, DataValue>> arguments) : base(MESSAGE_TYPE)
        {
            MethodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
            Arguments = new List<Tuple<string, DataValue>>(arguments).AsReadOnly();
        }

        public override void WriteInto(BinaryWriter stream)
        {
            base.WriteInto(stream);
            stream.WriteBigEndian(MethodName);
            stream.WriteBigEndian((short) Arguments.Count);
            foreach (var kv in Arguments)
            {
                stream.WriteBigEndian(kv.Item1);
                kv.Item2.WriteInto(stream);
            }
        }

        public override string ToString()
        {
            string args = string.Join(", ", Arguments.Select(kv => string.Format("{0}: ({1}) {2}", kv.Item1, kv.Item2.Type, kv.Item2.Value)));
            return $"MethodCallMessage{{{MethodName}({args})}}";
        }

        protected internal class MessageReader : MessageReader<MethodCallMessage>
        {
            public MessageReader() : base(MESSAGE_TYPE) { }

            public override MethodCallMessage ReadMessageFrom(BinaryReader stream)
            {
                string methodName = stream.ReadStringBigEndian();
                short argc = stream.ReadInt16BigEndian();
                var arguments = new List<Tuple<string, DataValue>>();
                for (int i = 0; i < argc; ++i)
                {
                    string argName = stream.ReadStringBigEndian();
                    DataValue argValue = DataValue.ReadFrom(stream);
                    arguments.Add(Tuple.Create(argName, argValue));
                }
                return new MethodCallMessage(methodName, arguments);
            }
        }

        protected internal static MessageReader<MethodCallMessage> Reader = new MessageReader();
    }
}
