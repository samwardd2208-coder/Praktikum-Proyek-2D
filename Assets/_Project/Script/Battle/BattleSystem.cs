using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Logika battle bergiliran (tanpa tampilan). Tampilan mendengarkan event lewat BattlePresenter (pola MVP).
///
/// ADAPTASI P4 (asumsi nama anggota dari P4; sesuaikan kalau berbeda):
///   BattleSession : IsBoss, EnemyData (MonsterData), EnemyLevel (int)
///   MonsterInstance: new MonsterInstance(MonsterData, int level), Data, Level, Attack, Defense, Speed,
///                    CurrentHP, MaxHP, TakeDamage(int), Heal(int) -> int, GainExp(int) -> IEnumerable&lt;GrowthEvent&gt;
///   SceneFlow     : EndBattle(BattleResult)
///   BattleResult  : None, Win, Lose, Fled
/// </summary>
public class BattleSystem : MonoBehaviour
{
    [SerializeField] float secondsPerChar = 0.018f;   // 55 huruf per detik
    [SerializeField] float evolveSeconds = 2.4f;

    // ---- Event (pendengar: lihat tabel "Event BattleSystem dan pendengarnya") ----
    public event System.Action Started;
    public event System.Action<string> MessageShown;
    public event System.Action ChoiceRequested;
    public event System.Action<MonsterInstance> Attacked;
    public event System.Action<MonsterInstance, int, bool, Element?> Hit;
    public event System.Action Missed;
    public event System.Action<MonsterInstance> Fainted;
    public event System.Action Victory;
    public event System.Action<MonsterData> LevelUp;                    // kirim data LAMA (From), bukan yang baru
    public event System.Action<MonsterData, MonsterData> Evolving;
    public event System.Action<MonsterInstance> Evolved;
    public event System.Action<BattleResult> Ended;


    // ---- State ----
    public MonsterInstance Player { get; private set; }
    public MonsterInstance Enemy { get; private set; }
    public bool IsBoss { get; private set; }
    public float EvolveSeconds => evolveSeconds;
    public IReadOnlyList<BattleAction> PlayerActions => playerActions;
    public MonsterData EnemyData; // Sesuaikan "MonsterBase" jika tipe datamu memiliki nama lain

public int EnemyLevel;






    PlayerParty party;
    SceneFlow scenes;
    BattleResult result = BattleResult.None;
    BattleAction chosen;

    int playerSkills = 3;     // "Jurus (3)"
    int enemySkills = 2;      // musuh: 35 % Jurus, maks 2 kali

    readonly AttackAction attackAction = new AttackAction();
    readonly SkillAction skillAction = new SkillAction();
    BattleAction[] playerActions;       // urutan = urutan menu: Serang, Jurus, Ramuan, Kabur

    // ------------------------------------------------------------------ mulai

    public void Construct(PlayerParty party, BattleSession session, SceneFlow scenes)
    {
        this.party = party;
        this.scenes = scenes;
        Player = party.Partner;
        IsBoss = session.IsBoss;
                Enemy = session.Enemy;   // lawan sudah dibuat WildMonster di BattleSession // ADAPTASI P4
        playerSkills = 3;
        enemySkills = 2;
        playerActions = new BattleAction[] { attackAction, skillAction, new PotionAction(), new RunAction() };
    }

    public void Begin() => StartCoroutine(Run());

    /// <summary>Dipanggil View/Presenter saat pemain memilih aksi.</summary>
    public void Choose(BattleAction action) => chosen = action;

    // ------------------------------------------------------------------ pesan

    // Pesan tampil, lalu lanjut setelah (hold + panjang teks x 18 ms).
    // E / Spasi / Enter mempercepat setelah teks selesai diketik.
    // Logika tidak menunggu klik: battle tetap jalan walau pemain diam.
    public IEnumerator Say(string text, float hold = 0.8f)
    {
        MessageShown?.Invoke(text);                                   // View mengetik 55 huruf/detik
        float wait = hold + text.Length * secondsPerChar;
        for (float t = 0f; t < wait; t += Time.deltaTime)
        {
            if (t > text.Length * secondsPerChar && MenuInput.Confirm) break;
            yield return null;
        }
    }

    // ------------------------------------------------------------------ alur utama

    IEnumerator Run()
    {
        Started?.Invoke();
        yield return new WaitForSeconds(0.5f);                        // tirai pembuka
        yield return Say(IsBoss ? $"{Enemy.Data.DisplayName}, penguasa Gua Petir, muncul!"
                                : $"{Enemy.Data.DisplayName} liar Lv{Enemy.Level} muncul!", 0.9f);
        yield return Say($"Maju, {Player.Data.DisplayName}!", 0.7f);

        while (result == BattleResult.None)
        {
            chosen = null;
            ChoiceRequested?.Invoke();
            yield return new WaitUntil(() => chosen != null);

            if (chosen is RunAction && IsBoss)                        // boss: giliran tidak hilang
            { yield return chosen.Execute(this, Player, Enemy); continue; }

            BattleAction enemyAction = PickEnemyAction();
            bool playerFirst = chosen.Priority > 0 || Player.Speed > Enemy.Speed
                               || (Player.Speed == Enemy.Speed && UnityEngine.Random.value < 0.5f);
            if (playerFirst) { yield return Turn(chosen, Player, Enemy); yield return Turn(enemyAction, Enemy, Player); }
            else             { yield return Turn(enemyAction, Enemy, Player); yield return Turn(chosen, Player, Enemy); }
        }

        Ended?.Invoke(result);
        yield return new WaitForSeconds(0.3f);                        // jeda sebelum fade
        scenes.EndBattle(result);
    }

    IEnumerator Turn(BattleAction action, MonsterInstance user, MonsterInstance target)
    {
        if (result != BattleResult.None) yield break;                 // sudah menang/kalah/kabur
        if (user.CurrentHP <= 0) yield break;

        yield return action.Execute(this, user, target);

        if (result != BattleResult.None) yield break;                 // misalnya Kabur berhasil
        if (Enemy.CurrentHP <= 0) yield return Win();
        else if (Player.CurrentHP <= 0) yield return Lose();
    }

    BattleAction PickEnemyAction()
    {
        if (enemySkills > 0 && UnityEngine.Random.value < 0.35f) return skillAction;
        return attackAction;
    }

    // ------------------------------------------------------------------ menang & evolusi

    IEnumerator Win()
    {
        result = BattleResult.Win;
        Fainted?.Invoke(Enemy);                                       // lawan tenggelam & memudar
        yield return new WaitForSeconds(0.6f);
        yield return Say(IsBoss ? $"{Enemy.Data.DisplayName} kalah!" : $"{Enemy.Data.DisplayName} liar kalah!", 0.8f);
        Victory?.Invoke();                                            // jingle menang

        int exp = (10 + Enemy.Level * 8) * (IsBoss ? 2 : 1);
        yield return Say($"{Player.Data.DisplayName} mendapat {exp} EXP.", 0.9f);

        foreach (GrowthEvent growth in Player.GainExp(exp))
        {
            if (growth.Type == GrowthEventType.LevelUp)
            {
                LevelUp?.Invoke(growth.From);                         // panel tetap pakai nama/sprite lama
                yield return Say(growth.Message, 0.9f);
                continue;
            }

            yield return Say($"Apa? {growth.From.DisplayName} mulai bersinar...", 0.9f);
            Evolving?.Invoke(growth.From, growth.To);                 // animasi 2,4 detik
            yield return new WaitForSeconds(evolveSeconds);
            Evolved?.Invoke(Player);                                  // kilat putih + sprite baru
            yield return Say(growth.Message, 1.2f);                   // "Pyrit berevolusi menjadi Pyrago!"
        }

        if (!IsBoss)
        {
            int healed = Mathf.RoundToInt(Player.MaxHP * 0.25f);
Player.Heal(healed);
            if (healed > 0)
                yield return Say($"{Player.Data.DisplayName} beristirahat sejenak dan pulih {healed} HP.", 0.8f);
        }
    }

    // ------------------------------------------------------------------ kalah

    IEnumerator Lose()
    {
        result = BattleResult.Lose;
        Fainted?.Invoke(Player);
        yield return new WaitForSeconds(0.6f);
        yield return Say($"{Player.Data.DisplayName} pingsan...", 0.9f);
        yield return Say("Kamu berlari kembali ke desa..", 1.0f);
        party.NeedsRespawn = true;      // WorldInstaller: pulihkan HP + pindah ke depan Batu Simpan
    }

    // ------------------------------------------------------------------ dipakai BattleAction

    public float DamageScale(MonsterInstance user) => user == Enemy && !IsBoss ? 0.8f : 1f;

    public int SkillsLeft(MonsterInstance user) => user == Player ? playerSkills : enemySkills;

    public void ConsumeSkill(MonsterInstance user)
    {
        if (user == Player) playerSkills = Mathf.Max(0, playerSkills - 1);
        else enemySkills = Mathf.Max(0, enemySkills - 1);
    }

    public int PotionsLeft => party.Potions;
    public bool UsePotion() => party.UsePotion();
    public void AddPotions(int amount) => party.AddPotions(amount);     // dipakai BattleDebugInput

    public void Flee() => result = BattleResult.Fled;

    public void RaiseAttacked(MonsterInstance m) => Attacked?.Invoke(m);
    public void RaiseMissed() => Missed?.Invoke();
    public void RaiseHit(MonsterInstance target, int damage, bool critical, Element? element)
        => Hit?.Invoke(target, damage, critical, element);
}
