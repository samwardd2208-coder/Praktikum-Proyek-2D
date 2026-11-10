using UnityEngine;

// Composition Root: satu-satunya tempat "new" service.
public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private string firstScene = "Scn_World";
    [SerializeField] private MonsterDatabase monsterDatabase;
    [SerializeField] private ScreenFader fader;
    private GameServices services;

        void Awake()
        {
            var gate = new InputGate(new InputSystemMoveInput(),
                                 new InputSystemActionInput());
            services = new GameServices
            {
                Input = gate,
                Database = monsterDatabase,
                Party = new PlayerParty(),
                Battle = new BattleSession(),
            };
            services.Scenes = new SceneFlow(this, fader, services);

            // --- PINDAHKAN KODE INI KE DALAM AWAKE() ---
            DialogueRunner dialogueRunner = FindObjectOfType<DialogueRunner>();
        
            if (dialogueRunner != null && services.Input != null)
            {
                dialogueRunner.Construct(services.Input);
            }
            // -------------------------------------------
            services.Flags = new StoryFlags(); // Tambahkan ini di dalam blok new GameServices
        } // <-- Kurung kurawal tutup Awake() ada di sini

        void Start() => services.Scenes.GoTo(firstScene);
}
