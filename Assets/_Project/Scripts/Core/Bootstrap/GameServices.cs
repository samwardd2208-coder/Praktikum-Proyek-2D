// Wadah semua service yang dibagikan ke tiap scene.
public class GameServices
{
    public InputGate Input;
    public MonsterDatabase Database;
    public PlayerParty Party;
    public BattleSession Battle;
    public SceneFlow Scenes;
    public StoryFlags Flags { get; set; }
    // public SaveService Save { get; set; } // (Akan kita buat detail di Praktikum 5, tapi siapkan variabelnya dulu agar tidak error)
    public ToastView Toast { get; set; }
}
