using System;
using System.IO;

namespace SharpDebug.VS.DPE
{
    internal interface IMessageSerializer
    {
        void Deserialize(BinaryReader reader);

        void Serialize(BinaryWriter writer);
    }

    internal static class MessageSerializer
    {
        public static TOutput Deserialize<TOutput>(byte[] bytes)
        {
            using (MemoryStream memoryStream = new MemoryStream(bytes))
            using (BinaryReader reader = new BinaryReader(memoryStream))
            {
                return (TOutput)Deserialize(reader, typeof(TOutput));
            }
        }

        public static byte[] Serialize(object data)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(memoryStream))
            {
                Serialize(writer, data);
                return memoryStream.ToArray();
            }
        }

        private static object Deserialize(BinaryReader reader, Type type)
        {
            if (type == typeof(bool))
                return reader.ReadBoolean();
            else if (type == typeof(char))
                return reader.ReadChar();
            else if (type == typeof(byte))
                return reader.ReadByte();
            else if (type == typeof(sbyte))
                return reader.ReadSByte();
            else if (type == typeof(short))
                return reader.ReadInt16();
            else if (type == typeof(ushort))
                return reader.ReadUInt16();
            else if (type == typeof(int))
                return reader.ReadInt32();
            else if (type == typeof(uint))
                return reader.ReadUInt32();
            else if (type == typeof(long))
                return reader.ReadInt64();
            else if (type == typeof(ulong))
                return reader.ReadUInt64();
            else if (type == typeof(float))
                return reader.ReadSingle();
            else if (type == typeof(double))
                return reader.ReadDouble();
            else if (type == typeof(string))
                return reader.ReadString();
            else if (type.IsArray)
            {
                int length = reader.ReadInt32();
                Type elementType = type.GetElementType();
                Array a = Array.CreateInstance(elementType, length);

                for (int i = 0; i < length; i++)
                    a.SetValue(Deserialize(reader, elementType), i);
                return a;
            }
            else if (type.FullName.StartsWith("System.Tuple"))
            {
                Type[] arguments = type.GenericTypeArguments;
                object[] parameters = new object[arguments.Length];

                for (int i = 0; i < arguments.Length; i++)
                    parameters[i] = Deserialize(reader, arguments[i]);
                return Activator.CreateInstance(type, parameters);
            }
            else
            {
                IMessageSerializer output = (IMessageSerializer)Activator.CreateInstance(type);
                output.Deserialize(reader);
                return output;
            }
        }

        private static void Serialize(BinaryWriter writer, object data)
        {
            Type type = data.GetType();

            if (type == typeof(bool))
                writer.Write((bool)data);
            else if (type == typeof(char))
                writer.Write((char)data);
            else if (type == typeof(byte))
                writer.Write((byte)data);
            else if (type == typeof(sbyte))
                writer.Write((sbyte)data);
            else if (type == typeof(short))
                writer.Write((short)data);
            else if (type == typeof(ushort))
                writer.Write((ushort)data);
            else if (type == typeof(int))
                writer.Write((int)data);
            else if (type == typeof(uint))
                writer.Write((uint)data);
            else if (type == typeof(long))
                writer.Write((long)data);
            else if (type == typeof(ulong))
                writer.Write((ulong)data);
            else if (type == typeof(float))
                writer.Write((float)data);
            else if (type == typeof(double))
                writer.Write((double)data);
            else if (type == typeof(string))
                writer.Write((string)data);
            else if (type.IsArray)
            {
                Array a = (Array)data;

                writer.Write(a.Length);
                for (int i = 0; i < a.Length; i++)
                    Serialize(writer, a.GetValue(i));
            }
            else if (type.FullName.StartsWith("System.Tuple"))
            {
                Type[] arguments = type.GenericTypeArguments;

                for (int i = 0; i < arguments.Length; i++)
                {
                    object value = type.GetProperty($"Item{i + 1}").GetValue(data);

                    Serialize(writer, value);
                }
            }
            else
            {
                IMessageSerializer serializer = (IMessageSerializer)data;

                serializer.Serialize(writer);
            }
        }
    }
}
