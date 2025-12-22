using UnityEngine;

public class MobileAutoAimRotation : MonoBehaviour
{
    [Header("Targeting")]
    public float detectRadius = 8f;
    public LayerMask enemyLayer;

    [Header("Rotation")]
    public float autoRotateSpeed = 8f;
    public float joystickDeadZone = 0.15f;

    [Header("References")]
    public Joystick rotateJoystick;

    private Transform currentTarget;

    private void Update()
    {
        if (IsJoystickRotating())
            return;

        FindTarget();

        if (currentTarget)
        {
            RotateTowardsTarget();
        }
    }

    private bool IsJoystickRotating()
    {
        if (!rotateJoystick) return false;
        return Mathf.Abs(rotateJoystick.Horizontal) > joystickDeadZone;
    }

    private void FindTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectRadius, enemyLayer);

        float closestDistance = float.MaxValue;
        Transform closest = null;

        foreach (var hit in hits)
        {
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closest = hit.transform;
            }
        }

        currentTarget = closest;
    }

    private void RotateTowardsTarget()
    {
        Vector3 dir = currentTarget.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            autoRotateSpeed * Time.deltaTime
        );
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }
}

