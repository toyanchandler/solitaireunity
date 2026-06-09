namespace _Game.Scripts.Template.GlobalProviders.Combat
{
    //Required component of DamageableObject.cs script
    public interface IDamageableAction
    {
        void Initialize(DamageableObject damageableObject);
        
        void TakeDamage(float damage);
        
        void HealthChanged(float currentHealth);
        
        void Death();
    }
}