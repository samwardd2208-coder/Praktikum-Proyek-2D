using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Pindah scene sambil membawa data (lewat GameServices).
// Semua scene dimuat ADDITIVE: Scn_Bootstrap tidak pernah dibongkar dan Scn_World
// tidak pernah dibongkar saat battle, sehingga posisi pemain tetap sama.
public class SceneFlow
{
    public const string TitleScene = "Scn_Title";
    public const string WorldScene = "Scn_World";
    public const string BattleScene = "Scn_Battle";

    private readonly MonoBehaviour runner;   // tempat menjalankan coroutine
    private readonly ScreenFader fader;
    private readonly GameServices services;

    private string contentScene;             // scene konten aktif (Title atau World)
    private bool transitioning;

    // true = jangan fade-in otomatis setelah scene baru siap (dipakai narasi pembuka).
    public bool HoldFade;

    public string CurrentScene => contentScene;
    public bool InBattle { get; private set; }

    // PauseMenu memakai ini: sibuk saat pindah scene atau saat battle.
    public bool IsBusy => transitioning || InBattle;

    public SceneFlow(MonoBehaviour runner, ScreenFader fader, GameServices services)
    {
        this.runner = runner;
        this.fader = fader;
        this.services = services;
    }

    // ---- Lepas layar hitam yang ditahan HoldFade ----
    public IEnumerator ReleaseFade()
    {
        HoldFade = false;
        if (fader != null) yield return fader.FadeIn();
    }

    // ---- Pindah ke scene konten (Title / World) ----
    public void GoTo(string sceneName)
    {
        if (transitioning) return;
        runner.StartCoroutine(GoToRoutine(sceneName));
    }

    private IEnumerator GoToRoutine(string sceneName)
    {
        transitioning = true;
        HoldFade = false;
        services.Input.Lock();
        if (fader != null) yield return fader.FadeOut();

        if (!string.IsNullOrEmpty(contentScene))
        {
            AsyncOperation unload = SceneManager.UnloadSceneAsync(contentScene);
            if (unload != null) yield return unload;
        }

        yield return LoadAndInstall(sceneName);
        contentScene = sceneName;

        // Jika Install() memulai dialog (narasi pembuka), biarkan DialogueRunner
        // yang membuka kunci setelah dialog selesai.
        bool dialogueBusy = services.Dialogue != null && services.Dialogue.IsRunning;
services.Input.Unlock();                                   // lepas kunci milik SceneFlow
if (dialogueBusy && !services.Input.IsLocked)
    services.Input.Lock();                                 // kunci milik dialog tetap ada

        transitioning = false;

        // Install() boleh menyalakan HoldFade; fade-in ditahan sampai ReleaseFade().
        if (!HoldFade && fader != null) yield return fader.FadeIn();
    }

    // ---- Battle: Kunci -> fade out -> muat Scn_Battle additive -> Install() ----
    // Dipanggil WildMonster setelah BattleSession.Begin(...)
    public void StartBattle() => runner.StartCoroutine(StartBattleRoutine());

    private IEnumerator StartBattleRoutine()
    {
        services.Input.Lock();
        if (fader != null) yield return fader.FadeOut();

        bool ok = false;
        yield return LoadAndInstall(BattleScene, success => ok = success);
        InBattle = ok;

        if (!ok)
        {
            // Scn_Battle tidak bisa dimuat (cek Scene List!) -> batalkan battle, jangan macet.
            services.Battle.Finish(BattleResult.Fled);
            services.Input.Unlock();
        }
        if (fader != null) yield return fader.FadeIn();
    }

    // ---- Kembali: fade out -> unload -> Finish (aman 2,2 s) -> fade in ----
    // Dipanggil BattleInstaller saat battle selesai.
    public void EndBattle(BattleResult result) => runner.StartCoroutine(EndBattleRoutine(result));

    private IEnumerator EndBattleRoutine(BattleResult result)
    {
        if (fader != null) yield return fader.FadeOut();

        AsyncOperation unload = SceneManager.UnloadSceneAsync(BattleScene);
        if (unload != null) yield return unload;
        InBattle = false;

        Scene world = SceneManager.GetSceneByName(contentScene);
        if (world.IsValid()) SceneManager.SetActiveScene(world);

        if (services.Audio != null) services.Audio.ResumeWorldMusic();   // musik dunia kembali

        services.Battle.Finish(result);   // callback monster + event Ended + mulai waktu aman
        services.Input.Unlock();
        if (fader != null) yield return fader.FadeIn();
    }

    // ---- Util ----
    private IEnumerator LoadAndInstall(string sceneName, System.Action<bool> done = null)
    {
        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (load == null)
        {
            Debug.LogError($"Scene '{sceneName}' gagal dimuat. Sudah ada di Build Settings > Scene List?");
            done?.Invoke(false);
            yield break;
        }
        yield return load;

        Scene scene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(scene);

        bool installed = false;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            SceneInstaller installer = root.GetComponent<SceneInstaller>();
            if (installer == null) continue;
            installer.Install(services);
            installed = true;
            break;
        }
        if (!installed)
            Debug.LogWarning($"Tidak ada SceneInstaller di ROOT scene '{sceneName}'.");

        done?.Invoke(true);
    }
}