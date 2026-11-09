using UnityEngine;
 
[RequireComponent(typeof(Collider2D))]
public class AreaZone : MonoBehaviour
{
    // P5: HUD mendengarkan event ini untuk menampilkan nama area.
    public static event System.Action<string> AreaEntered;
 
    // P6: diisi GameBootstrap (AreaZone.Audio = audioService;)
    public static AudioService Audio;
 
    [SerializeField] private string areaName = "Desa Awal";
    [SerializeField] private AudioClip music;     // BGM_Village / BGM_Forest
 
    public string AreaName => areaName;
 
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        Debug.Log($"Masuk area: {areaName}");
        AreaEntered?.Invoke(areaName);
 
        // AudioService mengabaikan clip yang sama dengan yang sedang diputar,
        // jadi keluar-masuk area yang sama tidak mengulang lagu.
        if (music != null && Audio != null) Audio.PlayWorldMusic(music);
    }
}