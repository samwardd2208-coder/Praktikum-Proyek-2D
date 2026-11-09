// Wadah semua service yang dibagikan antar-scene. Dibuat SATU kali di GameBootstrap.
using UnityEngine;
public class GameServices
{
    public InputGate Input { get; set; }          // input atau nol saat Lock()
    public MonsterDatabase Database { get; set; }
    public PlayerParty Party { get; set; }        // partner + 3 Ramuan
    public BattleSession Battle { get; set; }     // musuh + callback hasil
    public SceneFlow Scenes { get; set; }   
    public StoryFlags Flags;
    public ToastView Toast;
public DialogueRunner Dialogue;
public SaveService Save;
public AudioService Audio;
public Transform Player;     // diisi WorldInstaller
public float Playtime;      // fade, muat, Install()
}
