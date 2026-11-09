
using System.Collections.Generic;
using UnityEngine;

public class TitleInstaller : SceneInstaller
{
    [SerializeField] private TextMenu menu;
    [SerializeField] private AudioClip titleMusic;       // BGM_Title

    private readonly List<string> options = new List<string>();
    private GameServices services;
    private bool leaving;

    public override void Install(GameServices s)
    {
        services = s;
        options.Clear();
        if (s.Save.HasSave()) options.Add("Lanjutkan");
        options.Add("Mulai baru");
        options.Add("Keluar");
        menu.Picked += OnPicked;
        menu.Open(options.ToArray());
        s.Audio.PlayWorldMusic(titleMusic);
    }

    private void OnDestroy() { if (menu != null) menu.Picked -= OnPicked; }

    private void OnPicked(int index)
    {
        if (leaving) return;
        switch (options[index])
        {
            case "Lanjutkan": Continue(); break;
            case "Mulai baru": NewGame(); break;
            case "Keluar": Quit(); break;
        }
    }

    private void Continue()
    {
        SaveData data = services.Save.Load();
        if (data == null) { NewGame(); return; }
        leaving = true;
        SaveMapper.Apply(data, services);
        services.Save.PendingLoad = data;                 // posisi dipakai World
        services.Scenes.GoTo(SceneFlow.WorldScene);
    }

    private void NewGame()
    {
        leaving = true;
        services.Save.PendingLoad = null;
        services.Party.Reset();                           // Ramuan 3, tanpa partner
        services.Flags.Load(new List<string>());          // narasi diputar
        services.Scenes.GoTo(SceneFlow.WorldScene);
    }

    private void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
