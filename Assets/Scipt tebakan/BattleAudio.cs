using UnityEngine;
 
public class BattleAudio : MonoBehaviour
{
    [SerializeField] private AudioClip hit, hitCritical, miss, levelUp, evolve, victory, faint;
 
    private AudioService audioService;
 
    public void Init(GameServices s) { audioService = s.Audio; }
 
    public void Hit(bool critical) => Play(critical ? hitCritical : hit);
    public void Miss() => Play(miss);
    public void LevelUp() => Play(levelUp);
    public void Evolve() => Play(evolve);
    public void Victory() => Play(victory);
    public void Faint() => Play(faint);
 
    private void Play(AudioClip clip)
    {
        if (audioService != null) audioService.PlaySfx(clip);
    }
}