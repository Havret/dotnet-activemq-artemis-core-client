using System.Diagnostics;
using ActiveMQ.Artemis.Core.Client.InternalUtilities;

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
        var offset = 0;
        offset += ArtemisBinaryConverter.ReadBool(buffer, out Exists);
        offset += ArtemisBinaryConverter.ReadInt32(buffer[offset..], out var numQueues);
        QueueNames = new string[numQueues];
        for (var i = 0; i < numQueues; i++)
        {
            offset += ArtemisBinaryConverter.ReadSimpleString(buffer[offset..], out QueueNames[i]);
        }
        offset += ArtemisBinaryConverter.ReadBool(buffer[offset..], out AutoCreateQueues);
        offset += ArtemisBinaryConverter.ReadBool(buffer[offset..], out AutoCreateAddresses);
        offset += ArtemisBinaryConverter.ReadBool(buffer[offset..], out DefaultPurgeOnNoConsumers);
        offset += ArtemisBinaryConverter.ReadInt32(buffer[offset..], out DefaultMaxConsumers);
        offset += ArtemisBinaryConverter.ReadNullableBool(buffer[offset..], out DefaultExclusive);
        offset += ArtemisBinaryConverter.ReadNullableBool(buffer[offset..], out DefaultLastValue);
        offset += ArtemisBinaryConverter.ReadNullableSimpleString(buffer[offset..], out DefaultLastValueKey);
        offset += ArtemisBinaryConverter.ReadNullableBool(buffer[offset..], out DefaultNonDestructive);
        offset += ArtemisBinaryConverter.ReadNullableInt32(buffer[offset..], out DefaultConsumersBeforeDispatch);
        offset += ArtemisBinaryConverter.ReadNullableInt64(buffer[offset..], out DefaultDelayBeforeDispatch);
        offset += ArtemisBinaryConverter.ReadBool(buffer[offset..], out SupportsMulticast);
        offset += ArtemisBinaryConverter.ReadBool(buffer[offset..], out SupportsAnycast);
        
        Debug.Assert(offset == buffer.Length, "offset == buffer.Length");
    }
}