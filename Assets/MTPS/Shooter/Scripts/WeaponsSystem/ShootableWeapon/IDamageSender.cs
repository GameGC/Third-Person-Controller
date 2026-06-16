namespace MTPS.Shooter.WeaponsSystem.ShootableWeapon
{
    public interface IDamageSender
    {
        public float damage { get; }
        public SurfaceHitType HitType { get; }
    }
}