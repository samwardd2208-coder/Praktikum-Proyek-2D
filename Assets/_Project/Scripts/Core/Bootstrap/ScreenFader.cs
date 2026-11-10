using System.Collections;
using UnityEngine;

// Pasang pada objek yang punya CanvasGroup dan Image hitam layar penuh di Scn_Bootstrap
// (Canvas fader: Sort Order 100). Layar dimulai HITAM, jadi dunia tidak sempat terlihat
// sebelum narasi pembuka.
[RequireComponent(typeof(CanvasGroup))]
public class ScreenFader : MonoBehaviour
{
    [SerializeField] private float duration = 0.5f;
    private CanvasGroup group;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();

        // Mulai dengan layar hitam pekat
        group.alpha = 1f;
        group.blocksRaycasts = true;

        // DontDestroyOnLoad dihapus: Scn_Bootstrap tidak pernah di-unload,
        // dan perintah itu hanya valid untuk objek root (memunculkan peringatan bila bukan).
    }

    // Memudarkan ke hitam (alpha = 1)
    public IEnumerator FadeOut() => Fade(1f);

    // Memudarkan ke terang (alpha = 0)
    public IEnumerator FadeIn() => Fade(0f);

    private IEnumerator Fade(float target)
    {
        if (group == null) group = GetComponent<CanvasGroup>();

        group.blocksRaycasts = true;                 // selama transisi, layar menangkap klik
        float start = group.alpha;

        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            group.alpha = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }

        group.alpha = target;
        group.blocksRaycasts = target > 0.5f;        // hanya memblokir klik saat layar hitam
    }
}