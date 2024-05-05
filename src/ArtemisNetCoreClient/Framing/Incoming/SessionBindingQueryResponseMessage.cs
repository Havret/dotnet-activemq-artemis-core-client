using System.Diagnostics;

namespace ActiveMQ.Artemis.Core.Client.Framing.Incoming;

internal readonly struct SessionBindingQueryResponseMessage : IIncomingPacket
{
    public readonly bool Exists;
    public readonly string[] QueueNames;
    public readonly bool AutoCreateQueues;
    public readonly bool AutoCreateAddresses;
    public readonly bool DefaultPurgeOnNoConsumers;
    public readonly int DefaultMaxConsumers;
    public readonly bool? DefaultExclusive;
    public readonly bool? DefaultLastValue;
    public readonly string? DefaultLastValueKey;
    public readonly bool? DefaultNonDestructive;
    public readonly int? DefaultConsumersBeforeDispatch;
    public readonly long? DefaultDelayBeforeDispatch;
    public readonly bool SupportsMulticast;
    public readonly bool SupportsAnycast;
    
    public SessionBindingQueryResponseMessage(ReadOnlySpan<byte> buffer)
    {
        var readBytes = 0;
        readBytes += ArtemisBinaryConverter.ReadBool(buffer, out Exists);
        readBytes += ArtemisBinaryConverter.ReadInt32(buffer[readBytes..], out var numQueues);
        QueueNames = new string[numQueues];
        for (var i = 0; i < numQueues; i++)
        {
            readBytes += ArtemisBinaryConverter.ReadSimpleString(buffer[readBytes..], out QueueNames[i]);
        }
        readBytes += ArtemisBinaryConverter.ReadBool(buffer[readBytes..], out AutoCreateQueues);
        readBytes += ArtemisBinaryConverter.ReadBool(buffer[readBytes..], out AutoCreateAddresses);
        readBytes += ArtemisBinaryConverter.ReadBool(buffer[readBytes..], out DefaultPurgeOnNoConsumers);
        readBytes += ArtemisBinaryConverter.ReadInt32(buffer[readBytes..], out DefaultMaxConsumers);
        readBytes += ArtemisBinaryConverter.ReadNullableBool(buffer[readBytes..], out DefaultExclusive);
        readBytes += ArtemisBinaryConverter.ReadNullableBool(buffer[readBytes..], out DefaultLastValue);
        readBytes += ArtemisBinaryConverter.ReadNullableSimpleString(buffer[readBytes..], out DefaultLastValueKey);
        readBytes += ArtemisBinaryConverter.ReadNullableBool(buffer[readBytes..], out DefaultNonDestructive);
        readBytes += ArtemisBinaryConverter.ReadNullableInt32(buffer[readBytes..], out DefaultConsumersBeforeDispatch);
        readBytes += ArtemisBinaryConverter.ReadNullableInt64(buffer[readBytes..], out DefaultDelayBeforeDispatch);
        readBytes += ArtemisBinaryConverter.ReadBool(buffer[readBytes..], out SupportsMulticast);
        readBytes += ArtemisBinaryConverter.ReadBool(buffer[readBytes..], out SupportsAnycast);
     
        Debug.Assert(readBytes == buffer.Length, $"Expected to read {buffer.Length} bytes but got {readBytes}");
    }
}