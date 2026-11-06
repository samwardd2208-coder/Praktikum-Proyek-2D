public class GameServices
{
    public InputGate Input { get; set; }
    public MonsterDatabase Database { get; set; }
    public PlayerParty Party { get; set; }
    public BattleSession Battle { get; set; }
    public SceneFlow Scenes { get; set; }
}