using System.Collections;
using UnityEngine;

public class AudioService : MonoBehaviour
{
    [SerializeField] private AudioSource musicA, musicB, sfx;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.6f;
    [SerializeField] private float crossfadeSeconds = 1f;
    [Header("UI")]
[SerializeField] private AudioClip uiMove, uiConfirm, uiCancel;

public void PlayUiMove() => PlaySfx(uiMove, 0.02f);
public void PlayUiConfirm() => PlaySfx(uiConfirm, 0f);
public void PlayUiCancel() => PlaySfx(uiCancel, 0f);

    private AudioSource active, idle;
    private AudioClip worldMusic, current;
    private Coroutine fadeRoutine;

    public bool Muted { get; private set; }

    private void Awake()
    {
        active = musicA;
        idle = musicB;
        foreach (var s in new[] { musicA, musicB })
        {
            s.loop = true;
            s.playOnAwake = false;
            s.volume = 0f;
        }
        sfx.playOnAwake = false;
    }

    public void PlayWorldMusic(AudioClip clip) { worldMusic = clip; PlayMusic(clip); }
    public void PlayBattleMusic(AudioClip clip) => PlayMusic(clip);     // dipanggil BattleInstaller
    public void ResumeWorldMusic() => PlayMusic(worldMusic);            // dipanggil SceneFlow setelah battle

    private void PlayMusic(AudioClip clip)
    {
        if (clip == null || clip == current) return;
        current = clip;

        AudioSource from = active, to = idle;
        to.clip = clip;
        to.volume = 0f;
        to.Play();
        active = to;
        idle = from;

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(Crossfade(from, to));
    }

    private IEnumerator Crossfade(AudioSource from, AudioSource to)
    {
        float startFrom = from.volume, t = 0f;
        while (t < crossfadeSeconds)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / crossfadeSeconds);
            from.volume = Mathf.Lerp(startFrom, 0f, k);
            to.volume = Mathf.Lerp(0f, musicVolume, k);
            yield return null;
        }
        from.Stop();
        from.volume = 0f;
        to.volume = musicVolume;
    }

    public void PlaySfx(AudioClip clip, float pitchJitter = 0.05f)
    {
        if (clip == null) return;
        sfx.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
        sfx.PlayOneShot(clip);
    }

    public bool ToggleMute()   // tombol M
    {
        Muted = !Muted;
        AudioListener.volume = Muted ? 0f : 1f;
        return Muted;
    }
}