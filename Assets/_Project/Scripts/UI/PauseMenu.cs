using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseRoot;   // Canvas_Pause (nonaktif saat mulai)
    [SerializeField] private TextMenu menu;
    [SerializeField] private string worldSceneName = "Scn_World";
    [SerializeField] private string battleSceneName = "Scn_Battle";
    [SerializeField] private string titleSceneName = "Scn_Title";

    private GameServices services;
    private bool open;

    public void Construct(GameServices s) { services = s; }

    void Update()
    {
        if (services == null || Keyboard.current == null) return;
        if (!Keyboard.current[Key.Escape].wasPressedThisFrame) return;
        if (open) Close();
        else if (CanOpen()) Open();
    }

    // Hanya buka di dunia, tidak battle, tidak sibuk, gate tidak terkunci
    private bool CanOpen()
    {
        if (!SceneManager.GetSceneByName(worldSceneName).isLoaded) return false;
        if (SceneManager.GetSceneByName(battleSceneName).isLoaded) return false;
        if (services.Input.IsLocked) return false;
        if (services.Dialogue != null && services.Dialogue.IsRunning) return false;
        return true;
    }

    private void Open()
    {
        open = true;
        pauseRoot.SetActive(true);
        menu.Picked -= OnPicked;
        menu.Picked += OnPicked;
        menu.Open(new[] { "Lanjut", "Simpan sekarang", "Kembali ke judul" });
        services.Input.Lock();
    }

    private void Close()
    {
        open = false;
        menu.Picked -= OnPicked;
        pauseRoot.SetActive(false);
        services.Input.Unlock();
    }

    private void OnPicked(int index)
    {
        if (index == 0) Close();
        else if (index == 1) SaveNow();
        else { Close(); services.Scenes.GoTo(titleSceneName); }
    }

    private void SaveNow()
    {
        Vector3 pos = services.Player != null ? services.Player.position : Vector3.zero;
        SaveData data = SaveMapper.Capture(services, pos);
        bool ok = services.Save.Save(data);
        if (services.Toast != null) services.Toast.Show(ok ? "Tersimpan" : "Gagal menyimpan");
    }
}