using BuskDesk.Core;
using BuskDesk.Model;

namespace BuskDesk.Services;

public static class FixtureChannelMap
{
    public static int? AbsoluteAddress(PatchedFixture fixture, ChannelRole role)
    {
        if (fixture.Definition is null) return null;
        var ch = fixture.Definition.Channels.FirstOrDefault(c => c.Role == role);
        if (ch is null) return null;
        var addr = fixture.StartAddress + ch.Offset;
        return addr is >= 1 and <= 512 ? addr : null;
    }

    public static void WriteRole(PatchedFixture fixture, ChannelRole role, byte value, Action<int, byte> write)
    {
        var addr = AbsoluteAddress(fixture, role);
        if (addr is int a) write(a, value);
    }

    public static byte ReadRole(PatchedFixture fixture, ChannelRole role, Func<int, byte> read, byte fallback = 0)
    {
        var addr = AbsoluteAddress(fixture, role);
        return addr is int a ? read(a) : fallback;
    }

    public static bool HasPanTilt(PatchedFixture fixture) =>
        AbsoluteAddress(fixture, ChannelRole.Pan) is not null
        && AbsoluteAddress(fixture, ChannelRole.Tilt) is not null;
}
