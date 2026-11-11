using System.Collections;
using UnityEngine;

public class AudioService
{
    private AudioSource musicA;
    private AudioSource musicB;
    private AudioSource sfx;
    private AudioSource currentMusic;   // yang sedang main
    private AudioSource nextMusic;      // yang akan fade-in
    private AudioClip worldMusic;       // BGM dunia terakhir (untuk resume setelah battle)
    private MonoBehaviour runner;       // untuk menjalankan coroutine
    public bool Muted { get; private set; }

    private const float CROSSFADE_DURATION = 1f;

    public AudioService(AudioSource musicA, AudioSource musicB, AudioSource sfx, MonoBehaviour runner)
    {
        this.musicA = musicA;
        this.musicB = musicB;
        this.sfx = sfx;
        this.runner = runner;
        currentMusic = musicA;
        nextMusic = musicB;
        musicA.loop = true;
        musicB.loop = true;
        musicA.volume = 0f;
        musicB.volume = 0f;
    }

    // --- MUSIK DUNIA (dipanggil TitleInstaller, AreaZone) ---
    public void PlayWorldMusic(AudioClip clip)
    {
        worldMusic = clip;
        PlayMusic(clip);
    }

    // --- MUSIK BATTLE (dipanggil BattleInstaller) ---
    public void PlayBattleMusic(AudioClip clip) => PlayMusic(clip);

    // --- RESUME MUSIK DUNIA (dipanggil SceneFlow setelah battle) ---
    public void ResumeWorldMusic() => PlayMusic(worldMusic);

    // --- SFX (dipanggil SaveStone, BattleAudio, DialogueRunner, dll) ---
    public void PlaySfx(AudioClip clip, float pitchJitter = 0.05f)
    {
        if (clip == null) return;
        sfx.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
        sfx.PlayOneShot(clip);
    }

    // --- MUTE (tombol M) ---
    public bool ToggleMute()
    {
        Muted = !Muted;
        AudioListener.volume = Muted ? 0f : 1f;
        return Muted;
    }

    // --- INTERNAL: crossfade antar musik ---
    private void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        if (currentMusic.clip == clip && currentMusic.isPlaying) return; // sudah main

        nextMusic.clip = clip;
        nextMusic.volume = 0f;
        nextMusic.Play();

        // Swap role
        var temp = currentMusic;
        currentMusic = nextMusic;
        nextMusic = temp;

        // Jalankan crossfade
        if (musicA.isPlaying || musicB.isPlaying)
            runner.StartCoroutine(CrossfadeRoutine());
        else
            currentMusic.volume = 1f; // pertama kali, langsung full
    }

    private IEnumerator CrossfadeRoutine()
    {
        float t = 0f;
        while (t < CROSSFADE_DURATION)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / CROSSFADE_DURATION);
            currentMusic.volume = Mathf.Lerp(0f, 1f, p);
            nextMusic.volume = Mathf.Lerp(1f, 0f, p);
            yield return null;
        }
        currentMusic.volume = 1f;
        nextMusic.volume = 0f;
        nextMusic.Stop();
    }
}