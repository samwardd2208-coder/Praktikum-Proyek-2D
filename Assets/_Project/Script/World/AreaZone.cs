using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AreaZone : MonoBehaviour
{
    // P5: HUD mendengarkan event ini untuk menampilkan nama area.
    public static event System.Action<string> AreaEntered;

    [SerializeField] private string areaName = "Desa Awal";
    public string AreaName => areaName;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        Debug.Log($"Masuk area: {areaName}");
        AreaEntered?.Invoke(areaName);
    }
}