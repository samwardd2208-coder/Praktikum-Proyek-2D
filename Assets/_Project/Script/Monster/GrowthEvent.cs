// Tambahkan enum ini di bagian atas file GrowthEvent.cs
public enum GrowthEventType 
{ 
    LevelUp, 
    Evolved 
}

public class GrowthEvent
{
    public MonsterData From;
    public MonsterData To;
    public GrowthEventType Type { get; }
    public string Message { get; }

    public GrowthEvent(GrowthEventType type, string message)
    {
        Type = type;
        Message = message;
    }
}