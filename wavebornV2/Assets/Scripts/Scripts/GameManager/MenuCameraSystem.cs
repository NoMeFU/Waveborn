using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuCameraSystem : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Camera[] cameras;
    [SerializeField] private float switchInterval = 5f;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationAngle = 30f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("TV Static Effect")]
    [SerializeField] private Canvas staticCanvas;
    [SerializeField] private RawImage staticImage;
    [SerializeField] private float staticDuration = 0.3f;

    private int currentCameraIndex = 0;
    private float timer = 0f;
    private bool isRotatingRight = true;
    private Texture2D noiseTexture;

    void Start()
    {
        // Активуємо тільки першу камеру
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].gameObject.SetActive(i == 0);
        }

        // Створюємо canvas для ефекту, якщо його немає
        if (staticCanvas == null)
        {
            CreateStaticCanvas();
        }
        else
        {
            staticCanvas.gameObject.SetActive(false);
        }

        // Створюємо текстуру шуму
        CreateNoiseTexture();
    }

    void Update()
    {
        // Обертання активної камери
        RotateCurrentCamera();

        // Таймер для перемикання камер
        timer += Time.deltaTime;
        if (timer >= switchInterval)
        {
            timer = 0f;
            StartCoroutine(SwitchCamera());
        }
    }

    void RotateCurrentCamera()
    {
        Camera currentCam = cameras[currentCameraIndex];
        Vector3 currentRotation = currentCam.transform.localEulerAngles;

        // Нормалізуємо кут до діапазону -180 до 180
        float yAngle = currentRotation.y;
        if (yAngle > 180f) yAngle -= 360f;

        // Перемикаємо напрямок обертання при досягненні меж
        if (isRotatingRight && yAngle >= rotationAngle)
        {
            isRotatingRight = false;
        }
        else if (!isRotatingRight && yAngle <= -rotationAngle)
        {
            isRotatingRight = true;
        }

        // Обертаємо камеру
        float targetY = isRotatingRight ? rotationAngle : -rotationAngle;
        float newY = Mathf.MoveTowards(yAngle, targetY, rotationSpeed * Time.deltaTime);

        currentCam.transform.localEulerAngles = new Vector3(
            currentRotation.x,
            newY,
            currentRotation.z
        );
    }

    IEnumerator SwitchCamera()
    {
        // Показуємо ефект шипіння
        yield return StartCoroutine(ShowStaticEffect());

        // Вимикаємо поточну камеру
        cameras[currentCameraIndex].gameObject.SetActive(false);

        // Переходимо до наступної камери
        currentCameraIndex = (currentCameraIndex + 1) % cameras.Length;

        // Вмикаємо нову камеру
        cameras[currentCameraIndex].gameObject.SetActive(true);

        // Скидаємо напрямок обертання
        isRotatingRight = true;
    }

    IEnumerator ShowStaticEffect()
    {
        // Показуємо canvas
        staticCanvas.gameObject.SetActive(true);

        // Анімуємо шипіння
        float elapsed = 0f;
        while (elapsed < staticDuration)
        {
            elapsed += Time.deltaTime;

            // Оновлюємо шум
            UpdateNoiseTexture();

            yield return null;
        }

        // Ховаємо canvas
        staticCanvas.gameObject.SetActive(false);
    }

    void CreateStaticCanvas()
    {
        // Створюємо Canvas
        GameObject canvasObj = new GameObject("Static Canvas");
        staticCanvas = canvasObj.AddComponent<Canvas>();
        staticCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        staticCanvas.sortingOrder = 9999; // Поверх усього

        // Додаємо CanvasScaler
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        // Створюємо Image для шуму
        GameObject imageObj = new GameObject("Static Image");
        imageObj.transform.SetParent(canvasObj.transform, false);

        staticImage = imageObj.AddComponent<RawImage>();
        RectTransform rect = staticImage.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // Ховаємо canvas спочатку
        staticCanvas.gameObject.SetActive(false);
    }

    void CreateNoiseTexture()
    {
        noiseTexture = new Texture2D(256, 256);
        noiseTexture.filterMode = FilterMode.Point;

        if (staticImage != null)
        {
            staticImage.texture = noiseTexture;
        }

        UpdateNoiseTexture();
    }

    void UpdateNoiseTexture()
    {
        if (noiseTexture == null) return;

        // Генеруємо випадковий шум
        for (int y = 0; y < noiseTexture.height; y++)
        {
            for (int x = 0; x < noiseTexture.width; x++)
            {
                float noise = Random.Range(0f, 1f);
                noiseTexture.SetPixel(x, y, new Color(noise, noise, noise, 1f));
            }
        }
        noiseTexture.Apply();
    }

    void OnDestroy()
    {
        // Очищаємо текстуру
        if (noiseTexture != null)
        {
            Destroy(noiseTexture);
        }
    }
}

/*
ІНСТРУКЦІЯ З НАЛАШТУВАННЯ:

1. Створіть порожній GameObject в сцені (назвіть його "MenuCameraSystem")
2. Додайте цей скрипт до нього
3. Створіть кілька камер в сцені для різних кутів огляду
4. Перетягніть всі камери в масив "Cameras" у інспекторі
5. Залиште поля "Static Canvas" та "Static Image" порожніми - вони створяться автоматично!
6. Налаштуйте параметри:
   - Switch Interval: час між перемиканнями камер (секунди)
   - Rotation Angle: максимальний кут повороту (градуси)
   - Rotation Speed: швидкість обертання
   - Static Duration: тривалість ефекту шипіння

Тепер ефект шипіння буде видно на весь екран!
*/