namespace BuskDesk.Output;

public sealed class NullDmxOutput : IDmxOutput
{
    public string Id => "null";
    public string DisplayName => "Kein Ausgang (Simulation)";
    public string Description => "Sendet nichts — UI und Engine laufen lokal zum Testen.";
    public bool IsConnected { get; private set; }
    public string StatusText => IsConnected ? "Simulation aktiv" : "Getrennt";

    public Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        IsConnected = true;
        return Task.CompletedTask;
    }

    public Task DisconnectAsync()
    {
        IsConnected = false;
        return Task.CompletedTask;
    }

    public Task SendUniverseAsync(ReadOnlyMemory<byte> channels512, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public ValueTask DisposeAsync()
    {
        IsConnected = false;
        return ValueTask.CompletedTask;
    }
}
