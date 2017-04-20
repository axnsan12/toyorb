using System;
using System.IO;
using System.Text;

namespace ToyORB.Messages
{
    public static class BigEndianExtensions
    {
        public static void WriteBigEndian(this BinaryWriter stream, short value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            stream.Write(bytes);
        }

        public static void WriteBigEndian(this BinaryWriter stream, int value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            stream.Write(bytes);
        }

        public static void WriteBigEndian(this BinaryWriter stream, float value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            stream.Write(bytes);
        }

        public static void WriteBigEndian(this BinaryWriter stream, string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            stream.WriteBigEndian((short) bytes.Length);
            stream.Write(bytes);
        }

        public static byte[] ReadFully(this BinaryReader stream, int count)
        {
            byte[] buffer = new byte[count];
            return ReadFully(stream, buffer);
        }

        public static byte[] ReadFully(this BinaryReader stream, byte[] buffer)
        {
            int offset = 0;
            while (offset < buffer.Length)
            {
                int read = stream.Read(buffer, offset, buffer.Length - offset);
                if (read == 0)
                    throw new System.IO.EndOfStreamException();
                offset += read;
            }
            System.Diagnostics.Debug.Assert(offset == buffer.Length);
            return buffer;
        }

        private static readonly byte[] _shortBuffer = new byte[2], _intBuffer = new byte[4], _floatBuffer = new byte[4];

        public static short ReadInt16BigEndian(this BinaryReader stream)
        {
            stream.ReadFully(_shortBuffer);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(_shortBuffer);
            }
            return BitConverter.ToInt16(_shortBuffer, 0);
        }
        public static int ReadInt32BigEndian(this BinaryReader stream)
        {
            stream.ReadFully(_intBuffer);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(_intBuffer);
            }
            return BitConverter.ToInt32(_intBuffer, 0);
        }
        public static float ReadSingleBigEndian(this BinaryReader stream)
        {
            stream.ReadFully(_floatBuffer);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(_floatBuffer);
            }
            return BitConverter.ToSingle(_floatBuffer, 0);
        }

        public static string ReadStringBigEndian(this BinaryReader stream)
        {
            short length = stream.ReadInt16BigEndian();
            var bytes = stream.ReadFully(length);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}