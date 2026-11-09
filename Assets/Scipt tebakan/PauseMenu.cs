using System.Collections;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject root;      // PauseRoot
    [SerializeField] private TextMenu menu;

    private static readonly string[] Items = { "Lanjut", "Simpan sekarang", "Kembali ke judul" };

    private GameServices s;
    private bool open;

    public void Init(GameServices services)
    {
        s = services;
        menu.Picked += OnPicked;
        root.SetActive(false);
    }

    private void Update()
    {
        if (s == null || !HotKeys.Pause) return;
        if (open) Close();
        else if (CanOpen()) Open();
    }

    // Buka jika: scene Scn_World, tidak sibuk, tidak battle, gate tidak terkunci.
    private bool CanOpen() =>
        s.Scenes.CurrentScene == SceneFlow.WorldScene
        && !s.Scenes.IsBusy
        && !s.Dialogue.IsRunning
        && !s.Input.IsLocked;

    private void Open()
    {
        s.Input.Lock();
        open = true;
        root.SetActive(true);
        menu.Open(Items);
    }

    private void Close()
    {
        menu.Close();
        root.SetActive(false);
        open = false;
        StartCoroutine(UnlockNextFrame());     // tunda 1 frame agar Enter/E tidak ikut dibaca pemain
    }

    private IEnumerator UnlockNextFrame()
    {
        yield return null;
        s.Input.Unlock();
    }

    private void OnPicked(int index)
    {
        if (!open) return;
        switch (index)
        {
            case 0:                                    // Lanjut
                Close();
                break;
            case 1:                                    // Simpan sekarang
                bool ok = s.Save.Save(SaveMapper.Capture(s, s.Player.position));
                Close();
                s.Toast.Show(ok ? "Tersimpan" : "Gagal menyimpan");
                break;
            case 2:                                    // Kembali ke judul
                Close();
                s.Scenes.GoTo(SceneFlow.TitleScene);
                break;
        }
    }
}
