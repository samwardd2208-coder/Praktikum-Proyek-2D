using UnityEngine;
 
// Composition Root: satu-satunya tempat service dibuat. Taruh di Scn_Bootstrap.
public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private MonsterDatabase monsterDatabase;
    [SerializeField] private ScreenFader fader;
 
    [Header("P6")]
    [SerializeField] private DialogueRunner dialogue;     // Canvas_Dialogue
    [SerializeField] private AudioService audioService;   // objek AudioService
    [SerializeField] private PauseMenu pauseMenu;         // Canvas_Pause
    [SerializeField] private ToastView toast;             // objek ToastView
 
    private GameServices services;
 
    void Awake()
    {
        var gate = new InputGate(new InputSystemMoveInput(),
                                 new InputSystemActionInput());
        services = new GameServices
        {
            Input = gate, Database = monsterDatabase,
            Party = new PlayerParty(), Battle = new BattleSession(),
        };
        services.Scenes = new SceneFlow(this, fader, services);
 
        // P6: service baru
        services.Flags = new StoryFlags();
        services.Save = new SaveService();
        services.Audio = audioService;
        services.Dialogue = dialogue;
        services.Toast = toast;
 
        // objek statis yang butuh AudioService
        TextMenu.Audio = audioService;
        AreaZone.Audio = audioService;
 
        // setelah Input dan Audio terisi
        if (dialogue != null) dialogue.Init(services);
        if (pauseMenu != null) pauseMenu.Init(services);
    }
 
    // P6: game dimulai dari layar judul, bukan langsung ke dunia
    
    void Start()
     {
         Debug.Log("[Bootstrap] menuju " + SceneFlow.TitleScene);
         services.Scenes.GoTo(SceneFlow.TitleScene);
     }
 
    // P6: tombol M = mute / unmute
    void Update()
    {
        if (services == null || services.Audio == null) return;
        if (!HotKeys.Mute) return;
 
        bool muted = services.Audio.ToggleMute();
        if (services.Toast != null)
            services.Toast.Show(muted ? "Suara dimatikan" : "Suara dinyalakan");
    }
}