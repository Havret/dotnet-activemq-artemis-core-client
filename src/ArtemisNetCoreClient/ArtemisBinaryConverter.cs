using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace ActiveMQ.Artemis.Core.Client;

internal static class ArtemisBinaryConverter
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ReadInt32(in ReadOnlySpan<byte> source, out int value)
    {
        value = BinaryPrimitives.ReadInt32BigEndian(source);
        return sizeof(int);
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
    public static int ReadInt16(in ReadOnlySpan<byte> source, out short value)
    {
        value = BinaryPrimitives.ReadInt16BigEndian(source);
        return sizeof(short);
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
    public static int ReadString(in ReadOnlySpan<byte> source, out string value)
    {
        var readBytes = ReadInt32(source, out var length);
        if (length < 9)
        {
            Span<char> chars = stackalloc char[length];
            for (int i = 0; i < length; i++)
            {
                readBytes += ReadInt16(source[readBytes..], out var c);
                chars[i] = (char) c;
            }
            
            value = new string(chars);
        }
        else if (length < 0xFFF)
        {
            readBytes += ReadInt16(source[readBytes..], out var byteCount);
            value = Encoding.UTF8.GetString(source.Slice(readBytes, byteCount));
            readBytes += byteCount;
        }
        else
        {
            readBytes += ReadInt32(source[readBytes..], out var byteCount);
            
            unsafe
            {
                fixed(void* ptr = &MemoryMarshal.GetReference(source.Slice(readBytes, byteCount)))
                {
                    value =  string.Create(byteCount >> 1, (ptr: (IntPtr) ptr, byteCount), static (span, state) =>
                    {
                        var source = new Span<byte>(state.ptr.ToPointer(), state.byteCount);
                        for (var i = 0; i < state.byteCount; i += 2)
                        {
                            var lowByte = source[i];
                            var highByte = source[i + 1];
                            span[i >> 1] = (char) (lowByte | (highByte << 8));
                        }
                    });
                }
            }
            readBytes+= byteCount;
        }

        return readBytes;
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
            int byteCount;
            unsafe
            {
                ref var valDestination = ref destination.GetOffset(offset + sizeof(short));
                fixed (char* chars = value)
                fixed (byte* bytes = &valDestination)
                {
                    byteCount = Encoding.UTF8.GetBytes(chars, value.Length, bytes, byte.MaxValue);
                }
            }

            offset += WriteInt16(ref destination.GetOffset(offset), (short) byteCount);
            offset += byteCount;
        }
        else
        {
            offset += WriteInt32(ref destination.GetOffset(offset), value.Length << 1);
            foreach (var c in value)
            {
                // Low byte
                offset += WriteByte(ref destination.GetOffset(offset), (byte) (c & 0xFF)); 
                
                // High byte
                offset += WriteByte(ref destination.GetOffset(offset), (byte) ((c >> 8) & 0xFF));
            }
        }

        return offset;
    }
    
    private static int WriteAsShorts(ref byte destination, string value)
    {
        var offset = 0;
        foreach (var c in value)
        {
            offset += WriteInt16(ref destination.GetOffset(offset), (short) c);
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