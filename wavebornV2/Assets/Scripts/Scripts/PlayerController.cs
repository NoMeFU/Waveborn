using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float gravity = 20f;

    [Header("Aim (screen space)")]
    [SerializeField] private Camera cam;
    [SerializeField] private bool instantRotation = true;   // миттєво чи плавно
    [SerializeField] private float turnSpeedDeg = 1080f;    // град/сек, якщо не миттєво
    [SerializeField] private float aimDeadPixels = 2f;      // мертва зона курсора (щоб не сіпалось)

    [Header("Weapons")]
    [SerializeField] private WeaponSwitcher weaponSwitcher;

    private CharacterController cc;
    private float verticalVel;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (!cam) cam = Camera.main;
        if (!weaponSwitcher) weaponSwitcher = GetComponentInChildren<WeaponSwitcher>();
    }

    void Update()
    {
        // РУХ та ПОВОРОТ — завжди
        HandleMoveCameraRelative();
        HandleAimByMousePosition();

        // СТРІЛЬБА / ПЕРЕМИКАННЯ — тільки якщо не блокує UI
        bool uiBlocked = InputBlocker.Blocked ||
                         (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject());
        if (!uiBlocked)
            HandleFireAndSwitch();
    }

    // ---- MOVE відносно камери ----
    private void HandleMoveCameraRelative()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 camF = cam.transform.forward; camF.y = 0f; camF.Normalize();
        Vector3 camR = cam.transform.right; camR.y = 0f; camR.Normalize();

        Vector3 moveXZ = camF * v + camR * h;
        if (moveXZ.sqrMagnitude > 1f) moveXZ.Normalize();

        Vector3 horizVel = moveXZ * moveSpeed;

        if (cc.isGrounded) verticalVel = -1f;
        else verticalVel -= gravity * Time.deltaTime;

        cc.Move(new Vector3(horizVel.x, verticalVel, horizVel.z) * Time.deltaTime);
    }

    // ---- AIM через mousePosition ----
    private void HandleAimByMousePosition()
    {
        if (!cam) return;

        Vector3 playerScreen = cam.WorldToScreenPoint(transform.position);
        Vector2 mouse = Input.mousePosition;
        Vector2 delta = new Vector2(mouse.x - playerScreen.x, mouse.y - playerScreen.y);

        if (delta.sqrMagnitude < aimDeadPixels * aimDeadPixels) return; // мертва зона

        Vector3 camF = cam.transform.forward; camF.y = 0f; camF.Normalize();
        Vector3 camR = cam.transform.right; camR.y = 0f; camR.Normalize();

        Vector3 worldDir = camR * delta.x + camF * delta.y;
        if (worldDir.sqrMagnitude < 1e-6f) return;

        Quaternion target = Quaternion.LookRotation(worldDir, Vector3.up);

        if (instantRotation)
            transform.rotation = target;
        else
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeedDeg * Time.deltaTime);
    }

    // ---- FIRE / SWITCH ----
    private void HandleFireAndSwitch()
    {
        if (Input.GetMouseButton(0)) weaponSwitcher?.Current?.TryAttack();
        if (Input.GetKeyDown(KeyCode.Alpha1)) weaponSwitcher?.SelectIndex(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) weaponSwitcher?.SelectIndex(1);
        if (Input.mouseScrollDelta.y != 0) weaponSwitcher?.SelectNext(Input.mouseScrollDelta.y > 0);
    }
}
