using UnityEngine;

public class BattleAudio : MonoBehaviour
{
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip hitCriticalClip;
    [SerializeField] private AudioClip missClip;
    [SerializeField] private AudioClip levelUpClip;
    [SerializeField] private AudioClip evolveClip;
    [SerializeField] private AudioClip victoryJingleClip;
    [SerializeField] private AudioClip faintClip;

    private AudioService audio;

    public void Construct(AudioService audio) { this.audio = audio; }

    public void PlayHit(bool critical) =>
        audio?.PlaySfx(critical ? hitCriticalClip : hitClip);

    public void PlayMiss() => audio?.PlaySfx(missClip);
    public void PlayLevelUp() => audio?.PlaySfx(levelUpClip);
    public void PlayEvolve() => audio?.PlaySfx(evolveClip);
    public void PlayVictory() => audio?.PlaySfx(victoryJingleClip);
    public void PlayFaint() => audio?.PlaySfx(faintClip);
}