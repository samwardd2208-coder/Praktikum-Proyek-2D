using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// PENTING: Jangan tulis 'public enum BattleResult' di sini! 
// Karena sudah ada di BattleSession.cs.

public class BattleSystem
{
    public MonsterInstance Player { get; private set; }
    public MonsterInstance Enemy { get; private set; }
    public bool IsBoss { get; private set; }
    public PlayerParty Party { get; private set; }

    private BattleAction chosen;
    private BattleResult result = BattleResult.None;
    private SceneFlow scenes; // TAMBAHKAN INI

    // Event untuk UI/Presenter
    public event Action Started;
    public event Action<string> MessageShown;
    public event Action ChoiceRequested;
    public event Action<MonsterInstance> Attacked;
    public event Action<MonsterInstance, int, bool, Element?> Hit;
    public event Action Missed;
    public event Action<MonsterInstance> Fainted;
    public event Action Victory;
    public event Action<MonsterInstance> LevelUp;
    public event Action<MonsterInstance, MonsterInstance> Evolving;
    public event Action<MonsterInstance> Evolved;
    public event Action<BattleResult> Ended;

    private const float secondsPerChar = 0.018f;
    private const float evolveSeconds = 2.4f;

    public void Construct(PlayerParty party, BattleSession session, SceneFlow scenes)
    {
        Party = party;
        Player = party.Partner;
        Enemy = session.Enemy;
        IsBoss = session.IsBoss;
        this.scenes = scenes; // TAMBAHKAN INI
    }

    public IEnumerator Begin()
    {
        Started?.Invoke();
        yield return new WaitForSeconds(0.5f);

        yield return Say(IsBoss 
            ? $"{Enemy.Data.DisplayName}, penguasa Gua Petir, muncul!" 
            : $"{Enemy.Data.DisplayName} liar Lv {Enemy.Level} muncul!", 0.9f);
        yield return Say($"Maju, {Player.Data.DisplayName}!", 0.7f);

        while (result == BattleResult.None)
        {
            chosen = null;
            ChoiceRequested?.Invoke();
            yield return new WaitUntil(() => chosen != null);

            if (chosen is RunAction && IsBoss)
            {
                yield return chosen.Execute(this, Player, Enemy);
                continue;
            }

            BattleAction enemyAction = PickEnemyAction();
            bool playerFirst = chosen.Priority > 0 
                || Player.Speed > Enemy.Speed 
                || (Player.Speed == Enemy.Speed && UnityEngine.Random.value < 0.5f);

            if (playerFirst)
            {
                yield return Turn(chosen, Player, Enemy);
                if (result != BattleResult.None) break;
                yield return Turn(enemyAction, Enemy, Player);
            }
            else
            {
                yield return Turn(enemyAction, Enemy, Player);
                if (result != BattleResult.None) break;
                yield return Turn(chosen, Player, Enemy);
            }
        }

        Ended?.Invoke(result);
        yield return new WaitForSeconds(0.3f);

    }

    private IEnumerator Turn(BattleAction action, MonsterInstance user, MonsterInstance target)
    {
        yield return action.Execute(this, user, target);

        if (target.CurrentHP <= 0)
        {
            if (target == Enemy)
            {
                yield return Win();
            }
            else
            {
                yield return Lose();
            }
        }
    }

    private BattleAction PickEnemyAction()
    {
        // Sederhana dulu: Musuh selalu menyerang
        return new AttackAction(); 
    }

    private IEnumerator Win()
    {
        result = BattleResult.Win;
        Fainted?.Invoke(Enemy);
        yield return new WaitForSeconds(0.6f);
        yield return Say(IsBoss 
            ? $"{Enemy.Data.DisplayName} kalah!" 
            : $"{Enemy.Data.DisplayName} liar kalah!", 0.8f);
        
        Victory?.Invoke();
        int exp = (10 + Enemy.Level * 8) * (IsBoss ? 2 : 1);
        yield return Say($"{Player.Data.DisplayName} mendapat {exp} EXP.", 0.9f);

        // Logika Naik Level & Evolusi
        foreach (GrowthEvent growth in Player.GainExp(exp))
        {
            if (growth.Type == GrowthType.LevelUp)
            {
                LevelUp?.Invoke(growth.From);
                yield return Say(growth.Message, 0.9f);
                continue;
            }

            yield return Say($"Apa? {growth.From.Data.DisplayName} mulai bersinar...", 0.9f);
            Evolving?.Invoke(growth.From, growth.To);
            yield return new WaitForSeconds(evolveSeconds);
            Evolved?.Invoke(Player);
            yield return Say(growth.Message, 1.2f);
        }

        if (!IsBoss)
        {
            int healed = Player.Heal(Mathf.RoundToInt(Player.MaxHP * 0.25f));
            if (healed > 0) 
                yield return Say($"{Player.Data.DisplayName} beristirahat sejenak dan pulih {healed} HP.", 0.8f);
        }
    }

    private IEnumerator Lose()
    {
        result = BattleResult.Lose;
        Fainted?.Invoke(Player);
        yield return new WaitForSeconds(0.6f);
        yield return Say($"{Player.Data.DisplayName} pingsan...", 0.8f);
    
        // TAMBAHKAN INI: Pesan kedua sesuai modul
        yield return Say("Kamu berlari kembali ke desa.", 0.9f);
    
        // TAMBAHKAN INI: Pulihkan HP partner dan reset ramuan
        Player.Heal(Player.MaxHP); // HP penuh lagi
        if (Party != null)
        {
            // Reset ramuan ke 8 (sesuai modul slide 39)
            // Atau bisa juga tidak di-reset, tergantung desain game kamu
            // Party.AddPotions(8 - Party.Potions); 
        }
    }

    public void ChooseAction(BattleAction action)
    {
        chosen = action;
    }

    public void ForceFlee()
    {
        result = BattleResult.Flee;
    }

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

    public float DamageScale(MonsterInstance user) => 1f;

    public void RaiseAttacked(MonsterInstance user) => Attacked?.Invoke(user);
    public void RaiseHit(MonsterInstance target, int damage, bool critical, Element? element) 
        => Hit?.Invoke(target, damage, critical, element);
    public void RaiseMissed() => Missed?.Invoke();
}