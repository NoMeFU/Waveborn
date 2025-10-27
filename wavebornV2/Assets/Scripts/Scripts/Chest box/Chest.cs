using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChestItem
{
    public string itemName;
    [Range(0f, 1f)] public float dropChance = 0.5f;
    public GameObject itemPrefab;
}

public class Chest : MonoBehaviour
{
    [Header("Chest Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip rewardSound;
    [SerializeField] private ParticleSystem openEffect;
    [SerializeField] private List<ChestItem> possibleItems = new List<ChestItem>();

    [Header("Interaction")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float openDelay = 0.5f;

    private bool isOpened = false;
    private bool isPlayerNearby = false;

    private void Update()
    {
        if (isPlayerNearby && !isOpened && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(OpenChest());
        }
    }

    private IEnumerator OpenChest()
    {
        isOpened = true;

        // Звук відкриття
        if (audioSource && openSound)
            audioSource.PlayOneShot(openSound);

        // Анімація
        if (animator)
            animator.SetTrigger("Open");

        // Ефект
        if (openEffect)
            openEffect.Play();

        yield return new WaitForSeconds(openDelay);

        // Дроп предметів
        foreach (var item in possibleItems)
        {
            float roll = Random.value;
            if (roll <= item.dropChance && item.itemPrefab != null)
            {
                Instantiate(item.itemPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
                if (audioSource && rewardSound)
                    audioSource.PlayOneShot(rewardSound);
            }
        }

        yield return new WaitForSeconds(2f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerNearby = true;
            Debug.Log("Натисни [E], щоб відкрити скриню");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerNearby = false;
        }
    }
}
