public interface IGrenadeInfo
{
    int CurrentGrenades { get; }
    int MaxGrenades { get; }
    event System.Action<int, int> GrenadeCountChanged; // (current, max)
}
