using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ActiveMQ.Artemis.Core.Client.InternalUtilities;

internal static class SpanExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref byte GetReference(this Span<byte> span)
    {
        return ref MemoryMarshal.GetReference(span);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref byte GetOffset(this Span<byte> span, int offset)
    {
        return ref span.GetReference().GetOffset(offset);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref byte GetOffset(this ref byte source, int offset)
    {
        return ref Unsafe.Add(ref source, offset);
    }
}