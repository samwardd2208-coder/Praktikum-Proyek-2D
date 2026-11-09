using UnityEngine;

public class BattlePresenter
{
    private readonly BattleSystem battle;
    private readonly BattleView view;

    public BattlePresenter(BattleSystem battle, BattleView view)
    {
        this.battle = battle;
        this.view = view;

        battle.Started += OnStarted;
        battle.MessageShown += view.ShowMessage;
        battle.ChoiceRequested += OnChoiceRequested;
        battle.Attacked += m => PanelOf(m)?.PlayLunge();
        battle.Hit += OnHit;
        battle.Fainted += m => PanelOf(m)?.PlayFaint();
        battle.LevelUp += shown => view.playerPanel.Show(battle.Player, false, shown);
        battle.Evolving += (from, to) => 
        {
            view.playerPanel.PlayEvolve(from, to, 2.4f);
            view.PlayEvolveRings(2.4f);
        };
        battle.Evolved += m => 
        {
            view.PlayFlash();
            view.playerPanel.Show(m);
        };
        view.ActionClicked += OnActionClicked;
    }

    private void OnStarted()
    {
        view.enemyPanel.Show(battle.Enemy);
        view.playerPanel.Show(battle.Player);
        view.PlayIntro();
    }

    private void OnChoiceRequested()
    {
        string[] labels = { "Serang", "Jurus", "Ramuan", "Kabur" };
        bool[] usable = { true, true, battle.Party.Potions > 0, !battle.IsBoss };
        string prompt = $"Apa yang akan {battle.Player.Data.DisplayName} lakukan?";
        string hint = $"Jurus elemen: {battle.Player.Data.SkillName}";
        
        view.ShowMenu(labels, usable, prompt, hint);
    }

    private void OnActionClicked(int index)
    {
        BattleAction action = index switch
        {
            0 => new AttackAction(),
            1 => new AttackAction(), // Jurus (sederhana dulu)
            2 => new PotionAction(),
            3 => new RunAction(),
            _ => new AttackAction()
        };
        battle.ChooseAction(action);
    }

    private void OnHit(MonsterInstance target, int damage, bool critical, Element? element)
    {
        view.ShowMessage($"{target.Data.DisplayName} terkena {damage} damage!");
        PanelOf(target).PlayHurt();
    }

   private MonsterPanel PanelOf(MonsterInstance monster)
    {
        if (monster == null) return null;
    
        if (monster == battle.Enemy && view.enemyPanel != null) 
            return view.enemyPanel;
        if (monster == battle.Player && view.playerPanel != null) 
            return view.playerPanel;
    
    return null;
    }
}