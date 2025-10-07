using UnityEngine;

public class PickupCoin : MonoBehaviour
{
    [Header("Coin Settings")]
    [SerializeField] private int coinValue = 1;        
    [SerializeField] private AudioClip pickupSound;    
    [SerializeField] private float destroyDelay = 0.1f;

    private AudioSource audioSource;
    private bool isPickedUp;

    private void Awake()
    {
        // додаємо аудіо, якщо немає
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isPickedUp) return;

        if (other.CompareTag("Player"))
        {
            isPickedUp = true;

            // додаємо монети гравцю
            var wallet = other.GetComponent<PlayerWallet>();
            if (wallet != null)
            {
                wallet.AddCoins(coinValue);
            }

            // звук
            if (pickupSound != null)
                audioSource.PlayOneShot(pickupSound);

            // відключаємо модель монетки
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<Collider>().enabled = false;

            // знищення після невеликої паузи
            Destroy(gameObject, destroyDelay);
        }
    }
}
