using System.Net;
using System.Net.Sockets;
using System.Text;

namespace BuskDesk.Output;

/// <summary>
/// Art-Net ArtDmx sender. Air2DMX is a Wi‑Fi Art-Net node (USB = power only),
/// so it is reached through this output with a dedicated preset.
/// </summary>
public sealed class ArtNetOutput : IDmxOutput
{
    public const int ArtNetPort = 6454;

    private readonly string _id;
    private readonly string _displayName;
    private readonly string _description;
    private UdpClient? _udp;
    private IPEndPoint? _endpoint;
    private byte _sequence;

    public ArtNetOutput(
        string id,
        string displayName,
        string description,
        string targetHost,
        int universeZeroBased = 0,
        int net = 0)
    {
        _id = id;
        _displayName = displayName;
        _description = description;
        TargetHost = targetHost;
        Universe = universeZeroBased;
        Net = net;
    }

    public string Id => _id;
    public string DisplayName => _displayName;
    public string Description => _description;
    public string TargetHost { get; set; }
    public int Universe { get; set; }
    public int Net { get; set; }
    public bool IsConnected => _udp is not null;
    public string StatusText =>
        IsConnected
            ? $"Art-Net → {TargetHost}:{ArtNetPort} · U{Universe}"
            : "Getrennt";

    public static ArtNetOutput CreateBroadcast(int universeZeroBased = 0) =>
        new(
            "artnet-broadcast",
            "Art-Net (Broadcast)",
            "Generisches Art-Net an 255.255.255.255 — Nodes im LAN.",
            "255.255.255.255",
            universeZeroBased);

    public static ArtNetOutput CreateAir2Dmx() =>
        new(
            "air2dmx",
            "Air2DMX (WLAN Art-Net)",
            "JMS Air2DMX Hotspot: PC mit JMS_AIR2DMX_… verbinden, Ziel 2.0.0.1. USB nur Stromversorgung.",
            "2.0.0.1",
            universeZeroBased: 0);

    public static ArtNetOutput CreateUnicast(string host, int universeZeroBased = 0) =>
        new(
            "artnet-unicast",
            "Art-Net (Unicast)",
            $"Art-Net an {host}.",
            host,
            universeZeroBased);

    public Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        _udp?.Dispose();
        _udp = new UdpClient();
        _udp.EnableBroadcast = TargetHost is "255.255.255.255" or "2.255.255.255";
        _endpoint = new IPEndPoint(IPAddress.Parse(TargetHost), ArtNetPort);
        return Task.CompletedTask;
    }

    public Task DisconnectAsync()
    {
        _udp?.Dispose();
        _udp = null;
        _endpoint = null;
        return Task.CompletedTask;
    }

    public async Task SendUniverseAsync(ReadOnlyMemory<byte> channels512, CancellationToken cancellationToken = default)
    {
        if (_udp is null || _endpoint is null)
        {
            return;
        }

        var packet = BuildArtDmx(channels512.Span, Universe, Net, ref _sequence);
        await _udp.SendAsync(packet, _endpoint, cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync().ConfigureAwait(false);
    }

    internal static byte[] BuildArtDmx(ReadOnlySpan<byte> channels, int universe, int net, ref byte sequence)
    {
        var packet = new byte[18 + 512];
        Encoding.ASCII.GetBytes("Art-Net\0").CopyTo(packet.AsSpan(0, 8));
        packet[8] = 0x00; // OpDmx low
        packet[9] = 0x50; // OpDmx high
        packet[10] = 0x00;
        packet[11] = 14; // ProtVer
        sequence = sequence == 255 ? (byte)1 : (byte)(sequence + 1);
        packet[12] = sequence;
        packet[13] = 0; // Physical
        packet[14] = (byte)(universe & 0xFF);
        packet[15] = (byte)(net & 0x7F);
        packet[16] = 0x02; // Length hi (512)
        packet[17] = 0x00; // Length lo
        var copy = Math.Min(512, channels.Length);
        channels[..copy].CopyTo(packet.AsSpan(18));
        return packet;
    }
}
