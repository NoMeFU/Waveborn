using UnityEngine;

public class UpgradeNpc : MonoBehaviour
{
    [SerializeField] private UpgradeSystem upgradeSystem;
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private Transform player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // Якщо гравець поруч
        if (distance <= interactDistance)
        {

            if (Input.GetKeyDown(interactKey))
            {
                // Відкриваємо меню прокачки
                upgradeSystem.OpenMenu();
            }
        }
        else
        {
            // Якщо гравець далеко — закриваємо меню
            upgradeSystem.CloseMenu();
        }
    }
}
