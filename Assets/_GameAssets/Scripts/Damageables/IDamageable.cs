
public interface IDamageable
{
    void Damage(PlayerVehicleController playerVehicleController,string playerName);
    ulong GetKillerClientId();
    int GetDamageAmount();
    int GetRespawnTimer();
    string GetKillerName();
}
