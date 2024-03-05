using System.Buffers;
using System.Buffers.Binary;
using System.Text;

namespace ActiveMQ.Artemis.Core.Client;

internal class ByteBuffer
{
    private readonly MemoryStream _memoryStream;

    public ByteBuffer()
    {
        _memoryStream = new MemoryStream();
    }
    
    public ByteBuffer(byte[] payload)
    {
        _memoryStream = new MemoryStream(payload, writable: false);
    }

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

    public bool ReadBool()
    {
        var value = ReadByte();
        return value != 0;
    }

    public void WriteByte(byte value)
    {
        Span<byte> buffer = stackalloc byte[1] { value };
        _memoryStream.Write(buffer);
    }
    
    public byte ReadByte()
    {
        Span<byte> buffer = stackalloc byte[1];
        _ = _memoryStream.Read(buffer);
        return buffer[0];
    }
    
    public void WriteInt(int value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(buffer, value);
        _memoryStream.Write(buffer);
    }
    
    public int ReadInt()
    {
        Span<byte> buffer = stackalloc byte[sizeof(int)];
        _ = _memoryStream.Read(buffer);
        return BinaryPrimitives.ReadInt32BigEndian(buffer);
    }

    public void WriteLong(long value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(long)];
        BinaryPrimitives.WriteInt64BigEndian(buffer, value);
        _memoryStream.Write(buffer);
    }
    
    public long ReadLong()
    {
        Span<byte> buffer = stackalloc byte[sizeof(long)];
        _ = _memoryStream.Read(buffer);
        return BinaryPrimitives.ReadInt64BigEndian(buffer);
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

    private short ReadShort()
    {
        Span<byte> buffer = stackalloc byte[sizeof(short)];
        _ = _memoryStream.Read(buffer);
        return BinaryPrimitives.ReadInt16BigEndian(buffer);
    }

    public string ReadString()
    {
        var length = ReadInt();
        if (length < 9)
        {
            return ReadAsShorts(length);
        }
        else if (length < 0xFFF)
        {
            var actualByteCount = ReadShort();
            if (actualByteCount < 128)
            {
                Span<byte> buffer = stackalloc byte[actualByteCount];
                _ = _memoryStream.Read(buffer);
                return Encoding.UTF8.GetString(buffer);
            }
            else
            {
     
                var rented = ArrayPool<byte>.Shared.Rent(actualByteCount);
                try
                {
                    _ = _memoryStream.Read(rented, 0, actualByteCount);
                    return Encoding.UTF8.GetString(rented, 0, actualByteCount);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(rented);
                }
            }
        }
        else
        {
            return ReadAsBytes();
        }
    }
    
    private string ReadAsBytes()
    {
        return string.Empty;
    }

    private string ReadAsShorts(int length)
    {
        Span<char> chars = stackalloc char[length];
        for (int i = 0; i < length; i++)
        {
            var c = (char) ReadShort();
            chars[i] = c;
        }

        return new string(chars);
    }
}