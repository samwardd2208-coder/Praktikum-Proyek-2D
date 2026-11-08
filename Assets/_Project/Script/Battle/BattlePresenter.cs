/// <summary>
/// Presenter (MVP): menghubungkan event BattleSystem (logika) ke BattleView/MonsterPanel (tampilan).
/// Bukan MonoBehaviour: dibuat dengan  new BattlePresenter(battle, view)  oleh BattleInstaller.
/// </summary>
public class BattlePresenter
{
    readonly BattleSystem battle;
    readonly BattleView view;

    public BattlePresenter(BattleSystem battle, BattleView view)
    {
        this.battle = battle;
        this.view = view;

        battle.Started         += OnStarted;                      // isi panel + PlayIntro
        battle.MessageShown    += view.ShowMessage;
        battle.ChoiceRequested += OnChoiceRequested;              // label, usable, prompt, hint
        battle.Attacked        += m => PanelOf(m).PlayLunge();
        battle.Hit             += OnHit;                          // Show + PlayHurt
        battle.Fainted         += m => PanelOf(m).PlayFaint();
        battle.LevelUp         += shown => view.PlayerPanel.Show(battle.Player, false, shown);
        battle.Evolving        += (before, after) =>
        {
            view.PlayerPanel.PlayEvolve(before, after, battle.EvolveSeconds);
            view.PlayEvolveRings(battle.EvolveSeconds);
        };
        battle.Evolved         += m => { view.PlayFlash(); view.PlayerPanel.Show(m); };
        view.ActionClicked     += OnActionClicked;
    }

    MonsterPanel PanelOf(MonsterInstance m) => m == battle.Player ? view.PlayerPanel : view.EnemyPanel;

    void OnStarted()
    {
        view.EnemyPanel.BossMark = battle.IsBoss;
        view.EnemyPanel.Show(battle.Enemy);
        view.PlayerPanel.Show(battle.Player);
        view.PlayIntro();
    }

    void OnChoiceRequested()
    {
        MonsterInstance p = battle.Player;
        int skills = battle.SkillsLeft(p);
        int potions = battle.PotionsLeft;

        // Urutan = grid 2 x 2:  Serang | Jurus
        //                       Ramuan | Kabur
        string[] labels =
        {
            "Serang",
            $"Jurus ({skills})",
            $"Ramuan ({potions})",
            "Kabur",
        };
        bool[] usable =
        {
            true,
            skills > 0,
            potions > 0 && p.CurrentHP < p.MaxHP,
            true,                     // Kabur selalu bisa dipilih; boss menolak lewat pesan
        };

        view.ShowMenu(labels, usable,
            $"Apa yang akan {p.Data.DisplayName} lakukan?",
            $"Jurus elemen: {SkillAction.MoveName(p)}");
    }

    void OnHit(MonsterInstance target, int damage, bool critical, Element? element)
    {
        MonsterPanel panel = PanelOf(target);
        panel.Show(target, false);        // snap = false: bar HP turun PELAN, bukan loncat
        panel.PlayHurt();
    }

    void OnActionClicked(int index)
    {
        battle.Choose(battle.PlayerActions[index]);
    }
}
