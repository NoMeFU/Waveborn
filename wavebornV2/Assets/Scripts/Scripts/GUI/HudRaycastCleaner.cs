using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HudRaycastCleaner : MonoBehaviour
{
    void Awake()
    {
        foreach (var img in GetComponentsInChildren<Image>(true)) img.raycastTarget = false;
        foreach (var txt in GetComponentsInChildren<TMP_Text>(true)) txt.raycastTarget = false;
    }
}
