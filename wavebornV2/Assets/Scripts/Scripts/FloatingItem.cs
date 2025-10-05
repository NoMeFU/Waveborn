using UnityEngine;

public class FloatingItem : MonoBehaviour
{
    [Header("Floating Settings")]
    [SerializeField] private float rotateSpeed = 90f;
    [SerializeField] private float floatAmplitude = 0.25f;
    [SerializeField] private float floatSpeed = 2f;

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        // Обертання навколо вертикальної осі
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime, Space.World);

        // Рух вгору-вниз (синусоїда)
        transform.position = startPos + Vector3.up * Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
    }
}
