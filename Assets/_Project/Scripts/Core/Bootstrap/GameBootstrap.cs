using UnityEngine;
using UnityEngine.InputSystem; // ← TAMBAHKAN BARIS INI

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private string firstScene = "Scn_Title";
    [SerializeField] private MonsterDatabase monsterDatabase;
    [SerializeField] private ScreenFader fader;
    [SerializeField] private PauseMenu pauseMenu;

    // 3 slot AudioSource untuk audio service
    [SerializeField] private AudioSource musicA;
    [SerializeField] private AudioSource musicB;
    [SerializeField] private AudioSource sfxSource;

    private GameServices services;

    void Awake()
    {
        var gate = new InputGate(new InputSystemMoveInput(), new InputSystemActionInput());
        services = new GameServices
        {
            Input = gate,
            Database = monsterDatabase,
            Party = new PlayerParty(),
            Battle = new BattleSession(),
            Flags = new StoryFlags(),
            Save = new SaveService()
        };
        services.Scenes = new SceneFlow(this, fader, services);

        // Inisialisasi AudioService
        if (musicA != null && musicB != null && sfxSource != null)
        {
            services.Audio = new AudioService(musicA, musicB, sfxSource, this);
        }

        DialogueRunner dialogueRunner = FindObjectOfType<DialogueRunner>();
        if (dialogueRunner != null)
        {
            // Berikan AudioService ke DialogueRunner
            dialogueRunner.Construct(services.Input, services.Audio);
            services.Dialogue = dialogueRunner;
        }

        services.Save.Load();
        if (pauseMenu != null) pauseMenu.Construct(services);
    }

    void Start() => services.Scenes.GoTo(firstScene);

    // ✅ PERBAIKAN: Gunakan Keyboard.current (New Input System), bukan UnityEngine.Input
    void Update()
    {
        if (services.Audio != null && Keyboard.current != null)
        {
            if (Keyboard.current[Key.M].wasPressedThisFrame)
            {
                services.Audio.ToggleMute();
            }
        }
    }
}