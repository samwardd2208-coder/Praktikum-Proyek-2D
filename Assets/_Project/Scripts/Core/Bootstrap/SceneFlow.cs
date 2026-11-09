using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
 
// Mengatur perpindahan scene: muat dunia, masuk battle, kembali ke dunia.
public class SceneFlow
{
    public const string BattleScene = "Scn_Battle";
 
    private readonly MonoBehaviour runner;
    private readonly ScreenFader fader;
    private readonly GameServices services;
 
    private Scene worldScene;
    private readonly List<GameObject> hiddenRoots = new List<GameObject>();
 
    public SceneFlow(MonoBehaviour runner, ScreenFader fader, GameServices services)
    {
        this.runner = runner;
        this.fader = fader;
        this.services = services;
    }
 
    // Memuat scene dunia pertama kali.
    public void GoTo(string sceneName) => runner.StartCoroutine(GoToRoutine(sceneName));
 
    // Dipanggil monster liar saat menyentuh Player.
    public void StartBattle() => runner.StartCoroutine(StartBattleRoutine());
 
    // Dipanggil battle saat selesai.
    public void EndBattle(BattleResult result) => runner.StartCoroutine(EndBattleRoutine(result));
 
    private IEnumerator GoToRoutine(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.isLoaded)
        {
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            scene = SceneManager.GetSceneByName(sceneName);
        }
 
        SceneManager.SetActiveScene(scene);
        worldScene = scene;
        InstallScene(scene);
        yield return FadeIn();
    }
 
    private IEnumerator StartBattleRoutine()
    {
        services.Input.Lock();                       // Player diam
        yield return FadeOut();
 
        SetWorldActive(false);                       // dunia "dibekukan", posisi tetap
        yield return SceneManager.LoadSceneAsync(BattleScene, LoadSceneMode.Additive);
        Scene battle = SceneManager.GetSceneByName(BattleScene);
        SceneManager.SetActiveScene(battle);
        InstallScene(battle);
        yield return FadeIn();
    }
 
    private IEnumerator EndBattleRoutine(BattleResult result)
    {
        yield return FadeOut();
        yield return SceneManager.UnloadSceneAsync(BattleScene);
 
        SetWorldActive(true);
        SceneManager.SetActiveScene(worldScene);
        services.Input.Unlock();
        services.Battle.Finish(result);
        yield return FadeIn();
    }
 
    // Mencari installer di root scene, lalu memanggil Install.
    private void InstallScene(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            SceneInstaller installer = root.GetComponent<SceneInstaller>();
            if (installer != null)
            {
                installer.Install(services);
                return;
            }
        }
        Debug.LogWarning($"Scene {scene.name} tidak punya SceneInstaller di root.");
    }
 
    private void SetWorldActive(bool active)
    {
        if (!worldScene.IsValid()) return;
 
        if (!active)
        {
            hiddenRoots.Clear();
            foreach (GameObject root in worldScene.GetRootGameObjects())
            {
                if (!root.activeSelf) continue;
                hiddenRoots.Add(root);
                root.SetActive(false);
            }
        }
        else
        {
            foreach (GameObject root in hiddenRoots)
                if (root != null) root.SetActive(true);
            hiddenRoots.Clear();
        }
    }
 
    private IEnumerator FadeIn()
    {
        if (fader != null) yield return fader.FadeIn();
    }
 
    private IEnumerator FadeOut()
    {
        if (fader != null) yield return fader.FadeOut();
    }
}
 