using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    [SerializeField] private Texture2D cursorTexture;
    [SerializeField] private Vector2 hotspot = Vector2.zero;
    [SerializeField] private CursorMode cursorMode = CursorMode.Auto;

    void Start()
    {
        if (cursorTexture)
        {
            // hotspot = центр текстури, щоб приціл був по центру
            Vector2 center = new Vector2(cursorTexture.width / 2f, cursorTexture.height / 2f);
            Cursor.SetCursor(cursorTexture, center, cursorMode);
        }
    }
}
    