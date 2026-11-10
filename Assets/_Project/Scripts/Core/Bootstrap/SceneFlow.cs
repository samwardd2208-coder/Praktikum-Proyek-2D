using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Fade, muat scene, panggil Install(). Bukan MonoBehaviour; coroutine dijalankan lewat "runner".
public class SceneFlow
{
    private const string BattleScene = "Scn_Battle";

    private readonly MonoBehaviour runner;
    private readonly ScreenFader fader;
    private readonly GameServices services;
    private Scene worldScene;
    private bool battleActive;

    public SceneFlow(MonoBehaviour runner, ScreenFader fader, GameServices services)
    {
        this.runner = runner;
        this.fader = fader;
        this.services = services;
    }

    // Pindah ke scene dunia (unload dunia lama kalau ada).
    public void GoTo(string sceneName) => runner.StartCoroutine(GoToRoutine(sceneName));

    // Mulai battle: simpan musuh di BattleSession, lalu muat Scn_Battle.
    public void StartBattle(MonsterInstance enemy, bool isBoss, Action<BattleResult> onFinished)
    {
        if (battleActive) return;          // cegah dobel
        battleActive = true;
        services.Battle.Begin(enemy, isBoss, onFinished);
        runner.StartCoroutine(BattleRoutine());
    }

    // Dipanggil BattleInstaller (atau BattleSystem di P5) saat battle selesai.
    public void EndBattle(BattleResult result)
    {
        // Jika kalah, pindahkan player ke Batu Simpan
        if (result == BattleResult.Lose)
        {
            GameObject batuSimpan = GameObject.Find("Batu Simpan");
            if (batuSimpan != null)
            {
                PlayerPrefs.SetFloat("LoseSpawnX", batuSimpan.transform.position.x);
                PlayerPrefs.SetFloat("LoseSpawnY", batuSimpan.transform.position.y);
                PlayerPrefs.SetInt("LostBattle", 1);
                PlayerPrefs.Save();
            }
        }
        
        // GANTI DENGAN INI:
        runner.StartCoroutine(ReturnRoutine(result));
    }
    
    private IEnumerator GoToRoutine(string sceneName)
    {
        services.Input.Lock();
        yield return fader.FadeOut();

        if (worldScene.IsValid() && worldScene.isLoaded)
            yield return SceneManager.UnloadSceneAsync(worldScene);

        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        worldScene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(worldScene);
        InstallScene(worldScene);

        yield return fader.FadeIn();
        services.Input.Unlock();
    }

    private IEnumerator BattleRoutine()
    {
        services.Input.Lock();
        yield return fader.FadeOut();

        // Dunia disembunyikan (tidak di-unload) supaya posisi pemain & monster tetap.
        SetWorldActive(false);

        yield return SceneManager.LoadSceneAsync(BattleScene, LoadSceneMode.Additive);
        Scene battle = SceneManager.GetSceneByName(BattleScene);
        SceneManager.SetActiveScene(battle);
        InstallScene(battle);

        yield return fader.FadeIn();
    }

    private IEnumerator ReturnRoutine(BattleResult result)
    {
        yield return fader.FadeOut();

        Scene battle = SceneManager.GetSceneByName(BattleScene);
        if (battle.IsValid() && battle.isLoaded)
            yield return SceneManager.UnloadSceneAsync(battle);

        SetWorldActive(true);
        SceneManager.SetActiveScene(worldScene);

        // Finish() dipanggil setelah dunia aktif lagi: callback monster + event Ended
        // (kalah -> WorldInstaller memindah pemain).
        services.Battle.Finish(result);

        yield return fader.FadeIn();
        services.Input.Unlock();
        battleActive = false; 
    }

    private void SetWorldActive(bool active)
    {
        foreach (GameObject root in worldScene.GetRootGameObjects())
            root.SetActive(active);
    }

    private void InstallScene(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            SceneInstaller installer = root.GetComponent<SceneInstaller>();
            if (installer == null) continue;
            try { installer.Install(services); }
            catch (System.Exception e) { Debug.LogException(e); }
            return;
        }
        Debug.LogWarning($"SceneFlow: tidak ada SceneInstaller di root scene {scene.name}");
    }
}
