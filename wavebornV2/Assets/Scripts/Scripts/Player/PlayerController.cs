using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Move Settings")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float gravity = 20f;

    [Header("Camera & Rotation")]
    [SerializeField] private Camera cam;
    [SerializeField] private bool instantRotation = false;
    [SerializeField] private float turnSpeedDeg = 1080f;
    [SerializeField] private float aimDeadPixels = 2f;

    [Header("Weapons")]
    [SerializeField] private WeaponSwitcher weaponSwitcher;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string forwardParam = "Forward";
    [SerializeField] private string rightParam = "Right";
    [SerializeField] private string fireTrigger = "Fire";

    private CharacterController cc;
    private float verticalVel;
    private Vector3 moveDir;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (!cam) cam = Camera.main;
        if (!weaponSwitcher) weaponSwitcher = GetComponentInChildren<WeaponSwitcher>();
        if (!animator) animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        HandleMovement();
        HandleRotationByMouse();
        HandleFireAndSwitch();
        UpdateAnimator();
    }

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

    private void HandleRotationByMouse()
    {
        if (!cam) return;

        Vector3 playerScreen = cam.WorldToScreenPoint(transform.position);
        Vector2 mouse = Input.mousePosition;
        Vector2 delta = new Vector2(mouse.x - playerScreen.x, mouse.y - playerScreen.y);

        if (delta.sqrMagnitude < aimDeadPixels * aimDeadPixels) return;

        Vector3 camForward = cam.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = cam.transform.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 worldDir = camRight * delta.x + camForward * delta.y;
        if (worldDir.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(worldDir, Vector3.up);

        if (instantRotation)
            transform.rotation = targetRot;
        else
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeedDeg * Time.deltaTime);
    }

    private void HandleFireAndSwitch()
    {
        bool uiBlocked = InputBlocker.Blocked ||
                         (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject());

        if (uiBlocked) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) weaponSwitcher?.SelectIndex(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) weaponSwitcher?.SelectIndex(1);
        if (Input.mouseScrollDelta.y != 0) weaponSwitcher?.SelectNext(Input.mouseScrollDelta.y > 0);

        if (Input.GetMouseButtonDown(0))
        {
            var w = weaponSwitcher?.Current;
            if (w != null)
            {
                bool ok = w.TryAttack();
                if (ok)
                    animator?.SetTrigger(fireTrigger);
            }
        }
    }

    private void UpdateAnimator()
    {
        if (!animator) return;

        // швидкість руху (для переходу між idle/run)
        float speed = new Vector2(moveDir.x, moveDir.z).magnitude;

        // локальний напрям руху відносно гравця
        Vector3 localDir = transform.InverseTransformDirection(moveDir);

        animator.SetFloat("Speed", speed, 0.1f, Time.deltaTime);
        animator.SetFloat("Forward", localDir.z, 0.1f, Time.deltaTime);
        animator.SetFloat("Right", localDir.x, 0.1f, Time.deltaTime);
    }


    public void OnAnimFireEvent()
    {
        weaponSwitcher?.Current?.AnimFire();
    }
}
