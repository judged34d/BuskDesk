namespace BuskDesk.Core;

/// <summary>One DMX512 universe (512 channels, 1-based addressing in the UI).</summary>
public sealed class DmxUniverse
{
    public const int ChannelCount = 512;

    private readonly byte[] _channels = new byte[ChannelCount];

    public byte this[int address1Based]
    {
        get
        {
            Validate(address1Based);
            return _channels[address1Based - 1];
        }
        set
        {
            Validate(address1Based);
            _channels[address1Based - 1] = value;
        }
    }

    public void Clear() => Array.Clear(_channels);

    public void CopyTo(Span<byte> destination)
    {
        if (destination.Length < ChannelCount)
        {
            throw new ArgumentException("Destination too small for a DMX universe.");
        }

        _channels.AsSpan().CopyTo(destination);
    }

    public byte[] Snapshot()
    {
        var copy = new byte[ChannelCount];
        _channels.CopyTo(copy, 0);
        return copy;
    }

    public void Load(ReadOnlySpan<byte> source)
    {
        var length = Math.Min(ChannelCount, source.Length);
        source[..length].CopyTo(_channels);
        if (length < ChannelCount)
        {
            _channels.AsSpan(length).Clear();
        }
    }

    private static void Validate(int address1Based)
    {
        if (address1Based is < 1 or > ChannelCount)
        {
            throw new ArgumentOutOfRangeException(nameof(address1Based), "DMX address must be 1–512.");
        }
    }
}
