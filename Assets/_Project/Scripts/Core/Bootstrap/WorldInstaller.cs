using UnityEngine;
using System.Collections;

public class WorldInstaller : SceneInstaller
{
    [SerializeField] private PlayerMovement player;
    [SerializeField] private PlayerInteractor interactor;
    [SerializeField] private PartnerFollower follower;
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private MonsterData debugStarter;
    [SerializeField] private PartnerHud partnerHud;

    private BattleSession battle;
    private DialogueRunner dialogueRunner;

    public override void Install(GameServices s)
    {
        player.Construct(s.Input);
        interactor.Construct(s.Input);
        follower.Construct(s.Party, player);

        foreach (WildMonster wild in FindObjectsOfType<WildMonster>())
            wild.Construct(s.Battle, s.Scenes, s.Party, player.transform);

        dialogueRunner = FindObjectOfType<DialogueRunner>();
        foreach (Npc npc in FindObjectsOfType<Npc>())
        {
            npc.Construct(dialogueRunner, s.Flags, s.Party);
        }

        if (partnerHud != null)
        {
            partnerHud.Construct(s.Party);
            partnerHud.SetAreaName("Desa Awal");
        }

        battle = s.Battle;
        battle.Ended += OnBattleEnded;

        // Narasi pembuka: hanya sekali, di atas layar hitam.
        // HoldFade = true membuat SceneFlow TIDAK melakukan fade-in sendiri;
        // fade-in dilakukan Intro() lewat ReleaseFade() setelah narasi selesai.
        if (!s.Flags.Has(StoryFlags.IntroSeen))
        {
            s.Scenes.HoldFade = true;
            StartCoroutine(Intro(s));
        }
        // Kalau intro sudah pernah dilihat, SceneFlow yang memudarkan layar seperti biasa.
    }

    private IEnumerator Intro(GameServices s)
    {
        // Input terkunci selama narasi (SceneFlow juga sudah mengunci).
        s.Input.Lock();
        yield return null;                       // beri 1 frame supaya semua komponen siap

        if (dialogueRunner != null)
        {
            dialogueRunner.Run(IntroLines());
            yield return null;
            while (dialogueRunner.IsRunning) yield return null;
        }
        else
        {
            Debug.LogWarning("WorldInstaller: DialogueRunner tidak ditemukan, narasi dilewati.");
        }

        s.Flags.Set(StoryFlags.IntroSeen);

        // Layar hitam memudar ke dunia, lalu input dibuka.
        yield return s.Scenes.ReleaseFade();
    }

    private IEnumerator IntroLines()
    {
        yield return dialogueRunner.Narrate("Di ujung Pulau Batam, terdapat desa kecil yang damai...");
        yield return dialogueRunner.Narrate("Namun, hutan di utara penuh dengan monster liar yang mengancam.");
    }

    private void OnBattleEnded(BattleResult result, bool wasBoss)
    {
        if (result == BattleResult.Lose && respawnPoint != null)
            player.transform.position = respawnPoint.position;
    }

    void OnDestroy()
    {
        if (battle != null) battle.Ended -= OnBattleEnded;
    }
}