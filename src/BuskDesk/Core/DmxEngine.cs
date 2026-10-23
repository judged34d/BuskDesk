using BuskDesk.Output;

namespace BuskDesk.Core;

/// <summary>
/// Owns the live universe, merge layers, grand master and the 40 Hz output pump.
/// Layer order (low → high): defaults, scene, submasters, desk, probe override, GM, blackout.
/// </summary>
public sealed class DmxEngine : IAsyncDisposable
{
    public const int TargetHz = 40;

    private readonly object _gate = new();
    private readonly DmxUniverse _output = new();
    private readonly byte[] _desk = new byte[DmxUniverse.ChannelCount];
    private readonly byte[] _scene = new byte[DmxUniverse.ChannelCount];
    private readonly byte[] _submaster = new byte[DmxUniverse.ChannelCount];
    private readonly bool[] _probeOverride = new bool[DmxUniverse.ChannelCount];
    private readonly byte[] _probe = new byte[DmxUniverse.ChannelCount];
    private readonly bool[] _intensityMask = new bool[DmxUniverse.ChannelCount];

    private CancellationTokenSource? _loopCts;
    private Task? _loopTask;
    private IDmxOutput _outputDevice = new NullDmxOutput();
    private long _framesSent;
    private string _lastError = string.Empty;

    public DmxEngine()
    {
        Array.Fill(_intensityMask, true); // until fixtures teach us otherwise, GM scales all
    }

    public byte GrandMaster { get; private set; } = 255;
    public bool Blackout { get; private set; }
    public bool IsRunning => _loopTask is { IsCompleted: false };
    public long FramesSent => Interlocked.Read(ref _framesSent);
    public string LastError => _lastError;
    public IDmxOutput OutputDevice
    {
        get
        {
            lock (_gate) return _outputDevice;
        }
    }

    public event Action? FrameRendered;

    public void SetGrandMaster(byte value)
    {
        GrandMaster = value;
        Rebuild();
    }

    public void SetBlackout(bool enabled)
    {
        Blackout = enabled;
        Rebuild();
    }

    public void SetDeskChannel(int address1Based, byte value)
    {
        Validate(address1Based);
        lock (_gate)
        {
            _desk[address1Based - 1] = value;
        }

        Rebuild();
    }

    public void SetDeskSnapshot(ReadOnlySpan<byte> values)
    {
        lock (_gate)
        {
            values[..Math.Min(values.Length, DmxUniverse.ChannelCount)].CopyTo(_desk);
        }

        Rebuild();
    }

    public void ClearDesk()
    {
        lock (_gate) Array.Clear(_desk);
        Rebuild();
    }

    public void SetSceneSnapshot(ReadOnlySpan<byte> values)
    {
        lock (_gate)
        {
            Array.Clear(_scene);
            values[..Math.Min(values.Length, DmxUniverse.ChannelCount)].CopyTo(_scene);
        }

        Rebuild();
    }

    public void ClearScene()
    {
        lock (_gate) Array.Clear(_scene);
        Rebuild();
    }

    public void SetSubmasterSnapshot(ReadOnlySpan<byte> values, byte level)
    {
        lock (_gate)
        {
            Array.Clear(_submaster);
            var n = Math.Min(values.Length, DmxUniverse.ChannelCount);
            for (var i = 0; i < n; i++)
            {
                _submaster[i] = Scale(values[i], level);
            }
        }

        Rebuild();
    }

    public void ClearSubmaster()
    {
        lock (_gate) Array.Clear(_submaster);
        Rebuild();
    }

    public void SetProbeOverride(int address1Based, byte value, bool enabled = true)
    {
        Validate(address1Based);
        lock (_gate)
        {
            var i = address1Based - 1;
            _probeOverride[i] = enabled;
            _probe[i] = value;
        }

        Rebuild();
    }

    public void ClearProbeOverrides()
    {
        lock (_gate)
        {
            Array.Clear(_probeOverride);
            Array.Clear(_probe);
        }

        Rebuild();
    }

    public void SetIntensityMask(Func<int, bool> addressIsIntensity)
    {
        lock (_gate)
        {
            for (var i = 0; i < DmxUniverse.ChannelCount; i++)
            {
                _intensityMask[i] = addressIsIntensity(i + 1);
            }
        }

        Rebuild();
    }

    public byte GetOutputChannel(int address1Based)
    {
        Validate(address1Based);
        lock (_gate) return _output[address1Based];
    }

    public byte[] SnapshotOutput()
    {
        lock (_gate) return _output.Snapshot();
    }

    public async Task SetOutputAsync(IDmxOutput output, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(output);

        IDmxOutput previous;
        lock (_gate)
        {
            previous = _outputDevice;
            _outputDevice = output;
        }

        try
        {
            await previous.DisposeAsync().ConfigureAwait(false);
        }
        catch
        {
            // ignore dispose errors from previous device
        }

        try
        {
            await output.ConnectAsync(cancellationToken).ConfigureAwait(false);
            _lastError = string.Empty;
        }
        catch (Exception ex)
        {
            _lastError = ex.Message;
            throw;
        }
    }

    public void Start()
    {
        if (IsRunning)
        {
            return;
        }

        _loopCts = new CancellationTokenSource();
        _loopTask = Task.Run(() => RunLoopAsync(_loopCts.Token));
    }

    public async Task StopAsync()
    {
        if (_loopCts is null)
        {
            return;
        }

        await _loopCts.CancelAsync().ConfigureAwait(false);
        if (_loopTask is not null)
        {
            try { await _loopTask.ConfigureAwait(false); }
            catch (OperationCanceledException) { }
        }

        _loopCts.Dispose();
        _loopCts = null;
        _loopTask = null;
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
        IDmxOutput device;
        lock (_gate) device = _outputDevice;
        await device.DisposeAsync().ConfigureAwait(false);
    }

    private async Task RunLoopAsync(CancellationToken token)
    {
        var period = TimeSpan.FromMilliseconds(1000.0 / TargetHz);
        var buffer = new byte[DmxUniverse.ChannelCount];

        while (!token.IsCancellationRequested)
        {
            var started = Environment.TickCount64;
            try
            {
                lock (_gate)
                {
                    _output.CopyTo(buffer);
                }

                IDmxOutput device;
                lock (_gate) device = _outputDevice;
                if (device.IsConnected)
                {
                    await device.SendUniverseAsync(buffer, token).ConfigureAwait(false);
                }

                Interlocked.Increment(ref _framesSent);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _lastError = ex.Message;
            }

            FrameRendered?.Invoke();

            var elapsed = Environment.TickCount64 - started;
            var delay = period.TotalMilliseconds - elapsed;
            if (delay > 1)
            {
                try { await Task.Delay(TimeSpan.FromMilliseconds(delay), token).ConfigureAwait(false); }
                catch (OperationCanceledException) { break; }
            }
        }
    }

    private void Rebuild()
    {
        lock (_gate)
        {
            for (var i = 0; i < DmxUniverse.ChannelCount; i++)
            {
                // HTP for intensity-ish layers: scene / submaster / desk
                var value = Max3(_scene[i], _submaster[i], _desk[i]);

                if (_probeOverride[i])
                {
                    value = _probe[i];
                }

                if (Blackout && _intensityMask[i])
                {
                    value = 0;
                }
                else if (_intensityMask[i] && GrandMaster < 255)
                {
                    value = Scale(value, GrandMaster);
                }

                _output[i + 1] = value;
            }
        }
    }

    private static byte Max3(byte a, byte b, byte c) => Math.Max(a, Math.Max(b, c));

    private static byte Scale(byte value, byte master) =>
        (byte)((value * master + 127) / 255);

    private static void Validate(int address1Based)
    {
        if (address1Based is < 1 or > DmxUniverse.ChannelCount)
        {
            throw new ArgumentOutOfRangeException(nameof(address1Based));
        }
    }
}
