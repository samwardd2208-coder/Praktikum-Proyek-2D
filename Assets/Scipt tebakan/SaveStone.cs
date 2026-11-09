using System.Collections;
using UnityEngine;

public class SaveStone : MonoBehaviour, IInteractable
{
    [SerializeField] private AudioClip saveClip;       // SFX_Save

    private GameServices services;

    public void Construct(GameServices s) => services = s;

    public void Interact(PlayerInteractor who)
    {
        if (services != null && !services.Dialogue.IsRunning)
            services.Dialogue.Run(Use());
    }

    private IEnumerator Use()
    {
        if (services.Audio != null) services.Audio.PlaySfx(saveClip, 0f);
        if (services.Party.Partner != null)
        {
            services.Party.Partner.HealFull();       // P7: + kilau PixelBurst
        }
        bool saved = services.Save.Save(SaveMapper.Capture(services, services.Player.position));
        yield return services.Dialogue.Say("", saved
            ? "Batu Simpan bersinar. HP partner pulih dan perjalanan tersimpan."
            : "Batu Simpan bersinar. HP partner pulih, tetapi penyimpanan gagal.");
    }
}