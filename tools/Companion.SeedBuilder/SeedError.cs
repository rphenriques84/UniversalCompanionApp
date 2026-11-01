namespace Companion.SeedBuilder;

public sealed record SeedError(string File, string Message)
{
    public override string ToString() => $"{File}: {Message}";
}
