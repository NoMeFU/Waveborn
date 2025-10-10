using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float gravity = 20f;

    [Header("Camera & Rotation")]
    [SerializeField] private Camera cam;
    [SerializeField] private bool instantRotation = false;
    [SerializeField] private float turnSpeedDeg = 1080f;
    [SerializeField] private float aimDeadPixels = 2f;

    [Header("Weapons")]
    [SerializeField] private WeaponSwitcher weaponSwitcher;

    private CharacterController cc;
    private float verticalVel;
    private Vector3 moveDir;
    private float turnInput;
    private bool isShooting;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (!cam) cam = Camera.main;
        if (!weaponSwitcher) weaponSwitcher = GetComponentInChildren<WeaponSwitcher>();
    }

    private void Update()
    {
        HandleMovement();
        HandleRotationByMouse();
        HandleFireAndSwitch();
        //Debug.Log($"PlayerController: Speed={GetSpeed():F2}, LocalMove={GetLocalMove()}, IsShooting={IsShooting}");
    }

    // ===== РУХ =====
    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 camForward = cam.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = cam.transform.right;
        camRight.y = 0f;
        camRight.Normalize();

        moveDir = (camForward * v + camRight * h).normalized;
        Vector3 horizontalVel = moveDir * moveSpeed;

        if (cc.isGrounded)
            verticalVel = -1f;
        else
            verticalVel -= gravity * Time.deltaTime;

        Vector3 velocity = new Vector3(horizontalVel.x, verticalVel, horizontalVel.z);
        cc.Move(velocity * Time.deltaTime);
    }

    // ===== ПОВОРОТ =====
    private void HandleRotationByMouse()
    {
        if (!cam) return;

        Vector3 playerScreen = cam.WorldToScreenPoint(transform.position);
        Vector2 mouse = Input.mousePosition;
        Vector2 delta = new Vector2(mouse.x - playerScreen.x, mouse.y - playerScreen.y);

        if (delta.sqrMagnitude < aimDeadPixels * aimDeadPixels)
        {
            turnInput = 0f;
            return;
        }

        Vector3 camForward = cam.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = cam.transform.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 worldDir = camRight * delta.x + camForward * delta.y;
        if (worldDir.sqrMagnitude < 0.001f)
        {
            turnInput = 0f;
            return;
        }

        Quaternion targetRot = Quaternion.LookRotation(worldDir, Vector3.up);

        if (instantRotation)
            transform.rotation = targetRot;
        else
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeedDeg * Time.deltaTime);

        turnInput = Mathf.Clamp(delta.x / Screen.width * 10f, -1f, 1f);
    }

    // ===== ВОГОНЬ І ЗБРОЯ =====
    private void HandleFireAndSwitch()
    {
        bool uiBlocked = InputBlocker.Blocked ||
                         (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject());
        if (uiBlocked) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) weaponSwitcher?.SelectIndex(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) weaponSwitcher?.SelectIndex(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) weaponSwitcher?.SelectIndex(2);
        if (Input.mouseScrollDelta.y != 0) weaponSwitcher?.SelectNext(Input.mouseScrollDelta.y > 0);

        if (Input.GetMouseButtonDown(0))
        {
            var weapon = weaponSwitcher?.Current;
            if (weapon != null && weapon.TryAttack())
            {
                isShooting = true;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isShooting = false;
        }
    }

    // ===== API ДЛЯ АНІМАТОРА =====
    public Vector3 GetLocalMove()
    {
        // Повертає швидкість у локальних координатах
        return transform.InverseTransformDirection(moveDir * moveSpeed);
    }

    public float GetSpeed()
    {
        return moveDir.magnitude * moveSpeed;
    }

    public bool IsShooting => isShooting;
    public float GetTurnInput() => turnInput;
}
