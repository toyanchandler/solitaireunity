namespace _Game.Scripts.Template.GlobalProviders.Combat
{
    public interface IShooter
    {
        bool CanShoot { get; set; }

        void Shoot();
        
        abstract void OnFire();
    }
}