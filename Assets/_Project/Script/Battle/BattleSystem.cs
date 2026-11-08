using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Logika battle bergiliran (tanpa tampilan). Tampilan mendengarkan event lewat BattlePresenter (pola MVP).
/// </summary>
public class BattleSystem : MonoBehaviour
{
    [SerializeField] float secondsPerChar = 0.018f;   // 55 huruf per detik
    [SerializeField] float evolveSeconds = 2.4f;

    // ---- Event ----
    public event System.Action Started;
    public event System.Action<string> MessageShown;
    public event System.Action ChoiceRequested;
    public event System.Action<MonsterInstance> Attacked;
    public event System.Action<MonsterInstance, int, bool, Element?> Hit;
    public event System.Action Missed;
    public event System.Action<MonsterInstance> Fainted;
    public event System.Action Victory;
    public event System.Action<MonsterData> LevelUp;
    public event System.Action<MonsterData, MonsterData> Evolving;
    public event System.Action<MonsterInstance> Evolved;
    public event System.Action<BattleResult> Ended;

    // ---- State ----
    public MonsterInstance Player { get; private set; }
    public MonsterInstance Enemy { get; private set; }
    public bool IsBoss { get; private set; }
    public float EvolveSeconds => evolveSeconds;
    public IReadOnlyList<BattleAction> PlayerActions => playerActions;

    PlayerParty party;
    SceneFlow scenes;
    BattleResult result = BattleResult.None;
    BattleAction chosen;

    int playerSkills = 3;
    int enemySkills = 2;

    readonly AttackAction attackAction = new AttackAction();
    readonly SkillAction skillAction = new SkillAction();
    BattleAction[] playerActions;

    // ------------------------------------------------------------------ mulai

    public void Construct(PlayerParty party, BattleSession session, SceneFlow scenes)
    {
        this.party = party;
        this.scenes = scenes;
        
        Player = party.Partner;
        if (Player == null)
        {
            Debug.LogError("[BattleSystem] ERROR: Player Partner adalah NULL! Cek PlayerParty.");
        }

        IsBoss = session.IsBoss;
        Enemy = session.Enemy;
        
        // PENGAMAN: Cek apakah Enemy benar-benar ada
        if (Enemy == null)
        {
            Debug.LogError("[BattleSystem] ERROR: Enemy adalah NULL! Cek script yang memanggil Construct(), pastikan session.Enemy sudah di-assign dengan MonsterInstance yang valid.");
        }

        playerSkills = 3;
        enemySkills = 2;
        playerActions = new BattleAction[] { attackAction, skillAction, new PotionAction(), new RunAction() };
    }

    public void Begin() => StartCoroutine(Run());

    public void Choose(BattleAction action) => chosen = action;

    // ------------------------------------------------------------------ pesan

    public IEnumerator Say(string text, float hold = 0.8f)
    {
        MessageShown?.Invoke(text);
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
        yield return new WaitForSeconds(0.5f);
        
        // PENGAMAN: Jika Enemy null, hentikan agar tidak crash
        if (Enemy == null || Enemy.Data == null)
        {
            Debug.LogError("[BattleSystem] Run() dibatalkan karena data Enemy null.");
            yield break;
        }

        yield return Say(IsBoss ? $"{Enemy.Data.DisplayName}, penguasa Gua Petir, muncul!"
                                : $"{Enemy.Data.DisplayName} liar Lv{Enemy.Level} muncul!", 0.9f);
        yield return Say($"Maju, {Player.Data.DisplayName}!", 0.7f);

        while (result == BattleResult.None)
        {
            chosen = null;
            ChoiceRequested?.Invoke();
            yield return new WaitUntil(() => chosen != null);

            if (chosen is RunAction && IsBoss)
            { yield return chosen.Execute(this, Player, Enemy); continue; }

            BattleAction enemyAction = PickEnemyAction();
            bool playerFirst = chosen.Priority > 0 || Player.Speed > Enemy.Speed
                               || (Player.Speed == Enemy.Speed && UnityEngine.Random.value < 0.5f);
                               
            if (playerFirst) { yield return Turn(chosen, Player, Enemy); yield return Turn(enemyAction, Enemy, Player); }
            else             { yield return Turn(enemyAction, Enemy, Player); yield return Turn(chosen, Player, Enemy); }
        }

        Ended?.Invoke(result);
        yield return new WaitForSeconds(0.3f);
        scenes.EndBattle(result);
    }

    IEnumerator Turn(BattleAction action, MonsterInstance user, MonsterInstance target)
    {
        if (result != BattleResult.None) yield break;
        if (user.CurrentHP <= 0) yield break;

        yield return action.Execute(this, user, target);

        if (result != BattleResult.None) yield break;
        if (Enemy.CurrentHP <= 0) yield return Win();
        else if (Player.CurrentHP <= 0) yield return Lose();
    }

    BattleAction PickEnemyAction()
    {
        if (enemySkills > 0 && UnityEngine.Random.value < 0.35f) return skillAction;
        return attackAction;
    }

    // ------------------------------------------------------------------ menang & evolusi

       // ------------------------------------------------------------------ menang & evolusi

    IEnumerator Win()
    {
        result = BattleResult.Win;
        
        if (Enemy != null) Fainted?.Invoke(Enemy);
        yield return new WaitForSeconds(0.6f);
        
        string enemyName = (Enemy != null && Enemy.Data != null) ? Enemy.Data.DisplayName : "Musuh";
        yield return Say(IsBoss ? $"{enemyName} kalah!" : $"{enemyName} liar kalah!", 0.8f);
        Victory?.Invoke();

        int exp = (Enemy != null) ? (10 + Enemy.Level * 8) * (IsBoss ? 2 : 1) : 10;
        
        string playerName = (Player != null && Player.Data != null) ? Player.Data.DisplayName : "Monster";
        yield return Say($"{playerName} mendapat {exp} EXP.", 0.9f);

        if (Player != null)
        {
            var growthEvents = Player.GainExp(exp);
            
            // PENGAMAN: Jika GainExp mengembalikan null, jangan lakukan foreach (ini penyebab umum error baris 168)
            if (growthEvents != null)
            {
                foreach (GrowthEvent growth in growthEvents)
                {
                    if (growth == null) continue;

                    if (growth.Type == GrowthEventType.LevelUp)
                    {
                        if (growth.From != null) LevelUp?.Invoke(growth.From);
                        yield return Say(growth.Message, 0.9f);
                        continue;
                    }

                    if (growth.From != null)
                    {
                        yield return Say($"Apa? {growth.From.DisplayName} mulai bersinar...", 0.9f);
                        Evolving?.Invoke(growth.From, growth.To);
                    }
                    yield return new WaitForSeconds(evolveSeconds);
                    Evolved?.Invoke(Player);
                    if (growth.From != null)
                    {
                        yield return Say(growth.Message, 1.2f);
                    }
                }
            }
            else
            {
                Debug.LogWarning("[BattleSystem] Player.GainExp() mengembalikan NULL! Seharusnya mengembalikan list kosong.");
            }
        }

        if (!IsBoss && Player != null)
        {
            int healed = Mathf.RoundToInt(Player.MaxHP * 0.25f);
            Player.Heal(healed);
            if (healed > 0 && Player.Data != null)
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
        party.NeedsRespawn = true;
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
    public void AddPotions(int amount) => party.AddPotions(amount);

    public void Flee() => result = BattleResult.Fled;

    public void RaiseAttacked(MonsterInstance m) => Attacked?.Invoke(m);
    public void RaiseMissed() => Missed?.Invoke();
    public void RaiseHit(MonsterInstance target, int damage, bool critical, Element? element)
        => Hit?.Invoke(target, damage, critical, element);
}