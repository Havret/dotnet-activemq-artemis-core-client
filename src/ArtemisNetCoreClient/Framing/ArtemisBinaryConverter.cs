using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using ActiveMQ.Artemis.Core.Client.InternalUtilities;

namespace ActiveMQ.Artemis.Core.Client.Framing;

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
    public static int GetNullableInt32ByteCount(int? value)
    {
        var byteCount = sizeof(byte);
        if (value.HasValue)
        {
            byteCount += sizeof(int);
        }

        return byteCount;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ReadNullableInt32(in ReadOnlySpan<byte> source, out int? value)
    {
        var readBytes = ReadBool(source, out var hasValue);
        if (hasValue)
        {
            readBytes += ReadInt32(source[readBytes..], out var intValue);
            value = intValue;
        }
        else
        {
            value = null;
        }

        return readBytes;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int WriteNullableInt32(ref byte destination, int? value)
    {
        var offset = WriteBool(ref destination, value.HasValue);
        if (value.HasValue)
        {
            offset += WriteInt32(ref destination.GetOffset(offset), value.Value);
        }

        return offset;
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
    public static int ReadInt64(in ReadOnlySpan<byte> source, out long value)
    {
        value =  BinaryPrimitives.ReadInt64BigEndian(source);
        return sizeof(long);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int WriteInt64(ref byte destination, long value)
    {
        Unsafe.WriteUnaligned(ref destination, BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value);
        return sizeof(long);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetNullableInt64ByteCount(long? value)
    {
        var byteCount = sizeof(byte);
        if (value.HasValue)
        {
            byteCount += sizeof(long);
        }

        return byteCount;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ReadNullableInt64(in ReadOnlySpan<byte> source, out long? value)
    {
        var readBytes = ReadBool(source, out var hasValue);
        if (hasValue)
        {
            readBytes += ReadInt64(source[readBytes..], out var longValue);
            value = longValue;
        }
        else
        {
            value = null;
        }

        return readBytes;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int WriteNullableInt64(ref byte destination, long? value)
    {
        var offset = WriteBool(ref destination, value.HasValue);
        if (value.HasValue)
        {
            offset += WriteInt64(ref destination.GetOffset(offset), value.Value);
        }

        return offset;
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ReadInt16(in ReadOnlySpan<byte> source, out short value)
    {
        value = BinaryPrimitives.ReadInt16BigEndian(source);
        return sizeof(short);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int WriteInt16(ref byte destination, short value)
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
    public static int GetNullableBoolByteCount(bool? value)
    {
        var byteCount = sizeof(byte);
        if (value.HasValue)
        {
            byteCount += sizeof(byte);
        }

        return byteCount;
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
            _ => GetSimpleStringByteCount(value)
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
            readBytes += ReadSimpleString(source[readBytes..], out value);
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
            foreach (var c in value)
            {
                offset += WriteInt16(ref destination.GetOffset(offset), (short) c);
            }
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
            offset += WriteSimpleString(ref destination.GetOffset(offset), value);
        }

        return offset;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ReadNullableString(in ReadOnlySpan<byte> source, out string? value)
    {
        var readBytes = ReadByte(source, out var isNotNull);
        if (isNotNull == DataConstants.NotNull)
        {
            readBytes += ReadString(source[readBytes..], out var stringValue);
            value = stringValue;
        }
        else
        {
            value = null;
        }

        return readBytes;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int WriteNullableString(ref byte destination, string? value)
    {
        if (value is null)
        {
            return WriteByte(ref destination, DataConstants.Null);
        }
        
        var offset = WriteByte(ref destination, DataConstants.NotNull);
        offset += WriteString(ref destination.GetOffset(offset), value);
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
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetSimpleStringByteCount(string value)
    {
        return sizeof(int) + value.Length * 2;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int WriteSimpleString(ref byte destination, string value)
    {
        var offset = WriteInt32(ref destination, value.Length << 1);
        foreach (var c in value)
        {
            // Low byte
            offset += WriteByte(ref destination.GetOffset(offset), (byte) (c & 0xFF));

            // High byte
            offset += WriteByte(ref destination.GetOffset(offset), (byte) ((c >> 8) & 0xFF));
        }

        return offset;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ReadSimpleString(in ReadOnlySpan<byte> source, out string value)
    {
        var readBytes = ReadInt32(source, out var byteCount);
        unsafe
        {
            ref var reference = ref MemoryMarshal.GetReference(source.Slice(readBytes, byteCount));
            var ptr = Unsafe.AsPointer(ref reference);
            value = string.Create(byteCount >> 1, (ptr: (IntPtr) ptr, byteCount), static (span, state) =>
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

        readBytes += byteCount;

        return readBytes;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetNullableSimpleStringByteCount(string? value)
    {
        var byteCount = sizeof(byte);
        if (value != null)
        {
            byteCount += GetSimpleStringByteCount(value);
        }

        return byteCount;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ReadNullableSimpleString(in ReadOnlySpan<byte> source, out string? value)
    {
        var readBytes = ReadByte(source, out var isNotNull);
        if (isNotNull == DataConstants.NotNull)
        {
            readBytes += ReadSimpleString(source[readBytes..], out var stringValue);
            value = stringValue;
        }
        else
        {
            value = null;
        }

        return readBytes;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int WriteNullableSimpleString(ref byte destination, string? value)
    {
        if (value is null)
        {
            return WriteByte(ref destination, DataConstants.Null);
        }
        
        var offset = WriteByte(ref destination, DataConstants.NotNull);
        offset += WriteSimpleString(ref destination.GetOffset(offset), value);
        return offset;
    }

    public const int GuidByteCount = 16;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int WriteGuid(ref byte destination, Guid value)
    {
        var span = MemoryMarshal.CreateSpan(ref destination, GuidByteCount);
        _ = value.TryWriteBytes(span, bigEndian: true, out var bytesWritten);
        return bytesWritten;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ReadGuid(in ReadOnlySpan<byte> source, out Guid value)
    {
        value = new Guid(source, bigEndian: true);
        return GuidByteCount;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetNullableGuidByteCount(Guid? value)
    {
        var byteCount = sizeof(byte);
        if (value.HasValue)
        {
            byteCount += GuidByteCount;
        }

        return byteCount;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int WriteDouble(ref byte destination, double value)
    {
        var longValue = BitConverter.DoubleToInt64Bits(value);
        return WriteInt64(ref destination, longValue);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ReadDouble(in ReadOnlySpan<byte> source, out double value)
    {
        var readBytes = ReadInt64(source, out var longValue);
        value = BitConverter.Int64BitsToDouble(longValue);
        return readBytes;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int WriteFloat(ref byte destination, float value)
    {
        var intValue = BitConverter.SingleToInt32Bits(value);
        return WriteInt32(ref destination, intValue);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ReadFloat(in ReadOnlySpan<byte> source, out float value)
    {
        var readBytes = ReadInt32(source, out var intValue);
        value = BitConverter.Int32BitsToSingle(intValue);
        return readBytes;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int WriteNullableGuid(ref byte destination, Guid? value)
    {
        if (value.HasValue)
        {
            var offset = WriteByte(ref destination, DataConstants.NotNull);
            offset += WriteGuid(ref destination.GetOffset(offset), value.Value);
            return offset;
        }

        return WriteByte(ref destination, DataConstants.Null);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ReadNullableGuid(in ReadOnlySpan<byte> source, out Guid? value)
    {
        var readBytes = ReadByte(source, out var isNotNull);
        if (isNotNull == DataConstants.NotNull)
        {
            readBytes += ReadGuid(source[readBytes..], out var guidValue);
            value = guidValue;
        }
        else
        {
            value = null;
        }

        return readBytes;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ReadBytes(in ReadOnlySpan<byte> source, out byte[] value)
    {
        var readBytes = ReadInt32(source, out var length);
        value = new byte[length];
        source.Slice(readBytes, length).CopyTo(value);
        return readBytes + length;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int WriteBytes(ref byte destination, byte[] value)
    {
        var offset = 0;
        offset += WriteInt32(ref destination, value.Length);
        var span = MemoryMarshal.CreateSpan(ref destination.GetOffset(offset), value.Length);
        value.CopyTo(span);
        return offset + value.Length;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetNullableObjectByteCount(object? value)
    {
        var byteCount = sizeof(byte);
        switch (value)
        {
            case null:
                break;
            case string stringValue:
                byteCount += GetSimpleStringByteCount(stringValue);
                break;
            case bool:
            case byte:
                byteCount += sizeof(byte);
                break;
            case byte[] bytes:
                byteCount += sizeof(int) + bytes.Length;
                break;
            case short:
            case char:
                byteCount += sizeof(short);
                break;
            case long:
            case double:
                byteCount += sizeof(long);
                break;
            case int:
            case float:
                byteCount += sizeof(int);
                break;
            default:
                throw new NotSupportedException($"Unsupported object type: {value.GetType()}");
        }

        return byteCount;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ReadNullableObject(in ReadOnlySpan<byte> source, out object? value)
    {
        var readBytes = ReadByte(source, out var type);
        switch (type)
        {
            case DataConstants.Null:
                value = null;
                break;
            case DataConstants.String:
                readBytes += ReadSimpleString(source[readBytes..], out var stringValue);
                value = stringValue;
                break;
            case DataConstants.Byte:
                readBytes += ReadByte(source[readBytes..], out var byteValue);
                value = byteValue;
                break;
            case DataConstants.Bytes:
                readBytes += ReadBytes(source[readBytes..], out var bytes);
                value = bytes;
                break;
            case DataConstants.Bool:
                readBytes += ReadBool(source[readBytes..], out var boolValue);
                value = boolValue;
                break;
            case DataConstants.Char:
                readBytes += ReadInt16(source[readBytes..], out var charValue);
                value = (char) charValue;
                break;
            case DataConstants.Double:
                readBytes += ReadDouble(source[readBytes..], out var doubleValue);
                value = doubleValue;
                break;
            case DataConstants.Float:
                readBytes += ReadFloat(source[readBytes..], out var floatValue);
                value = floatValue;
                break;
            case DataConstants.Short:
                readBytes += ReadInt16(source[readBytes..], out var shortValue);
                value = shortValue;
                break;
            case DataConstants.Int:
                readBytes += ReadInt32(source[readBytes..], out var intValue);
                value = intValue;
                break;
            case DataConstants.Long:
                readBytes += ReadInt64(source[readBytes..], out var longValue);
                value = longValue;
                break;
            default:
                throw new NotSupportedException($"Unsupported object type: {type}");
        }

        return readBytes;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int WriteNullableObject(ref byte destination, object? value)
    {
        var offset = 0;
        switch (value)
        {
            case null:
                offset += WriteByte(ref destination, DataConstants.Null);
                break;
            case bool boolValue:
                offset += WriteByte(ref destination, DataConstants.Bool);
                offset += WriteBool(ref destination.GetOffset(offset), boolValue);
                break;
            case byte byteValue:
                offset += WriteByte(ref destination, DataConstants.Byte);
                offset += WriteByte(ref destination.GetOffset(offset), byteValue);
                break;
            case string stringValue:
                offset += WriteByte(ref destination, DataConstants.String);
                offset += WriteSimpleString(ref destination.GetOffset(offset), stringValue);
                break;
            case byte[] bytes:
                offset += WriteByte(ref destination, DataConstants.Bytes);
                offset += WriteBytes(ref destination.GetOffset(offset), bytes);
                break;
            case char charValue:
                offset += WriteByte(ref destination, DataConstants.Char);
                offset += WriteInt16(ref destination.GetOffset(offset), (short) charValue);
                break;
            case double doubleValue:
                offset += WriteByte(ref destination, DataConstants.Double);
                offset += WriteDouble(ref destination.GetOffset(offset), doubleValue);
                break;
            case float floatValue:
                offset += WriteByte(ref destination, DataConstants.Float);
                offset += WriteFloat(ref destination.GetOffset(offset), floatValue);
                break;
            case short shortValue:
                offset += WriteByte(ref destination, DataConstants.Short);
                offset += WriteInt16(ref destination.GetOffset(offset), shortValue);
                break;
            case int intValue:
                offset += WriteByte(ref destination, DataConstants.Int);
                offset += WriteInt32(ref destination.GetOffset(offset), intValue);
                break;
            case long longValue:
                offset += WriteByte(ref destination, DataConstants.Long);
                offset += WriteInt64(ref destination.GetOffset(offset), longValue);
                break;
            default:
                throw new NotSupportedException($"Unsupported object type: {value.GetType()}");
        }

        return offset;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int WriteDateTimeOffset(ref byte destination, DateTimeOffset value)
    {
        var milliseconds = value != DateTimeOffset.MinValue ? value.ToUnixTimeMilliseconds() : 0;
        return WriteInt64(ref destination, milliseconds);
    }
}