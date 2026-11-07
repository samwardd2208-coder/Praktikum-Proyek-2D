public interface IDamageable
{
    int CurrentHP { get; }
    void TakeDamage(int amount);
}
