using System.IO.Ports;

namespace BuskDesk.Output;

/// <summary>
/// Enttec DMX USB Pro widget protocol (label 6). Also covers many clones
/// including JMS USB2DMX PRO when they emulate Pro framing over FTDI VCP.
/// </summary>
public sealed class EnttecProOutput : IDmxOutput
{
    private SerialPort? _port;

    public EnttecProOutput(string? portName = null)
    {
        PortName = portName ?? string.Empty;
    }

    public string Id => "enttec-pro";
    public string DisplayName => "USB DMX Pro / USB2DMX";
    public string Description =>
        "Enttec DMX USB Pro API (7E…E7). Funktioniert mit vielen FTDI-Clones inkl. JMS USB2DMX PRO.";
    public string PortName { get; set; }
    public bool IsConnected => _port is { IsOpen: true };
    public string StatusText =>
        IsConnected ? $"Pro → {PortName}" : string.IsNullOrWhiteSpace(PortName) ? "Kein COM-Port" : $"Bereit ({PortName})";

    public static IReadOnlyList<string> ListPorts() => SerialPort.GetPortNames().OrderBy(p => p).ToArray();

    public Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(PortName))
        {
            var ports = ListPorts();
            if (ports.Count == 0)
            {
                throw new InvalidOperationException("Kein COM-Port gefunden. USB2DMX/Enttec-Treiber prüfen.");
            }

            PortName = ports[0];
        }

        _port?.Dispose();
        _port = new SerialPort(PortName)
        {
            BaudRate = 57600,
            DataBits = 8,
            Parity = Parity.None,
            StopBits = StopBits.One,
            Handshake = Handshake.None,
            WriteTimeout = 1000,
            ReadTimeout = 1000
        };
        _port.Open();
        return Task.CompletedTask;
    }

    public Task DisconnectAsync()
    {
        if (_port is not null)
        {
            if (_port.IsOpen)
            {
                _port.Close();
            }

            _port.Dispose();
            _port = null;
        }

        return Task.CompletedTask;
    }

    public Task SendUniverseAsync(ReadOnlyMemory<byte> channels512, CancellationToken cancellationToken = default)
    {
        if (_port is not { IsOpen: true })
        {
            return Task.CompletedTask;
        }

        var frame = BuildProFrame(channels512.Span);
        _port.Write(frame, 0, frame.Length);
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync().ConfigureAwait(false);
    }

    /// <summary>Label 6 = Send DMX packet. Payload = start code 0 + 512 channels.</summary>
    internal static byte[] BuildProFrame(ReadOnlySpan<byte> channels)
    {
        const byte label = 6;
        var dataLength = 513; // start code + 512
        var frame = new byte[5 + dataLength];
        frame[0] = 0x7E;
        frame[1] = label;
        frame[2] = (byte)(dataLength & 0xFF);
        frame[3] = (byte)((dataLength >> 8) & 0xFF);
        frame[4] = 0x00; // DMX start code
        var copy = Math.Min(512, channels.Length);
        channels[..copy].CopyTo(frame.AsSpan(5));
        frame[^1] = 0xE7;
        return frame;
    }
}
