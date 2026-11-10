using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ToastView : MonoBehaviour
{
    [SerializeField] private GameObject panel;   // Panel background toast
    [SerializeField] private Text textLabel;     // Teks pesan
    [SerializeField] private float duration = 3f; // Durasi muncul (detik)

    private Coroutine hideCoroutine;

    public void Show(string message)
    {
        if (panel == null || textLabel == null) return;

        panel.SetActive(true);
        textLabel.text = message;

        // Reset timer jika toast dipanggil berturut-turut
        if (hideCoroutine != null) StopCoroutine(hideCoroutine);
        hideCoroutine = StartCoroutine(HideAfter(duration));
    }

    private IEnumerator HideAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        panel.SetActive(false);
    }
}