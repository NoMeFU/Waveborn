// Assets/Scripts/System/UIInputHelper.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public static class UIInputHelper
{
    private static readonly List<RaycastResult> results = new();

    /// Перевіряє, чи під курсором є КЛІКАБЕЛЬНИЙ UI (кнопка/слайдер/інпут тощо).
    public static bool IsPointerOverClickableUI()
    {
        if (EventSystem.current == null) return false;

        var ped = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        results.Clear();
        EventSystem.current.RaycastAll(ped, results);

        for (int i = 0; i < results.Count; i++)
        {
            var go = results[i].gameObject;
            if (!go) continue;

            if (go.GetComponent<Button>() ||
                go.GetComponent<Toggle>() ||
                go.GetComponent<Slider>() ||
                go.GetComponent<Scrollbar>() ||
                go.GetComponent<InputField>() ||
                go.GetComponent<TMP_InputField>() ||
                go.GetComponent<Dropdown>() ||
                go.GetComponent<IPointerClickHandler>() != null)
                return true;
        }
        return false;
    }
}
