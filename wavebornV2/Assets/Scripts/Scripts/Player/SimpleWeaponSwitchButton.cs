using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

public class SimpleWeaponSwitchButton : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button switchButton; // Кнопка зміни зброї

    [Header("Settings")]
    [SerializeField] private bool showOnMobileOnly = true;

    private void Start()
    {
        // Налаштування кнопки
        if (switchButton != null)
        {
            switchButton.onClick.RemoveAllListeners();
            switchButton.onClick.AddListener(OnSwitchButtonClicked);

            // Показуємо тільки на мобільних якщо потрібно
            bool isMobile = Application.isMobilePlatform;
#if UNITY_ANDROID || UNITY_IOS
            isMobile = true;
#endif

            if (showOnMobileOnly)
                switchButton.gameObject.SetActive(isMobile);
            else
                switchButton.gameObject.SetActive(true);

            Debug.Log($"<color=lime>✅ Кнопка зміни зброї налаштована. Mobile: {isMobile}</color>");
        }
    }

    private void OnDestroy()
    {
        if (switchButton != null)
            switchButton.onClick.RemoveListener(OnSwitchButtonClicked);
    }

    private void OnSwitchButtonClicked()
    {
        // Знаходимо гравця
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Гравець не знайдений!");
            return;
        }

        // Знаходимо WeaponSwitcher через рефлексію
        Component weaponSwitcher = player.GetComponent("WeaponSwitcher") as Component;
        if (weaponSwitcher == null)
        {
            // Шукаємо в дітях
            foreach (Transform child in player.GetComponentsInChildren<Transform>())
            {
                weaponSwitcher = child.GetComponent("WeaponSwitcher") as Component;
                if (weaponSwitcher != null)
                    break;
            }
        }

        if (weaponSwitcher != null)
        {
            // Викликаємо метод SelectNext через рефлексію
            MethodInfo selectNextMethod = weaponSwitcher.GetType().GetMethod("SelectNext");
            if (selectNextMethod != null)
            {
                selectNextMethod.Invoke(weaponSwitcher, new object[] { true });
                Debug.Log("<color=cyan>🔄 Зброю змінено!</color>");
            }
            else
            {
                Debug.LogWarning("Метод SelectNext не знайдено!");
            }
        }
        else
        {
            Debug.LogWarning("WeaponSwitcher не знайдено на гравці!");
        }
    }
}