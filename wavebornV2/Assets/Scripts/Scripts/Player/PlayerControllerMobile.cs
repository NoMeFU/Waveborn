using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MobilePlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4.5f;
    [SerializeField] private float gravity = 20f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 12f;
    [SerializeField] private float aimRotationSpeed = 720f; // Швидкість повороту від стіка прицілу
    [SerializeField] private float aimDeadZone = 0.15f; // Мертва зона стіка
    [SerializeField] private float shootThreshold = 0.2f; // Поріг для стрільби

    [Header("Input")]
    [SerializeField] private Joystick moveJoystick; // Стік руху (лівий)
    [SerializeField] private Joystick aimJoystick;  // Стік прицілу (правий)

    [Header("Weapons")]
    [SerializeField] private WeaponSwitcher weaponSwitcher;

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;

    private CharacterController controller;
    private Vector3 moveDir;
    private float verticalVelocity;
    private float turnInput; // Для аніматора
    private bool isShooting; // Чи стріляємо зараз

    public Vector3 MoveInputWorld { get; private set; }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        // Шукаємо WeaponSwitcher якщо не прив'язаний
        if (!weaponSwitcher)
            weaponSwitcher = GetComponentInChildren<WeaponSwitcher>();

        // Якщо не прив'язано вручну - шукаємо автоматично
        if (!moveJoystick || !aimJoystick)
        {
            Joystick[] joysticks = FindObjectsOfType<Joystick>();
            Debug.Log($"<color=cyan>📱 Знайдено джойстиків: {joysticks.Length}</color>");

            for (int i = 0; i < joysticks.Length; i++)
            {
                Debug.Log($"  [{i}] {joysticks[i].name} на позиції {joysticks[i].transform.position}");
            }

            if (joysticks.Length >= 2)
            {
                if (!moveJoystick) moveJoystick = joysticks[0];
                if (!aimJoystick) aimJoystick = joysticks[1];
                Debug.Log($"<color=lime>✅ Автопідключення:</color> Move={moveJoystick.name}, Aim={aimJoystick.name}");
            }
            else if (joysticks.Length == 1)
            {
                Debug.LogWarning("<color=orange>⚠️ Знайдено тільки 1 джойстик! Потрібно 2!</color>");
            }
            else
            {
                Debug.LogError("<color=red>❌ Джойстики не знайдені на сцені!</color>");
            }
        }
        else
        {
            Debug.Log($"<color=green>✅ Джойстики прив'язані вручну:</color> Move={moveJoystick?.name}, Aim={aimJoystick?.name}");
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        if (!moveJoystick)
        {
            MoveInputWorld = Vector3.zero;
            return;
        }

        // 🎮 Отримуємо ввід з джойстика руху
        Vector3 input = new Vector3(moveJoystick.Horizontal, 0f, moveJoystick.Vertical);
        input = Vector3.ClampMagnitude(input, 1f);
        MoveInputWorld = input;

        // 🏃 Горизонтальний рух
        Vector3 horizontalMove = input * moveSpeed;

        // ⬇️ ГРАВІТАЦІЯ
        if (controller.isGrounded)
        {
            verticalVelocity = -1f;
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }

        // 🔀 Об'єднуємо рух
        Vector3 finalMove = new Vector3(horizontalMove.x, verticalVelocity, horizontalMove.z);
        controller.Move(finalMove * Time.deltaTime);
    }

    private void HandleRotation()
    {
        if (!aimJoystick)
        {
            if (showDebug && Time.frameCount % 60 == 0)
                Debug.LogError("<color=red>❌ AIM JOYSTICK НЕ ПРИВ'ЯЗАНИЙ!</color>");
            isShooting = false;
            return;
        }

        // 🎯 Отримуємо ввід з стіка прицілу
        float horizontal = aimJoystick.Horizontal;
        float vertical = aimJoystick.Vertical;

        // 🛡️ ЗАХИСТ ВІД NaN
        if (float.IsNaN(horizontal) || float.IsNaN(vertical))
        {
            if (showDebug && Time.frameCount % 60 == 0)
                Debug.LogWarning($"<color=red>⚠️ JOYSTICK ПОВЕРТАЄ NaN!</color> Перевірте тип джойстика. Має бути Dynamic або Floating, а не Fixed!");
            horizontal = 0f;
            vertical = 0f;
        }

        // 🔍 DEBUG - виводимо значення стіка
        if (showDebug && Time.frameCount % 30 == 0)
        {
            Debug.Log($"<color=yellow>🎮 AIM STICK:</color> H={horizontal:F2}, V={vertical:F2} | Magnitude={Mathf.Sqrt(horizontal * horizontal + vertical * vertical):F2}");
        }

        // Перевірка мертвої зони
        float magnitude = Mathf.Sqrt(horizontal * horizontal + vertical * vertical);

        // 🔫 СТРІЛЬБА: якщо стік за межами порогу - стріляємо!
        if (magnitude > shootThreshold)
        {
            if (!isShooting)
            {
                isShooting = true;
                if (showDebug)
                    Debug.Log($"<color=red>🔥 ПОЧАЛИ СТРІЛЯТИ!</color> Magnitude={magnitude:F2}");
            }

            // Стріляємо зі зброї
            if (weaponSwitcher && weaponSwitcher.Current)
            {
                weaponSwitcher.Current.TryAttack();
            }
        }
        else
        {
            if (isShooting && showDebug)
                Debug.Log("<color=cyan>🛑 ПРИПИНИЛИ СТРІЛЯТИ!</color>");
            isShooting = false;
        }

        if (magnitude < aimDeadZone)
        {
            turnInput = 0f;

            // Якщо стік прицілу не використовується - повертаємось за рухом
            if (moveJoystick && MoveInputWorld.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(MoveInputWorld, Vector3.up);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRot,
                    rotationSpeed * Time.deltaTime
                );
            }
            return;
        }

        // 🔄 ПОВОРОТ ЗА СТІКОМ ПРИЦІЛУ (як мишкою!)
        Vector3 aimDirection = new Vector3(horizontal, 0f, vertical);

        if (aimDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(aimDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                aimRotationSpeed * Time.deltaTime
            );

            // Зберігаємо turnInput для аніматора
            turnInput = Mathf.Clamp(horizontal, -1f, 1f);
        }
    }

    // 📊 Геттери для аніматора
    public float GetSpeed() => MoveInputWorld.magnitude * moveSpeed;
    public Vector3 GetLocalMove() => transform.InverseTransformDirection(MoveInputWorld * moveSpeed);
    public float GetTurnInput() => turnInput;
    public bool IsShooting => isShooting; // ⬅️ Тепер повертає реальний стан!

    // 🔍 DEBUG UI
    private void OnGUI()
    {
        if (!showDebug) return;

        GUI.color = Color.white;
        GUI.backgroundColor = new Color(0, 0, 0, 0.7f);

        int y = 10;
        int lineHeight = 25;

        // Статус джойстиків
        GUI.Label(new Rect(10, y, 500, 20), $"Move Joystick: {(moveJoystick ? "✅ " + moveJoystick.name : "❌ НЕ ПРИВ'ЯЗАНИЙ")}");
        y += lineHeight;

        GUI.Label(new Rect(10, y, 500, 20), $"Aim Joystick: {(aimJoystick ? "✅ " + aimJoystick.name : "❌ НЕ ПРИВ'ЯЗАНИЙ")}");
        y += lineHeight;

        if (moveJoystick)
        {
            GUI.color = Color.cyan;
            GUI.Label(new Rect(10, y, 500, 20), $"Move: H={moveJoystick.Horizontal:F2}, V={moveJoystick.Vertical:F2}");
            y += lineHeight;
        }

        if (aimJoystick)
        {
            float magnitude = Mathf.Sqrt(aimJoystick.Horizontal * aimJoystick.Horizontal + aimJoystick.Vertical * aimJoystick.Vertical);
            GUI.color = magnitude > shootThreshold ? Color.red : (magnitude > aimDeadZone ? Color.green : Color.yellow);
            GUI.Label(new Rect(10, y, 500, 20), $"Aim: H={aimJoystick.Horizontal:F2}, V={aimJoystick.Vertical:F2} | Mag={magnitude:F2} | Shooting={isShooting}");
            y += lineHeight;
        }

        GUI.color = Color.white;
        GUI.Label(new Rect(10, y, 500, 20), $"Player Rotation: {transform.eulerAngles.y:F1}°");
    }
}