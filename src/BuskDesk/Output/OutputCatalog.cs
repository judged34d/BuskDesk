namespace BuskDesk.Output;

public static class OutputCatalog
{
    public static IReadOnlyList<OutputOption> Options { get; } =
    [
        new("null", "Kein Ausgang (Simulation)", "Nur Software testen, kein Hardware-Signal."),
        new("air2dmx", "Air2DMX (WLAN Art-Net)", "JMS Air2DMX — Hotspot, Ziel 2.0.0.1, Universe 0."),
        new("artnet-broadcast", "Art-Net Broadcast", "Universelles Art-Net im LAN."),
        new("artnet-unicast", "Art-Net Unicast", "Art-Net an eine feste IP."),
        new("enttec-pro", "USB DMX Pro / USB2DMX", "Enttec Pro API über COM-Port (FTDI).")
    ];

    public static IDmxOutput Create(string id, OutputSettings settings) => id switch
    {
        "air2dmx" => ArtNetOutput.CreateAir2Dmx(),
        "artnet-broadcast" => ArtNetOutput.CreateBroadcast(settings.ArtNetUniverse),
        "artnet-unicast" => ArtNetOutput.CreateUnicast(
            string.IsNullOrWhiteSpace(settings.ArtNetHost) ? "192.168.1.10" : settings.ArtNetHost,
            settings.ArtNetUniverse),
        "enttec-pro" => new EnttecProOutput(settings.SerialPort),
        _ => new NullDmxOutput()
    };
}

public sealed class OutputSettings
{
    public string SelectedOutputId { get; set; } = "null";
    public string ArtNetHost { get; set; } = "2.0.0.1";
    public int ArtNetUniverse { get; set; }
    public string SerialPort { get; set; } = string.Empty;
}
