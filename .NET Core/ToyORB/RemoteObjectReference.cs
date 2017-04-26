using System;
using System.IO;
using ToyORB.Messages;

namespace ToyORB
{
    public class RemoteObjectReference
    {
        public string Host { get; }
        public int Port { get; }
        public string ServiceName { get; }
        public string TypeName { get; }

        public RemoteObjectReference(string host, int port, string serviceName, string typeName)
        {
            Host = host ?? throw new ArgumentNullException(nameof(host));
            Port = port;
            ServiceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
            TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
        }

        public void WriteInto(BinaryWriter stream)
        {
            stream.WriteBigEndian(Host);
            stream.WriteBigEndian(Port);
            stream.WriteBigEndian(ServiceName);
            stream.WriteBigEndian(TypeName);
        }

        public static RemoteObjectReference ReadFrom(BinaryReader stream)
        {
            string host = stream.ReadStringBigEndian();
            int port = stream.ReadInt32BigEndian();
            string serviceName = stream.ReadStringBigEndian();
            string typeName = stream.ReadStringBigEndian();
            return new RemoteObjectReference(host, port, serviceName, typeName);
        }

        public override string ToString()
        {
            return $"RemoteObjectReference{{{nameof(Host)}: {Host}, {nameof(Port)}: {Port}, {nameof(ServiceName)}: {ServiceName}, {nameof(TypeName)}: {TypeName}}}";
        }
    }
}
