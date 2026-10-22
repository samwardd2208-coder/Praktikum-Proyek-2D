using UnityEngine;
[RequireComponent(typeof(Collider2D))]
public class AreaZone : MonoBehaviour
{
    [SerializeField] private string areaName = "Desa Awal";
    public string AreaName => areaName;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        Debug.Log($"Masuk area: {areaName}");
    }
}