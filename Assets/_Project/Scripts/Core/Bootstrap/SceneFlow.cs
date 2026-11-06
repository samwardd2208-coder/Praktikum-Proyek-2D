using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFlow
{
    private readonly GameBootstrap bootstrap;
    private readonly ScreenFader fader;
    private readonly GameServices services;
    private string currentAdditiveScene;

    public SceneFlow(GameBootstrap bootstrap, ScreenFader fader, GameServices services)
    {
        this.bootstrap = bootstrap;
        this.fader = fader;
        this.services = services;
    }

    public IEnumerator GoTo(string sceneName)
    {
        if (fader != null) yield return bootstrap.StartCoroutine(fader.FadeOut());

        // Unload scene additive sebelumnya jika ada
        if (!string.IsNullOrEmpty(currentAdditiveScene))
        {
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(currentAdditiveScene);
            if (unloadOp != null)
            {
                while (!unloadOp.isDone) yield return null;
            }
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (op == null) yield break;

        while (!op.isDone) yield return null;

        Scene newScene = SceneManager.GetSceneByName(sceneName);
        if (newScene.IsValid())
        {
            SceneManager.SetActiveScene(newScene);
            foreach (var rootObj in newScene.GetRootGameObjects())
            {
                SceneInstaller installer = rootObj.GetComponent<SceneInstaller>();
                if (installer != null)
                {
                    installer.Install(services);
                    break;
                }
            }
        }
        currentAdditiveScene = sceneName;

        if (fader != null) yield return bootstrap.StartCoroutine(fader.FadeIn());
    }
}