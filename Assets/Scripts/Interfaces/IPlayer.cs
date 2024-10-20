public interface IPlayer
{
    public void PlayerAimController();
    public void PlayerShootController();
    public void PlayerAim();
    public void PlayerShoot();
    public void TakeDamage(float damage, string shooterName);
    public void InZoneCheck();
}
