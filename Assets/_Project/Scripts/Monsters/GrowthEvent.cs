public enum GrowthType
{
    LevelUp,
    Evolved
}

public struct GrowthEvent
{
    public GrowthType Type { get; }
    public MonsterData Before { get; }
    public MonsterData After { get; }
    public string Message { get; }

    public GrowthEvent(GrowthType type, MonsterData before, MonsterData after, string message)
    {
        Type = type;
        Before = before;
        After = after;
        Message = message;
    }
}