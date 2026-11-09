using System.Collections;

public abstract class BattleAction
{
    public abstract int Priority { get; }
    public abstract IEnumerator Execute(BattleSystem battle, 
        MonsterInstance user, MonsterInstance target);
}