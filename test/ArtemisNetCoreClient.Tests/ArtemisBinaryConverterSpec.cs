using ActiveMQ.Artemis.Core.Client.InternalUtilities;
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
    [InlineData(true, new[] { unchecked((byte) -1), unchecked((byte) -1) })]
    [InlineData(false, new[] { unchecked((byte) -1), (byte) 0 })]
    [InlineData(null, new byte[] { 0 })]
    public void should_encode_nullable_bool(bool? value, byte[] encoded)
    {
        // Arrange
        var bufferSize = ArtemisBinaryConverter.GetNullableBoolByteCount(value);
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
        var bufferSize = ArtemisBinaryConverter.GetNullableInt32ByteCount(value);
        var byteBuffer = new byte[bufferSize];

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
        var bufferSize = ArtemisBinaryConverter.GetNullableInt64ByteCount(value);
        var byteBuffer = new byte[bufferSize];

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
    
    [Fact]
    public void Should_encode_guid()
    {
        // Arrange
        var byteBuffer = new byte[ArtemisBinaryConverter.GuidByteCount];
        var guid = Guid.Parse("c69fdd4e-afa3-46bb-a6ef-f2d5fa4172fa");

        // Act
        var writtenBytes = ArtemisBinaryConverter.WriteGuid(ref byteBuffer.AsSpan().GetReference(), guid);

        // Assert
        var expected = new byte[]
        {
            unchecked((byte) -58),
            unchecked((byte) -97),
            unchecked((byte) -35),
            78,
            unchecked((byte) -81),
            unchecked((byte) -93),
            70,
            unchecked((byte) -69),
            unchecked((byte) -90),
            unchecked((byte) -17),
            unchecked((byte) -14),
            unchecked((byte) -43),
            unchecked((byte) -6),
            65,
            114,
            unchecked((byte) -6)
        };
        Assert.Equal(expected, byteBuffer);
        Assert.Equal(ArtemisBinaryConverter.GuidByteCount, writtenBytes);
    }

    [Fact]
    public void Should_decode_guid()
    {
        // Arrange
        var byteBuffer = new byte[]
        {
            unchecked((byte) -58),
            unchecked((byte) -97),
            unchecked((byte) -35),
            78,
            unchecked((byte) -81),
            unchecked((byte) -93),
            70,
            unchecked((byte) -69),
            unchecked((byte) -90),
            unchecked((byte) -17),
            unchecked((byte) -14),
            unchecked((byte) -43),
            unchecked((byte) -6),
            65,
            114,
            unchecked((byte) -6)
        };

        // Act
        var readBytes = ArtemisBinaryConverter.ReadGuid(byteBuffer, out var value);

        // Assert
        Assert.Equal(Guid.Parse("c69fdd4e-afa3-46bb-a6ef-f2d5fa4172fa"), value);
        Assert.Equal(ArtemisBinaryConverter.GuidByteCount, readBytes);
    }

    [Theory]
    [InlineData(new byte[]
    {
        1,
        unchecked((byte) -58),
        unchecked((byte) -97),
        unchecked((byte) -35),
        78,
        unchecked((byte) -81),
        unchecked((byte) -93),
        70,
        unchecked((byte) -69),
        unchecked((byte) -90),
        unchecked((byte) -17),
        unchecked((byte) -14),
        unchecked((byte) -43),
        unchecked((byte) -6),
        65,
        114,
        unchecked((byte) -6)
    }, "c69fdd4e-afa3-46bb-a6ef-f2d5fa4172fa")]
    [InlineData(new byte[] { 0 }, null)]
    public void Should_encode_nullable_guid(byte[] encoded, string? stringGuid)
    {
        // Arrange
        Guid? guid = stringGuid != null ? Guid.Parse(stringGuid) : null;
        var byteBuffer = new byte[ArtemisBinaryConverter.GetNullableGuidByteCount(guid)];

        // Act
        var writtenBytes = ArtemisBinaryConverter.WriteNullableGuid(ref byteBuffer.AsSpan().GetReference(), guid);

        // Assert
        Assert.Equal(encoded, byteBuffer);
        Assert.Equal(encoded.Length, writtenBytes);
    }

    [Theory]
    [InlineData(new byte[]
    {
        1,
        unchecked((byte) -58),
        unchecked((byte) -97),
        unchecked((byte) -35),
        78,
        unchecked((byte) -81),
        unchecked((byte) -93),
        70,
        unchecked((byte) -69),
        unchecked((byte) -90),
        unchecked((byte) -17),
        unchecked((byte) -14),
        unchecked((byte) -43),
        unchecked((byte) -6),
        65,
        114,
        unchecked((byte) -6)
    }, "c69fdd4e-afa3-46bb-a6ef-f2d5fa4172fa")]
    [InlineData(new byte[] { 0 }, null)]
    public void Should_decode_nullable_guid(byte[] encoded, string? expected)
    {
        // Act
        var readBytes = ArtemisBinaryConverter.ReadNullableGuid(encoded, out var value);

        // Assert
        Assert.Equal(expected, value?.ToString());
        Assert.Equal(encoded.Length, readBytes);
    }

    [Theory]
    [InlineData(null, new byte[] { 0 })] // null
    [InlineData(true, new byte[] { 2, unchecked((byte) -1) })] // true
    [InlineData(false, new byte[] { 2, 0 })] // false
    [InlineData((byte) 125, new byte[] { 3, 125 })] // byte
    [InlineData(new byte[] { 1, 2, 3, 4, 5, 6 }, new byte[] { 4, 0, 0, 0, 6, 1, 2, 3, 4, 5, 6 })] // byte[]
    [InlineData(125, new byte[] { 6, 0, 0, 0, 125 })] // int
    [InlineData((short) 125, new byte[] { 5, 0, 125 })] // short
    [InlineData(long.MaxValue, new byte[] { 7, 127, 255, 255, 255, 255, 255, 255, 255 })] // long
    [InlineData(12.23F, new byte[] { 8, 65, 67, unchecked((byte) -82), 20 })] // float
    [InlineData(12.23D, new byte[] { 9, 64, 40, 117, unchecked((byte) -62), unchecked((byte) -113), 92, 40, unchecked((byte) -10) })] // double
    [InlineData("abcdefgh", new byte[] { 10, 0, 0, 0, 16, 97, 0, 98, 0, 99, 0, 100, 0, 101, 0, 102, 0, 103, 0, 104, 0 })] // string
    [InlineData('a', new byte[] { 11, 0, 97 })] // char
    public void Should_encode_nullable_object(object? obj, byte[] encoded)
    {
        // Arrange
        var byteBuffer = new byte[ArtemisBinaryConverter.GetNullableObjectByteCount(obj)];

        // Act
        var writtenBytes = ArtemisBinaryConverter.WriteNullableObject(ref byteBuffer.AsSpan().GetReference(), obj);

        // Assert
        Assert.Equal(encoded, byteBuffer);
        Assert.Equal(encoded.Length, writtenBytes);
    }
    
    [Theory]
    [InlineData(new byte[] { 0 }, null)] // null
    [InlineData(new byte[] { 2, unchecked((byte) -1) }, true)] // true
    [InlineData(new byte[] { 2, 0 }, false)] // false
    [InlineData(new byte[] { 3, 125 }, (byte) 125)] // byte
    [InlineData(new byte[] { 4, 0, 0, 0, 6, 1, 2, 3, 4, 5, 6 }, new byte[] { 1, 2, 3, 4, 5, 6 })] // byte[]
    [InlineData(new byte[] { 5, 0, 125 }, (short) 125)] // short
    [InlineData(new byte[] { 6, 0, 0, 0, 125 }, 125)] // int
    [InlineData(new byte[] { 7, 127, 255, 255, 255, 255, 255, 255, 255 }, long.MaxValue)] // long
    [InlineData(new byte[] { 8, 65, 67, unchecked((byte) -82), 20 }, 12.23F)] // float
    [InlineData(new byte[] { 9, 64, 40, 117, unchecked((byte) -62), unchecked((byte) -113), 92, 40, unchecked((byte) -10) }, 12.23D)] // double
    [InlineData(new byte[] { 10, 0, 0, 0, 16, 97, 0, 98, 0, 99, 0, 100, 0, 101, 0, 102, 0, 103, 0, 104, 0 }, "abcdefgh")] // string
    [InlineData(new byte[] { 11, 0, 97 }, 'a')] // char
    public void Should_decode_nullable_object(byte[] encoded, object? expected)
    {
        // Act
        var readBytes = ArtemisBinaryConverter.ReadNullableObject(encoded, out var value);

        // Assert
        Assert.Equal(expected, value);
        Assert.Equal(encoded.Length, readBytes);
    }
}