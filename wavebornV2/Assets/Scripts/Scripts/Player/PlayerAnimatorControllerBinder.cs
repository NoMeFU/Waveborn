using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimatorBinder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private WeaponSwitcher weaponSwitcher;

    [Header("Debug")]
    [SerializeField] private bool showDebug = false;

    // Хеші параметрів аніматора
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int ForwardHash = Animator.StringToHash("Forward");
    private static readonly int RightHash = Animator.StringToHash("Right");
    private static readonly int TurnHash = Animator.StringToHash("Turn");
    private static readonly int IsShootingHash = Animator.StringToHash("IsShooting");
    private static readonly int WeaponTypeHash = Animator.StringToHash("WeaponType");
    private static readonly int FireHash = Animator.StringToHash("Fire");

    private int lastWeaponType = -1;

    private void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();
        if (!playerController) playerController = GetComponentInParent<PlayerController>();
        if (!weaponSwitcher) weaponSwitcher = GetComponentInParent<WeaponSwitcher>();

        Debug.Log($"<color=cyan>PlayerAnimatorBinder Awake:</color> Animator={animator != null}, PlayerController={playerController != null}, WeaponSwitcher={weaponSwitcher != null}");
    }

    private void Start()
    {
        if (weaponSwitcher)
        {
            weaponSwitcher.WeaponChanged += OnWeaponChanged;
        }
    }

    private void OnDestroy()
    {
        if (weaponSwitcher)
        {
            weaponSwitcher.WeaponChanged -= OnWeaponChanged;
        }
    }

    private void OnWeaponChanged(WeaponBase newWeapon)
    {
        if (!animator) return;

        int typeValue = newWeapon ? (int)newWeapon.Type : 0;
        animator.SetInteger(WeaponTypeHash, typeValue);

        Debug.Log($"<color=yellow>⚔️ ЗБРОЯ ЗМІНЕНА:</color> {(newWeapon ? newWeapon.DisplayName : "Без зброї")} | WeaponType={typeValue}");
    }

    private void Update()
    {
        if (!animator || !playerController) return;

        UpdateMovementAnimation();
        UpdateCombatAnimation();
        UpdateWeaponTypeDebug();
    }

    private void UpdateMovementAnimation()
    {
        Vector3 localMove = playerController.GetLocalMove();
        float speed = playerController.GetSpeed();

        animator.SetFloat(SpeedHash, speed);
        animator.SetFloat(ForwardHash, localMove.z);
        animator.SetFloat(RightHash, localMove.x);
        animator.SetFloat(TurnHash, playerController.GetTurnInput());

        if (showDebug)
        {
            Debug.Log($"<color=lime>[MOVEMENT]</color> Speed={speed:F2} | Forward={localMove.z:F2} | Right={localMove.x:F2}");
        }
    }

    private void UpdateCombatAnimation()
    {
        bool isShooting = playerController.IsShooting;
        animator.SetBool(IsShootingHash, isShooting);

        // Оновлення WeaponType кожен кадр
        int currentWeaponType = 0;
        if (weaponSwitcher && weaponSwitcher.Current)
        {
            currentWeaponType = (int)weaponSwitcher.Current.Type;
        }

        animator.SetInteger(WeaponTypeHash, currentWeaponType);
    }

    private void UpdateWeaponTypeDebug()
    {
        if (!showDebug) return;

        int currentWeaponType = animator.GetInteger(WeaponTypeHash);

        if (currentWeaponType != lastWeaponType)
        {
            string weaponName = currentWeaponType == 0 ? "БЕЗ ЗБРОЇ" :
                               currentWeaponType == 1 ? "ПІСТОЛЕТ" :
                               currentWeaponType == 2 ? "ГВИНТІВКА" :
                               currentWeaponType == 3 ? "МЕЧ" : "НЕВІДОМО";

            Debug.Log($"<color=orange>🔄 WeaponType ЗМІНЕНО:</color> {lastWeaponType} → {currentWeaponType} ({weaponName})");

            // Перевірка стану Animator
            var currentState = animator.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"<color=cyan>📊 Поточний стан:</color> {currentState.shortNameHash} | IsName(Pistol)={currentState.IsName("Pistol SubState")} | IsName(Unarmed)={currentState.IsName("Unnamed substate")}");

            lastWeaponType = currentWeaponType;
        }
    }

    private void OnGUI()
    {
        if (!showDebug || !animator) return;

        int y = 10;
        int lineHeight = 25;

        GUI.color = Color.white;
        GUI.Label(new Rect(10, y, 400, 20), $"Speed: {animator.GetFloat(SpeedHash):F2}");
        y += lineHeight;

        GUI.Label(new Rect(10, y, 400, 20), $"Forward: {animator.GetFloat(ForwardHash):F2}");
        y += lineHeight;

        GUI.Label(new Rect(10, y, 400, 20), $"Right: {animator.GetFloat(RightHash):F2}");
        y += lineHeight;

        int weaponType = animator.GetInteger(WeaponTypeHash);
        GUI.color = weaponType > 0 ? Color.green : Color.yellow;
        GUI.Label(new Rect(10, y, 400, 20), $"WeaponType: {weaponType}");
        y += lineHeight;

        GUI.color = Color.cyan;
        if (weaponSwitcher && weaponSwitcher.Current)
        {
            GUI.Label(new Rect(10, y, 400, 20), $"Current Weapon: {weaponSwitcher.Current.DisplayName}");
        }
        else
        {
            GUI.Label(new Rect(10, y, 400, 20), "Current Weapon: NONE");
        }
    }
}