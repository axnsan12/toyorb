using System;
using System.IO;
using System.Linq;
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
                    throw new EndOfStreamException();
                offset += read;
            }
            System.Diagnostics.Debug.Assert(offset == buffer.Length);
            return buffer;
        }

        private static readonly byte[] ShortBuffer = new byte[2], IntBuffer = new byte[4], FloatBuffer = new byte[4];

        public static short ReadInt16BigEndian(this BinaryReader stream)
        {
            stream.ReadFully(ShortBuffer);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(ShortBuffer);
            }
            return BitConverter.ToInt16(ShortBuffer, 0);
        }
        public static int ReadInt32BigEndian(this BinaryReader stream)
        {
            stream.ReadFully(IntBuffer);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(IntBuffer);
            }
            return BitConverter.ToInt32(IntBuffer, 0);
        }
        public static float ReadSingleBigEndian(this BinaryReader stream)
        {
            stream.ReadFully(FloatBuffer);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(FloatBuffer);
            }
            return BitConverter.ToSingle(FloatBuffer, 0);
        }

        public static string ReadStringBigEndian(this BinaryReader stream)
        {
            short length = stream.ReadInt16BigEndian();
            var bytes = stream.ReadFully(length);
            return Encoding.UTF8.GetString(bytes);
        }

        public static string RandomString(this Random random, int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
