using Xunit;

namespace ActiveMQ.Artemis.Core.Client.Tests;

public class ArtemisBinaryConverterSpec
{
    [Fact]
    public void should_encode_byte()
    {
        // Arrange
        var byteBuffer = new byte[1];

        // Act
        var writtenBytes = ArtemisBinaryConverter.WriteByte(ref byteBuffer.AsSpan().GetReference(), 125);

        // Assert
        Assert.Equal([125], byteBuffer);
        Assert.Equal(1, writtenBytes);
    }
    
    [Fact]
    public void should_decode_byte()
    {
        // Arrange
        var byteBuffer = new byte[]{125};

        // Act
        var readBytes = ArtemisBinaryConverter.ReadByte(byteBuffer, out var value);

        // Assert
        Assert.Equal(125, value);
        Assert.Equal(1, readBytes);
    }
    
    [Theory]
    [InlineData(true, new[] { unchecked((byte) -1) })]
    [InlineData(false, new byte[] { 0 })]
    public void should_encode_bool(bool value, byte[] encoded)
    {
        // Arrange
        var byteBuffer = new byte[1];

        // Act
        var writtenBytes = ArtemisBinaryConverter.WriteBool(ref byteBuffer.AsSpan().GetReference(), value);

        // Assert
        Assert.Equal(encoded, byteBuffer);
        Assert.Equal(1, writtenBytes);
    }
    
    [Theory]
    [InlineData(new[] { unchecked((byte) -1) }, true)]
    [InlineData(new byte[] { 0 }, false)]
    public void should_decode_bool(byte[] encoded, bool expected)
    {
        // Act
        var readBytes = ArtemisBinaryConverter.ReadBool(encoded, out var value);

        // Assert
        Assert.Equal(expected, value);
        Assert.Equal(1, readBytes);
    }

    [Theory]
    [InlineData(true, new[] { unchecked((byte) -1), unchecked((byte) -1) }, 2)]
    [InlineData(false, new[] { unchecked((byte) -1), (byte) 0 }, 2)]
    [InlineData(null, new byte[] { 0 }, 1)]
    public void should_encode_nullable_bool(bool? value, byte[] encoded, int bufferSize)
    {
        // Arrange
        var byteBuffer = new byte[bufferSize];

        // Act
        var writtenBytes = ArtemisBinaryConverter.WriteNullableBool(ref byteBuffer.AsSpan().GetReference(), value);

        // Assert
        Assert.Equal(encoded, byteBuffer);
        Assert.Equal(encoded.Length, writtenBytes);
    }
    
    [Theory]
    [InlineData(new[] { unchecked((byte) -1), unchecked((byte) -1) }, true)]
    [InlineData(new[] { unchecked((byte) -1), (byte) 0 }, false)]
    [InlineData(new byte[] { 0 }, null)]
    public void should_decode_nullable_bool(byte[] encoded, bool? expected)
    {
        // Act
        var readBytes = ArtemisBinaryConverter.ReadNullableBool(encoded, out var value);

        // Assert
        Assert.Equal(expected, value);
        Assert.Equal(encoded.Length, readBytes);
    }
    
    [Fact]
    public void should_encode_int()
    {
        // Arrange
        var byteBuffer = new byte[4];

        // Act
        var writtenBytes = ArtemisBinaryConverter.WriteInt32(ref byteBuffer.AsSpan().GetReference(), 125);

        // Assert
        Assert.Equal([0, 0, 0, 125], byteBuffer);
        Assert.Equal(4, writtenBytes);
    }
    
    [Fact]
    public void should_decode_int()
    {
        // Arrange
        var byteBuffer = new byte[] { 0, 0, 0, 125 };

        // Act
        var readBytes = ArtemisBinaryConverter.ReadInt32(byteBuffer, out var value);

        // Assert
        Assert.Equal(125, value);
        Assert.Equal(4, readBytes);
    }
    
    [Theory]
    [InlineData(170, new byte[] { unchecked((byte) -1), 0, 0, 0, unchecked((byte) -86) })]
    [InlineData(null, new byte[] { 0 })]
    public void should_encode_nullable_int(int? value, byte[] encoded)
    {
        // Arrange
        var byteBuffer = new byte[encoded.Length];

        // Act
        var writtenBytes = ArtemisBinaryConverter.WriteNullableInt32(ref byteBuffer.AsSpan().GetReference(), value);

        // Assert
        Assert.Equal(encoded, byteBuffer);
        Assert.Equal(encoded.Length, writtenBytes);
    }
    
    [Theory]
    [InlineData(new byte[] { unchecked((byte) -1), 0, 0, 0, unchecked((byte) -86) }, 170)]
    [InlineData(new byte[] { 0 }, null)]
    public void should_decode_nullable_int(byte[] encoded, int? expected)
    {
        // Act
        var readBytes = ArtemisBinaryConverter.ReadNullableInt32(encoded, out var value);

        // Assert
        Assert.Equal(expected, value);
        Assert.Equal(encoded.Length, readBytes);
    }
    
    [Fact]
    public void should_encode_long()
    {
        // Arrange
        var byteBuffer = new byte[8];
        
        // Act
        var writtenBytes = ArtemisBinaryConverter.WriteInt64(ref byteBuffer.AsSpan().GetReference(), long.MaxValue);
        
        // Assert
        Assert.Equal(
            [
                127,
                unchecked((byte) -1),
                unchecked((byte) -1),
                unchecked((byte) -1),
                unchecked((byte) -1),
                unchecked((byte) -1),
                unchecked((byte) -1),
                unchecked((byte) -1)
            ],
            byteBuffer);
        Assert.Equal(8, writtenBytes);
    }

    [Fact]
    public void should_decode_long()
    {
        // Arrange
        var byteBuffer = new byte[]
        {
            127,
            unchecked((byte) -1),
            unchecked((byte) -1),
            unchecked((byte) -1),
            unchecked((byte) -1),
            unchecked((byte) -1),
            unchecked((byte) -1),
            unchecked((byte) -1)
        };

        // Act
        var readBytes = ArtemisBinaryConverter.ReadInt64(byteBuffer, out var value);

        // Assert
        Assert.Equal(long.MaxValue, value);
        Assert.Equal(8, readBytes);
    }

    [Theory]
    [InlineData(280L, new byte[] { unchecked((byte) -1), 0, 0, 0, 0, 0, 0, 1, 24 })]
    [InlineData(null, new byte[] { 0 })]
    public void should_encode_nullable_long(long? value, byte[] encoded)
    {
        // Arrange
        var byteBuffer = new byte[encoded.Length];

        // Act
        var writtenBytes = ArtemisBinaryConverter.WriteNullableInt64(ref byteBuffer.AsSpan().GetReference(), value);

        // Assert
        Assert.Equal(encoded, byteBuffer);
        Assert.Equal(encoded.Length, writtenBytes);
    }

    [Theory]
    [InlineData(new byte[] { unchecked((byte) -1), 0, 0, 0, 0, 0, 0, 1, 24 }, 280L)]
    [InlineData(new byte[] { 0 }, null)]
    public void should_decode_nullable_long(byte[] encoded, long? expected)
    {
        // Act
        var readBytes = ArtemisBinaryConverter.ReadNullableInt64(encoded, out var value);

        // Assert
        Assert.Equal(expected, value);
        Assert.Equal(encoded.Length, readBytes);
    }

    [Fact]
    public void should_encode_short_string()
    {
        // Arrange
        var str = "abcdefgh";
        var byteCount = ArtemisBinaryConverter.GetStringByteCount(str);
        var byteBuffer = new byte[byteCount];

        // Act
        var writtenBytes = ArtemisBinaryConverter.WriteString(ref byteBuffer.AsSpan().GetReference(), str);

        // Assert
        var expected = new byte[] { 0, 0, 0, 8, 0, 97, 0, 98, 0, 99, 0, 100, 0, 101, 0, 102, 0, 103, 0, 104 };
        Assert.Equal(expected, byteBuffer);
        Assert.Equal(byteCount, writtenBytes);
    }
    
    [Fact]
    public void should_decode_short_string()
    {
        // Arrange
        var byteBuffer = new byte[] { 0, 0, 0, 8, 0, 97, 0, 98, 0, 99, 0, 100, 0, 101, 0, 102, 0, 103, 0, 104 };

        // Act
        var readBytes = ArtemisBinaryConverter.ReadString(byteBuffer, out var value);

        // Assert
        Assert.Equal("abcdefgh", value);
        Assert.Equal(byteBuffer.Length, readBytes);
    }
    
    [Fact]
    public void should_encode_medium_string()
    {
        // Arrange
        var str = "abcdefghijkl";
        var byteCount = ArtemisBinaryConverter.GetStringByteCount(str);
        var byteBuffer = new byte[byteCount];

        // Act
        var writtenBytes = ArtemisBinaryConverter.WriteString(ref byteBuffer.AsSpan().GetReference(), str);
        
        // Assert
        var expected = new byte[] { 0, 0, 0, 12, 0, 12, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108 };
        Assert.Equal(expected, byteBuffer);
        Assert.Equal(byteCount, writtenBytes);
    }
    
    [Fact]
    public void should_decode_medium_string()
    {
        // Arrange
        var byteBuffer = new byte[] {0, 0, 0, 12, 0, 12, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108};

        // Act
        var readBytes = ArtemisBinaryConverter.ReadString(byteBuffer, out var value);

        // Assert
        Assert.Equal("abcdefghijkl", value);
        Assert.Equal(byteBuffer.Length, readBytes);
    }
    
    [Fact]
    public void should_encode_long_string()
    {
        // Arrange
        var str = Enumerable.Repeat(0, 513).Aggregate("", (s, _) => s + "abcdefgh");
        var byteCount = ArtemisBinaryConverter.GetStringByteCount(str);
        var byteBuffer = new byte[byteCount];
        
        // Act
        var writtenBytes = ArtemisBinaryConverter.WriteString(ref byteBuffer.AsSpan().GetReference(), str);

        // Assert
        var expected = File.ReadAllText("long_encoded_string.txt").Split(",").Select(byte.Parse).ToArray();
        Assert.Equal(expected, byteBuffer);
        Assert.Equal(byteCount, writtenBytes);
    }
    
    [Fact]
    public void should_decode_long_string()
    {
        // Arrange
        var byteBuffer = File.ReadAllText("long_encoded_string.txt").Split(",").Select(byte.Parse).ToArray();
        
        // Act
        var readBytes = ArtemisBinaryConverter.ReadString(byteBuffer, out var value);

        // Assert
        var expected = Enumerable.Repeat(0, 513).Aggregate("", (s, _) => s + "abcdefgh");
        Assert.Equal(expected, value);
        Assert.Equal(byteBuffer.Length, readBytes);
    }
    
    [Theory]
    [InlineData("abcdefgh", new byte[] { 1, 0, 0, 0, 8, 0, 97, 0, 98, 0, 99, 0, 100, 0, 101, 0, 102, 0, 103, 0, 104 })]
    [InlineData(null, new byte[] { 0 })]
    public void should_encode_nullable_string(string value, byte[] encoded)
    {
        // Arrange
        var byteBuffer = new byte[encoded.Length];

        // Act
        var writtenBytes = ArtemisBinaryConverter.WriteNullableString(ref byteBuffer.AsSpan().GetReference(), value);

        // Assert
        Assert.Equal(encoded, byteBuffer);
        Assert.Equal(encoded.Length, writtenBytes);
    }
    
    [Theory]
    [InlineData(new byte[] { 1, 0, 0, 0, 8, 0, 97, 0, 98, 0, 99, 0, 100, 0, 101, 0, 102, 0, 103, 0, 104 }, "abcdefgh")]
    [InlineData(new byte[] { 0 }, null)]
    public void should_decode_nullable_string(byte[] encoded, string? expected)
    {
        // Act
        var readBytes = ArtemisBinaryConverter.ReadNullableString(encoded, out var value);

        // Assert
        Assert.Equal(expected, value);
        Assert.Equal(encoded.Length, readBytes);
    }
    
    [Fact]
    public void Should_encode_simple_string()
    {
        // Arrange
        var str = "abcdefgh";
        var byteCount = ArtemisBinaryConverter.GetSimpleStringByteCount(str);
        var byteBuffer = new byte[byteCount];

        // Act
        var writtenBytes = ArtemisBinaryConverter.WriteSimpleString(ref byteBuffer.AsSpan().GetReference(), str);
        
        // Assert
        var expected = new byte[] { 0, 0, 0, 16, 97, 0, 98, 0, 99, 0, 100, 0, 101, 0, 102, 0, 103, 0, 104, 0 };
        Assert.Equal(expected, byteBuffer);
        Assert.Equal(byteCount, writtenBytes);
    }
    
    [Fact]
    public void Should_decode_simple_string()
    {
        // Arrange
        var byteBuffer = new byte[] { 0, 0, 0, 16, 97, 0, 98, 0, 99, 0, 100, 0, 101, 0, 102, 0, 103, 0, 104, 0 };

        // Act
        var readBytes = ArtemisBinaryConverter.ReadSimpleString(byteBuffer, out var value);

        // Assert
        Assert.Equal("abcdefgh", value);
        Assert.Equal(byteBuffer.Length, readBytes);
    }
    
    [Theory]
    [InlineData(new byte[] { 1, 0, 0, 0, 16, 97, 0, 98, 0, 99, 0, 100, 0, 101, 0, 102, 0, 103, 0, 104, 0 }, "abcdefgh")]
    [InlineData(new byte[] { 0 }, null)]
    public void should_decode_nullable_simple_string(byte[] encoded, string? expected)
    {
        // Act
        var readBytes = ArtemisBinaryConverter.ReadNullableSimpleString(encoded, out var value);

        // Assert
        Assert.Equal(expected, value);
        Assert.Equal(encoded.Length, readBytes);
    }
    
    [Theory]
    [InlineData(new byte[] { 1, 0, 0, 0, 16, 97, 0, 98, 0, 99, 0, 100, 0, 101, 0, 102, 0, 103, 0, 104, 0 }, "abcdefgh")]
    [InlineData(new byte[] { 0 }, null)]
    public void should_encode_nullable_simple_string(byte[] encoded, string? expected)
    {
        // Arrange
        var byteCount = ArtemisBinaryConverter.GetNullableSimpleStringByteCount(expected);
        var byteBuffer = new byte[byteCount];

        // Act
        var writtenBytes = ArtemisBinaryConverter.WriteNullableSimpleString(ref byteBuffer.AsSpan().GetReference(), expected);

        // Assert
        Assert.Equal(encoded, byteBuffer);
        Assert.Equal(encoded.Length, writtenBytes);
    }
}