using System.Diagnostics;

namespace ActiveMQ.Artemis.Core.Client.Framing;

internal readonly struct PacketHeader
{
    public const int HeaderSize = sizeof(int) + sizeof(byte) + sizeof(long);

    public PacketHeader(ReadOnlySpan<byte> buffer)
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