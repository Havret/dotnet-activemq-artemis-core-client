using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;

namespace ActiveMQ.Artemis.Core.Client;

internal static class ArtemisBinaryConverter
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ReadInt32(in ReadOnlySpan<byte> source)
    {
        return BinaryPrimitives.ReadInt32BigEndian(source);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int WriteInt32(ref byte destination, int value)
    {
        Unsafe.WriteUnaligned(ref destination, BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value);
        return sizeof(int);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ReadByte(in ReadOnlySpan<byte> source, out byte value)
    {
        value = source[0];
        return sizeof(byte);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int WriteByte(ref byte destination, byte value)
    {
        Unsafe.WriteUnaligned(ref destination, value);
        return sizeof(byte);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ReadInt64(in ReadOnlySpan<byte> source)
    {
        return BinaryPrimitives.ReadInt64BigEndian(source);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int WriteInt64(ref byte destination, long value)
    {
        Unsafe.WriteUnaligned(ref destination, BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value);
        return sizeof(long);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int WriteInt16(ref byte destination, short value)
    {
        Unsafe.WriteUnaligned(ref destination, BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value);
        return sizeof(short);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ReadBool(in ReadOnlySpan<byte> source, out bool value)
    {
        value = source[0] != 0;
        return sizeof(byte);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int WriteBool(ref byte destination, bool value)
    {
        const byte minusOne = unchecked((byte) -1);
        const byte zero = 0;
        return WriteByte(ref destination, value ? minusOne : zero);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ReadNullableBool(in ReadOnlySpan<byte> source, out bool? value)
    {
        var readBytes = ReadBool(source, out var hasValue);
        if (hasValue)
        {
            readBytes += ReadBool(source[readBytes..], out var boolValue);
            value = boolValue;
        }
        else
        {
            value = null;
        }

        return readBytes;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int WriteNullableBool(ref byte destination, bool? value)
    {
        var offset = WriteBool(ref destination, value.HasValue);
        if (value.HasValue)
        {
            offset += WriteBool(ref destination.GetOffset(offset), value.Value);
        }

        return offset;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetStringByteCount(string value)
    {
        var byteCount = sizeof(int);
        byteCount += value.Length switch
        {
            < 9 => value.Length * sizeof(short),
            < 0xFFF => sizeof(short) + Encoding.UTF8.GetByteCount(value),
            _ => sizeof(int) + value.Length * 2
        };

        return byteCount;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int WriteString(ref byte destination, string value)
    {
        var length = value.Length;
        var offset = WriteInt32(ref destination, length);
        if (value.Length < 9)
        {
            offset += WriteAsShorts(ref destination.GetOffset(offset), value);
        }
        else if (value.Length < 0xFFF)
        {
        }
        else
        {
        }

        return offset;
    }
    
    private static int WriteAsShorts(ref byte destination, string value)
    {
        var offset = 0;
        foreach (var c in value)
        {
            offset += WriteInt16(ref destination, (short) c);
        }

        return offset;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetNullableStringByteCount(string? value)
    {
        var byteCount = sizeof(byte);
        if (value != null)
        {
            byteCount += GetStringByteCount(value);
        }

        return byteCount;
    }
}