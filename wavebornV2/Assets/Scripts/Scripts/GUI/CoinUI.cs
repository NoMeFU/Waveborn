using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CoinUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private PlayerWallet playerWallet;
    [SerializeField] private Image coinIcon;
    [SerializeField] private TextMeshProUGUI coinText;

    [Header("Animation Settings")]
    [SerializeField] private float popScale = 1.3f;
    [SerializeField] private float popSpeed = 6f;

    private Vector3 initialScale;
    private bool isPopping;

    private void Start()
    {
        if (playerWallet == null)
            playerWallet = FindObjectOfType<PlayerWallet>();

        if (playerWallet != null)
            playerWallet.OnCoinsChanged += UpdateUI;

        initialScale = transform.localScale;
        UpdateUI(playerWallet?.Coins ?? 0);
    }

    private void OnDestroy()
    {
        if (playerWallet != null)
            playerWallet.OnCoinsChanged -= UpdateUI;
    }

    private void UpdateUI(int coins)
    {
        if (coinText != null)
            coinText.text = coins.ToString();

        if (!isPopping)
            StartCoroutine(AnimatePop());
    }

    private System.Collections.IEnumerator AnimatePop()
    {
        isPopping = true;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * popSpeed;
            float scale = Mathf.Lerp(1f, popScale, Mathf.Sin(t * Mathf.PI));
            transform.localScale = initialScale * scale;
            yield return null;
        }
        transform.localScale = initialScale;
        isPopping = false;
    }
}
