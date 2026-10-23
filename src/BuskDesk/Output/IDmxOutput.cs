namespace BuskDesk.Output;

public interface IDmxOutput : IAsyncDisposable
{
    string Id { get; }
    string DisplayName { get; }
    string Description { get; }
    bool IsConnected { get; }
    string StatusText { get; }

    Task ConnectAsync(CancellationToken cancellationToken = default);
    Task DisconnectAsync();
    Task SendUniverseAsync(ReadOnlyMemory<byte> channels512, CancellationToken cancellationToken = default);
}

public sealed record OutputOption(string Id, string DisplayName, string Description);
