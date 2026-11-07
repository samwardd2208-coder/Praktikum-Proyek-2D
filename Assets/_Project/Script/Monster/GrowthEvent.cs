public enum GrowthEventType { LevelUp, Evolved }

// Catatan kejadian saat monster mendapat EXP (naik level / berevolusi).
public class GrowthEvent
{
    public GrowthEventType Type { get; }
    public string Message { get; }

    public GrowthEvent(GrowthEventType type, string message)
    {
        Type = type;
        Message = message;
    }
}
