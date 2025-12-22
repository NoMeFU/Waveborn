using UnityEngine;

public interface IPlayerController
{
    Vector3 GetLocalMove();
    float GetSpeed();
    float GetTurnInput();
    bool IsShooting { get; }
}
