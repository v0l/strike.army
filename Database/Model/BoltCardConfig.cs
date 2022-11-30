namespace StrikeArmy.Database.Model;

public class BoltCardConfig
{
    public Guid K0 { get; init; } = Guid.NewGuid();
    public Guid K1 { get; init; } = Guid.NewGuid();
    public Guid K2 { get; init; } = Guid.NewGuid();
    public Guid K3 { get; init; } = Guid.NewGuid();
    public Guid K4 { get; init; } = Guid.NewGuid();
    public uint Counter { get; init; } = 0;
}
