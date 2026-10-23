using BuskDesk.Core;
using BuskDesk.Model;

namespace BuskDesk.Services;

/// <summary>Plays one A↔B move pattern into the engine effect layer.</summary>
public sealed class MovePatternPlayer
{
    private readonly DmxEngine _engine;
    private readonly object _gate = new();
    private MovePattern? _active;
    private PatchedFixture? _fixture;
    private long _startedAtMs;
    private bool _running;

    public MovePatternPlayer(DmxEngine engine)
    {
        _engine = engine;
        _engine.FrameRendered += OnFrame;
    }

    public bool IsPlaying
    {
        get { lock (_gate) return _running; }
    }

    public string? ActivePatternId
    {
        get { lock (_gate) return _active?.Id; }
    }

    public void Start(MovePattern pattern, PatchedFixture fixture)
    {
        lock (_gate)
        {
            _active = pattern;
            _fixture = fixture;
            _startedAtMs = Environment.TickCount64;
            _running = true;
        }
    }

    public void Stop()
    {
        lock (_gate)
        {
            _running = false;
            _active = null;
            _fixture = null;
        }

        _engine.ClearEffectOverrides();
    }

    private void OnFrame()
    {
        MovePattern pattern;
        PatchedFixture fixture;
        long started;
        lock (_gate)
        {
            if (!_running || _active is null || _fixture is null) return;
            pattern = _active;
            fixture = _fixture;
            started = _startedAtMs;
        }

        var oneWay = Math.Max(0.25, pattern.OneWaySeconds);
        var cycle = oneWay * 2.0; // A→B→A
        var elapsedSec = (Environment.TickCount64 - started) / 1000.0;
        var phase = elapsedSec % cycle;
        double t;
        if (phase <= oneWay)
        {
            t = phase / oneWay; // 0→1 A to B
        }
        else
        {
            t = 1.0 - ((phase - oneWay) / oneWay); // 1→0 B to A
        }

        t = Math.Clamp(t, 0, 1);
        WriteLerp(fixture, ChannelRole.Pan, pattern.PanA, pattern.PanB, t);
        WriteLerp(fixture, ChannelRole.Tilt, pattern.TiltA, pattern.TiltB, t);
        if (pattern.PanFineA is byte pfa && pattern.PanFineB is byte pfb)
        {
            WriteLerp(fixture, ChannelRole.PanFine, pfa, pfb, t);
        }

        if (pattern.TiltFineA is byte tfa && pattern.TiltFineB is byte tfb)
        {
            WriteLerp(fixture, ChannelRole.TiltFine, tfa, tfb, t);
        }

        if (pattern.IncludeIntensity)
        {
            FixtureChannelMap.WriteRole(fixture, ChannelRole.Intensity, pattern.Intensity, WriteEffect);
        }

        if (pattern.IncludeColor)
        {
            FixtureChannelMap.WriteRole(fixture, ChannelRole.ColorRed, pattern.Red, WriteEffect);
            FixtureChannelMap.WriteRole(fixture, ChannelRole.ColorGreen, pattern.Green, WriteEffect);
            FixtureChannelMap.WriteRole(fixture, ChannelRole.ColorBlue, pattern.Blue, WriteEffect);
            FixtureChannelMap.WriteRole(fixture, ChannelRole.ColorWhite, pattern.White, WriteEffect);
        }

        if (pattern.IncludeGobo)
        {
            FixtureChannelMap.WriteRole(fixture, ChannelRole.Gobo, pattern.Gobo, WriteEffect);
        }

        if (pattern.IncludeStrobe)
        {
            FixtureChannelMap.WriteRole(fixture, ChannelRole.Strobe, pattern.Strobe, WriteEffect);
        }
    }

    private void WriteLerp(PatchedFixture fixture, ChannelRole role, byte a, byte b, double t)
    {
        var value = (byte)Math.Clamp((int)Math.Round(a + (b - a) * t), 0, 255);
        FixtureChannelMap.WriteRole(fixture, role, value, WriteEffect);
    }

    private void WriteEffect(int address, byte value) =>
        _engine.SetEffectOverride(address, value);
}
