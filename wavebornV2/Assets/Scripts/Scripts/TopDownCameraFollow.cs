using UnityEngine;

public class TopDownCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;            // Player
    [SerializeField] private Vector3 offset = new Vector3(0, 15f, -10f);
    [SerializeField] private float followLerp = 10f;
    [SerializeField] private Vector3 fixedEuler = new Vector3(45f, 0f, 0f);

    void LateUpdate()
    {
        if (!target) return;
        transform.position = Vector3.Lerp(transform.position, target.position + offset, followLerp * Time.deltaTime);
        transform.rotation = Quaternion.Euler(fixedEuler); // фіксований кут, не крутиться з гравцем
    }
}
