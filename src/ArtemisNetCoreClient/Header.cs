using System.Diagnostics;

namespace ActiveMQ.Artemis.Core.Client;

internal readonly struct Header
{
    public const int HeaderSize = sizeof(int) + sizeof(byte) + sizeof(long);

    public Header(ReadOnlySpan<byte> buffer)
    {
        var readBytes = ArtemisBinaryConverter.ReadInt32(buffer, out FrameSize);
        readBytes += ArtemisBinaryConverter.ReadByte(buffer[readBytes..], out var packetType);
        PacketType = (PacketType) packetType;
        readBytes += ArtemisBinaryConverter.ReadInt64(buffer[readBytes..], out ChannelId);

        Debug.Assert(readBytes == HeaderSize, $"Expected to read {HeaderSize} bytes but got {readBytes}");
    }

    public readonly int FrameSize;
    public readonly long ChannelId;
    public readonly PacketType PacketType;
}