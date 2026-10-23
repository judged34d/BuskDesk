using System.Text.Json.Serialization;
using BuskDesk.Core;

namespace BuskDesk.Model;

public sealed class FixtureDefinition
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Manufacturer { get; set; } = "Generic";
    public string Model { get; set; } = "Fixture";
    public string ModeName { get; set; } = "Default";
    public List<FixtureChannel> Channels { get; set; } = [];

    [JsonIgnore]
    public int ChannelCount => Channels.Count;

    public string DisplayName => $"{Manufacturer} {Model} · {ModeName} ({ChannelCount}ch)";
}

public sealed class FixtureChannel
{
    public int Offset { get; set; } // 0-based within fixture
    public string Name { get; set; } = "Channel";
    public ChannelRole Role { get; set; } = ChannelRole.Raw;
    public byte DefaultValue { get; set; }
    public List<ChannelRange> Ranges { get; set; } = [];
}

public sealed class ChannelRange
{
    public byte From { get; set; }
    public byte To { get; set; } = 255;
    public string Label { get; set; } = string.Empty;
}

public sealed class PatchedFixture
{
    public string InstanceId { get; set; } = Guid.NewGuid().ToString("N");
    public string DefinitionId { get; set; } = string.Empty;
    public string Name { get; set; } = "Fixture";
    public int StartAddress { get; set; } = 1;
    public string Group { get; set; } = "Default";

    [JsonIgnore]
    public FixtureDefinition? Definition { get; set; }

    public int EndAddress =>
        Definition is null ? StartAddress : StartAddress + Math.Max(0, Definition.ChannelCount - 1);
}

public sealed class SceneLook
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "Szene";
    public int FadeMs { get; set; } = 1000;
    public byte[] Channels { get; set; } = new byte[DmxUniverse.ChannelCount];
}

public sealed class SubmasterSlot
{
    public string Name { get; set; } = "—";
    public byte[]? Channels { get; set; }
    public byte Level { get; set; }
    public bool HasContent => Channels is { Length: > 0 };
}
