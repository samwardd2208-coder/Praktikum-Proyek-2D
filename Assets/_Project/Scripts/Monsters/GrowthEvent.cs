public enum GrowthType
{
    LevelUp,
    Evolution
}
 
public class GrowthEvent
{
    public GrowthType Type { get; }
    public MonsterData From { get; }
    public MonsterData To { get; }
    public string Message { get; }
 
    public GrowthEvent(GrowthType type, MonsterData from, MonsterData to, string message)
    {
        Type = type;
        From = from;
        To = to;
        Message = message;
    }
}
 