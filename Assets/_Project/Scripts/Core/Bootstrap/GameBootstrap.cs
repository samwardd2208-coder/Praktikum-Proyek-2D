using System.Collections; // <-- PASTIKAN BARIS INI ADA DI ATAS
using UnityEngine;

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
    }

    // PERBAIKAN ADA DI SINI:
    // 1. Ubah 'void' menjadi 'IEnumerator'
    // 2. Tambahkan 'yield return' di depannya
    IEnumerator Start()
    {
        yield return services.Scenes.GoTo(firstScene);
    }
}