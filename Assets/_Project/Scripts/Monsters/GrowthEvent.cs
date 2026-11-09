public class GrowthEvent
{
    public GrowthType Type { get; }
    public MonsterInstance From { get; }
    public MonsterInstance To { get; }
    public string Message { get; }

    public GrowthEvent(GrowthType type, MonsterInstance from, MonsterInstance to, string message)
    {
        Type = type;
        From = from;
        To = to;
        Message = message;
    }
}