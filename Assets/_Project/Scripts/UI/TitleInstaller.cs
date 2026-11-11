using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TitleInstaller : SceneInstaller
{
    [SerializeField] private TextMenu menu;
    [SerializeField] private string worldSceneName = "Scn_World";
    [SerializeField] private AudioSource musicSource;  // opsional
    [SerializeField] private AudioClip titleMusic;     // opsional

    private GameServices services;
    private readonly List<string> options = new List<string>();
    private bool hasSave;

    public override void Install(GameServices s)
    {
        services = s;
        hasSave = s.Save != null && s.Save.HasSave();

        options.Clear();
        if (hasSave) options.Add("Lanjutkan");
        options.Add("Mulai baru");
        options.Add("Keluar");

        menu.Picked -= OnPicked;
        menu.Picked += OnPicked;
        menu.Open(options.ToArray());

        if (s.Audio != null && titleMusic != null)
            s.Audio.PlayWorldMusic(titleMusic);
    }

    private void OnPicked(int index)
    {
        if (hasSave)
        {
            if (index == 0) Continue();
            else if (index == 1) NewGame();
            else Quit();
        }
        else
        {
            if (index == 0) NewGame();
            else Quit();
        }
    }

    private void Continue()
    {
        SaveData data = services.Save.Load();
        if (data == null) { NewGame(); return; }
        SaveMapper.Apply(data, services);
        services.Save.PendingLoad = data;   // posisi & partner dipakai WorldInstaller
        services.Scenes.GoTo(worldSceneName);
    }

    private void NewGame()
    {
        services.Party.Reset();                      // Ramuan 3, tanpa partner
        services.Flags.Load(new List<string>());     // narasi diputar lagi
        services.Save.PendingLoad = null;            // jangan load save lama
        services.Scenes.GoTo(worldSceneName);
    }

    private void Quit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}