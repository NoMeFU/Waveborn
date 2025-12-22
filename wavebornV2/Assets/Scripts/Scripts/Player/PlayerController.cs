using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float gravity = 20f;

    [Header("Smooth Motion")]
    [SerializeField] private float accel = 12f;
    [SerializeField] private float decel = 18f;

    private Vector3 smoothVelocity;
    private float verticalVel;

    [Header("Camera & Rotation")]
    [SerializeField] private Camera cam;
    [SerializeField] private bool instantRotation = false;
    [SerializeField] private float turnSpeedDeg = 1080f;
    [SerializeField] private float aimDeadPixels = 2f;

    [Header("Weapons")]
    [SerializeField] private WeaponSwitcher weaponSwitcher;

    private CharacterController cc;

    private Vector3 currentMoveDirection;
    private float currentSpeed;
    private float turnInput;
    private bool isShooting;

    public bool canMove = true;

    // Base values (for upgrades)
    public float BaseMoveSpeed => moveSpeed;
    public float BaseFireRate { get; private set; } = 1f;
    public float BaseBuffDuration { get; private set; } = 5f;

    private float currentBuffDuration = 5f;
    private float currentFireRate = 1f;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (!cam) cam = Camera.main;
        if (!weaponSwitcher) weaponSwitcher = GetComponentInChildren<WeaponSwitcher>();
    }

    private void Update()
    {
        if (!canMove) return;

        HandleMovement();
        HandleRotationByMouse();
        HandleFireAndSwitch();

        // КРИТИЧНО: Фіксуємо rotation щоб персонаж не перевертався
        KeepUpright();
    }

    // ============================================================
    //                   ФІКСАЦІЯ ПОЛОЖЕННЯ
    // ============================================================
    private void KeepUpright()
    {
        // Примусово тримаємо персонажа вертикально
        Vector3 currentEuler = transform.eulerAngles;
        transform.eulerAngles = new Vector3(0f, currentEuler.y, 0f);
    }

    // ============================================================
    //                         MOVEMENT
    // ============================================================
    private void HandleMovement()
    {
        // 1 — Input
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(h, 0, v);
        input = Vector3.ClampMagnitude(input, 1f);

        // 2 — Напрямки камери (строго горизонтальні)
        Vector3 fwd = cam.transform.forward;
        fwd.y = 0;
        fwd.Normalize();

        Vector3 right = cam.transform.right;
        right.y = 0;
        right.Normalize();

        // 3 — Бажаний напрямок руху
        Vector3 desired = (fwd * input.z + right * input.x);
        if (desired.magnitude > 0.01f)
        {
            desired.Normalize();
            desired *= moveSpeed;
        }

        // 4 — Плавний рух (accel / decel)
        if (input.magnitude > 0.1f)
        {
            smoothVelocity = Vector3.Lerp(smoothVelocity, desired, accel * Time.deltaTime);
        }
        else
        {
            smoothVelocity = Vector3.Lerp(smoothVelocity, Vector3.zero, decel * Time.deltaTime);
        }

        // 5 — Гравітація
        if (cc.isGrounded)
        {
            verticalVel = -2f; // Невелике значення щоб тримати на землі
        }
        else
        {
            verticalVel -= gravity * Time.deltaTime;
        }

        // 6 — Фінальна швидкість (ГОРИЗОНТАЛЬНА + ВЕРТИКАЛЬНА)
        Vector3 finalVel = new Vector3(smoothVelocity.x, verticalVel, smoothVelocity.z);

        // 7 — Move
        cc.Move(finalVel * Time.deltaTime);

        // Зберігаємо для анімації
        currentMoveDirection = new Vector3(smoothVelocity.x, 0, smoothVelocity.z).normalized;
        currentSpeed = new Vector3(smoothVelocity.x, 0, smoothVelocity.z).magnitude;
    }

    // ============================================================
    //                         ROTATION
    // ============================================================
    private void HandleRotationByMouse()
    {
        if (!cam) return;

        Vector3 mousePos = Input.mousePosition;
        Vector3 screenPos = cam.WorldToScreenPoint(transform.position);

        Vector2 delta = new Vector2(mousePos.x - screenPos.x, mousePos.y - screenPos.y);

        if (delta.sqrMagnitude < aimDeadPixels * aimDeadPixels)
            return;

        // Напрямки камери (строго горизонтальні)
        Vector3 fwd = cam.transform.forward;
        fwd.y = 0;
        fwd.Normalize();

        Vector3 right = cam.transform.right;
        right.y = 0;
        right.Normalize();

        Vector3 worldDir = right * delta.x + fwd * delta.y;
        if (worldDir.sqrMagnitude < 0.001f) return;

        worldDir.y = 0; // Гарантуємо горизонтальність
        worldDir.Normalize();

        Quaternion targetRot = Quaternion.LookRotation(worldDir, Vector3.up);

        if (instantRotation)
        {
            transform.rotation = targetRot;
        }
        else
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                turnSpeedDeg * Time.deltaTime
            );
        }

        turnInput = Mathf.Clamp(delta.x / Screen.width * 10f, -1f, 1f);
    }

    // ============================================================
    //                     FIRE & WEAPON SWITCHING
    // ============================================================
    private void HandleFireAndSwitch()
    {
        bool uiBlocked = InputBlocker.Blocked ||
                         (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject());

        if (uiBlocked) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) weaponSwitcher?.SelectIndex(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) weaponSwitcher?.SelectIndex(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) weaponSwitcher?.SelectIndex(2);

        if (Input.mouseScrollDelta.y != 0)
            weaponSwitcher?.SelectNext(Input.mouseScrollDelta.y > 0);

        if (Input.GetMouseButtonDown(0))
        {
            var w = weaponSwitcher?.Current;
            if (w != null && w.TryAttack())
                isShooting = true;
        }
        else if (Input.GetMouseButtonUp(0))
            isShooting = false;
    }

    // ============================================================
    //                            API
    // ============================================================
    public void SetSpeed(float newSpeed) => moveSpeed = newSpeed;
    public void SetFireRate(float newRate) => currentFireRate = newRate;
    public void SetBuffDuration(float newDuration) => currentBuffDuration = newDuration;

    // ============================================================
    //                        ANIMATION GETTERS
    // ============================================================
    public Vector3 GetLocalMove()
    {
        return transform.InverseTransformDirection(currentMoveDirection);
    }

    public float GetSpeed() => currentSpeed;
    public bool IsShooting => isShooting;
    public float GetTurnInput() => turnInput;
}