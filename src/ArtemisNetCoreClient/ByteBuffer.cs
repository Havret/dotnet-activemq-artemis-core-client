using System.Buffers;
using System.Buffers.Binary;
using System.Text;

namespace ActiveMQ.Artemis.Core.Client;

internal class ByteBuffer
{
    private readonly MemoryStream _memoryStream = new();

    public ReadOnlyMemory<byte> GetBuffer()
    {
        _memoryStream.TryGetBuffer(out var buffer);
        return buffer;
    }
    
    public void WriteBool(bool value)
    {
        const byte minusOne = unchecked((byte) -1);
        const byte zero = 0;
        WriteByte(value ? minusOne : zero);
    }

    public void WriteByte(byte value)
    {
        Span<byte> buffer = stackalloc byte[1] { value };
        _memoryStream.Write(buffer);
    }
    
    public void WriteInt(int value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(buffer, value);
        _memoryStream.Write(buffer);
    }

    public void WriteLong(long value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(long)];
        BinaryPrimitives.WriteInt64BigEndian(buffer, value);
        _memoryStream.Write(buffer);
    }
    
    public void WriteNullableString(string? value)
    {
        if (value is null)
        {
            WriteByte(DataConstants.Null);
        }
        else
        {
            WriteByte(DataConstants.NotNull);
            WriteString(value);
        }
    }

    public void WriteString(string value)
    {
        var length = value.Length;
        WriteInt(length);
        if (value.Length < 9)
        {
            WriteAsShorts(value);
        }
        else if (value.Length < 0xFFF)
        {
            if (length <= 127 / 3)
            {
                Span<byte> buffer = stackalloc byte[128];
                var actualByteCount = Encoding.UTF8.GetBytes(value, buffer);
                WriteShort((short)actualByteCount);
                _memoryStream.Write(buffer[..actualByteCount]);
            }
            else
            {
                var rented = ArrayPool<byte>.Shared.Rent(value.Length * 3); // max expansion: each char -> 3 bytes
                var actualByteCount = Encoding.UTF8.GetBytes(value, rented);
                WriteShort((short)actualByteCount);
                _memoryStream.Write(rented, 0, actualByteCount);
                ArrayPool<byte>.Shared.Return(rented);
            }
        }
        else
        {
            WriteAsBytes(value);
        }
    }

    private void WriteAsShorts(string value)
    {
        foreach (var c in value.AsSpan())
        {
            WriteShort((short) c);
        }
    }
    
    private void WriteAsBytes(string value)
    {
        var chars = value.AsSpan();
        WriteInt(chars.Length << 1);
        foreach (var c in chars)
        {
            var low = (byte) (c & 0xFF); // Low byte
            var high = (byte) ((c >> 8) & 0xFF); // High byte

            WriteByte(low);
            WriteByte(high);
        }
    }

    private void WriteShort(short value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(short)];
        BinaryPrimitives.WriteInt16BigEndian(buffer, value);
        _memoryStream.Write(buffer);
    }
}