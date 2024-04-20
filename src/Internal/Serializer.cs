// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace spkl.CLI.IPC.Internal;
internal static class Serializer
{
    public static void Write(object obj, Stream typeStream, Stream dataStream)
    {
        Serializer.WriteType(obj, typeStream);
        Serializer.WriteData(obj, dataStream);
    }

    public static T Read<T>(Stream typeStream, Stream dataStream)
    {
        Type type = Serializer.ReadType(typeStream);
        return (T)Serializer.ReadData(type, dataStream);
    }

    private static void WriteType(object obj, Stream stream)
    {
        using StreamWriter typeWriter = new StreamWriter(stream, Encoding.UTF8, 1024, leaveOpen: true);
        typeWriter.Write(obj.GetType().AssemblyQualifiedName);
        typeWriter.Flush();
    }

    private static Type ReadType(Stream stream)
    {
        using StreamReader typeReader = new StreamReader(stream, Encoding.UTF8, true, 1024, leaveOpen: true);
        string typeName = typeReader.ReadToEnd();
        return Type.GetType(typeName, throwOnError: true)!;
    }

    private static void WriteData(object obj, Stream stream)
    {
        Serializer.GetSerializer(obj.GetType()).WriteObject(stream, obj);
        stream.Flush();
    }

    private static object ReadData(Type type, Stream stream)
    {
        return Serializer.GetSerializer(type).ReadObject(stream)
            ?? throw new ArgumentException("The stream contained no object.", nameof(stream));
    }

    private static DataContractSerializer GetSerializer(Type type)
    {
        return new DataContractSerializer(type);
    }
}
