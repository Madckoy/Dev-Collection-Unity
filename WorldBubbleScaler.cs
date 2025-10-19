using UnityEngine;

/// <summary>
/// Масштабирует заранее созданный префаб сферы до нужного радиуса.
/// Используется для "пузыря мира", внутри которого видна текстура неба.
/// </summary>
public class WorldBubbleScaler : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Префаб или объект сферы, служащий фоном неба")]
    public GameObject bubblePrefab;

    [Header("Settings")]
    public bool createInstance = true;   // если true — клонируем префаб, иначе масштабируем существующий
    public bool autoCenter = true;       // центрировать по (0,0,0)
    public bool keepProportion = true;   // одинаковый масштаб по осям

    private GameObject bubbleInstance;

    /// <summary>
    /// Вызывается при загрузке мира.
    /// </summary>
    public void ApplyRadius(float radius)
    {
        if (bubblePrefab == null)
        {
            Debug.LogWarning("⚠️ WorldBubbleScaler: bubblePrefab не назначен!");
            return;
        }

        // уничтожаем старый экземпляр, если был
        if (bubbleInstance != null)
            Destroy(bubbleInstance);

        // создаем или используем существующий объект
        bubbleInstance = createInstance ? Instantiate(bubblePrefab) : bubblePrefab;
        bubbleInstance.name = "WorldBubble";

        if (autoCenter)
            bubbleInstance.transform.position = Vector3.zero;

        // радиус * 2 = диаметр, потому что scale — это полный размер
        float diameter = radius * 2f;
        Vector3 scale = keepProportion
            ? new Vector3(diameter, diameter, diameter)
            : new Vector3(radius * 2f, radius * 1.8f, radius * 2f); // на случай кастомных форм

        bubbleInstance.transform.localScale = scale;

        Debug.Log($"✨ Bubble scaled to radius={radius} (scale={scale})");
    }
}
