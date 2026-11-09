using System;
using System.Collections;
using UnityEngine;

public class BattleSystem : MonoBehaviour
{
    // ---------- Pengaturan ----------
    [SerializeField] float secondsPerChar = 0.018f;
    [SerializeField] float evolveSeconds = 2.4f;

    // ---------- Event (didengar Presenter / View) ----------
    public event Action Started;
    public event Action<string> MessageShown;
    public event Action ChoiceRequested;
    public event Action<MonsterInstance> Attacked;
    public event Action<MonsterInstance, int, bool, Element?> Hit;
    public event Action Missed;
    public event Action<MonsterInstance> Fainted;
    public event Action Victory;
    public event Action<MonsterData> LevelUp;
    public event Action<MonsterData, MonsterData> Evolving;
    public event Action<MonsterInstance> Evolved;
    public event Action<BattleResult> Ended;

    // ---------- State ----------
    PlayerParty party;
    BattleSession session;
    SceneFlow scenes;

    BattleResult result = BattleResult.None;
    BattleAction chosen;
    int skillUses;
    int enemySkillUses;

    public MonsterInstance Player => party.Partner;
    public MonsterInstance Enemy => session.Enemy;
    public bool IsBoss => session.IsBoss;
    public PlayerParty Party => party;

    public const int MaxSkillUses = 3;
    public int SkillLeft => skillUses;

    // ---------- Setup ----------
    public void Construct(PlayerParty p, BattleSession s, SceneFlow f)
    {
        party = p;
        session = s;
        scenes = f;
    }

    public void Begin()
    {
        skillUses = MaxSkillUses;
        enemySkillUses = 2;
        result = BattleResult.None;
        StartCoroutine(Run());
    }

    public void Choose(BattleAction action)
    {
        chosen = action;
    }

    // ---------- Dipanggil BattleAction ----------
    public void RaiseAttacked(MonsterInstance m)
    {
        Attacked?.Invoke(m);
    }

    public void RaiseMissed()
    {
        Missed?.Invoke();
    }

    public void RaiseHit(MonsterInstance target, int dmg, bool crit, Element? move)
    {
        Hit?.Invoke(target, dmg, crit, move);
    }

    public float DamageScale(MonsterInstance user)
    {
        if (user == Enemy && !IsBoss) return 0.8f;
        return 1f;
    }

    public void Escape()
    {
        result = BattleResult.Escaped;
    }

    // ---------- Pesan ----------
    public IEnumerator Say(string text, float hold = 0.8f)
    {
        MessageShown?.Invoke(text);
        float typing = text.Length * secondsPerChar;
        float wait = hold + typing;
        for (float t = 0f; t < wait; t += Time.deltaTime)
        {
            if (t > typing && MenuInput.Confirm) break;
            yield return null;
        }
    }

    // ---------- Alur utama ----------
    IEnumerator Run()
    {
        Started?.Invoke();
        yield return new WaitForSeconds(0.5f);

        if (IsBoss)
            yield return Say(Enemy.Data.DisplayName + ", penguasa Gua Petir, muncul!", 0.9f);
        else
            yield return Say(Enemy.Data.DisplayName + " liar Lv" + Enemy.Level + " muncul!", 0.9f);

        yield return Say("Maju, " + Player.Data.DisplayName + "!", 0.7f);

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
                yield return Turn(enemyAction, Enemy, Player);
            }
            else
            {
                yield return Turn(enemyAction, Enemy, Player);
                yield return Turn(chosen, Player, Enemy);
            }
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
        if (enemySkillUses > 0 && UnityEngine.Random.value < 0.35f)
        {
            enemySkillUses--;
            return new SkillAction();
        }
        return new AttackAction();
    }

    // ---------- Menang ----------
    IEnumerator Win()
    {
        result = BattleResult.Win;
        Fainted?.Invoke(Enemy);
        yield return new WaitForSeconds(0.6f);

        if (IsBoss)
            yield return Say(Enemy.Data.DisplayName + " kalah!", 0.8f);
        else
            yield return Say(Enemy.Data.DisplayName + " liar kalah!", 0.8f);

        Victory?.Invoke();

        int exp = (10 + Enemy.Level * 8) * (IsBoss ? 2 : 1);
        yield return Say(Player.Data.DisplayName + " mendapat " + exp + " EXP.", 0.9f);

        foreach (GrowthEvent growth in Player.GainExp(exp))
        {
            if (growth.Type == GrowthType.LevelUp)
            {
                LevelUp?.Invoke(growth.From);
                yield return Say(growth.Message, 0.9f);
                continue;
            }

            yield return Say("Apa? " + growth.From.DisplayName + " mulai bersinar...", 0.9f);
            Evolving?.Invoke(growth.From, growth.To);
            yield return new WaitForSeconds(evolveSeconds);
            Evolved?.Invoke(Player);
            yield return Say(growth.Message, 1.2f);
        }

        if (!IsBoss)
        {
            int healed = Player.Heal(Mathf.RoundToInt(Player.MaxHP * 0.25f));
            if (healed > 0)
                yield return Say(Player.Data.DisplayName + " beristirahat sejenak dan pulih " + healed + " HP.", 0.8f);
        }
    }

    // ---------- Kalah ----------
    IEnumerator Lose()
    {
        result = BattleResult.Lose;
        Fainted?.Invoke(Player);
        yield return new WaitForSeconds(0.6f);
        yield return Say(Player.Data.DisplayName + " pingsan... Kamu berlari kembali ke desa.", 1.2f);
        Player.Heal(Player.MaxHP);
    }

    // ---------- Menu pemain (dipakai Presenter) ----------
    public string[] MenuLabels()
    {
        return new string[]
        {
            "Serang",
            "Jurus (" + skillUses + ")",
            "Ramuan (" + party.Potions + ")",
            "Kabur"
        };
    }

    public bool[] MenuUsable()
    {
        return new bool[]
        {
            true,
            skillUses > 0,
            party.Potions > 0 && Player.CurrentHP < Player.MaxHP,
            true
        };
    }

    public string SkillHint()
    {
        return "Jurus elemen: " + Player.Data.SkillName;
    }

    public void ChooseByIndex(int index)
    {
        switch (index)
        {
            case 0: Choose(new AttackAction()); break;
            case 1: skillUses--; Choose(new SkillAction()); break;
            case 2: Choose(new PotionAction()); break;
            case 3: Choose(new RunAction()); break;
        }
    }
}