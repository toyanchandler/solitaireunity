namespace _Game.Scripts.Template.GlobalProviders.Health
{
    public interface IHealth
    {
        float CurrentHealth { get; }
        void TakeDamage(float damage);
        void Heal(float amount);
    }
}