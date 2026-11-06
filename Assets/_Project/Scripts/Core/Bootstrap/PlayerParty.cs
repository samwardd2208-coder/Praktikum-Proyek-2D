public class PlayerParty
{
    public MonsterInstance Partner { get; private set; }

    public void SetPartner(MonsterInstance instance)
    {
        Partner = instance;
    }
}