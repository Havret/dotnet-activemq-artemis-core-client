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
}