using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;

#region JSON DTOs

[Serializable]
public class WorldItem
{
    public string worldId;
    public string name;
}

[Serializable]
public class WorldResponse
{
    public List<WorldItem> items;
}

[Serializable]
public class WorldDetail
{
    public string worldId;
    public string name;
    public long seed;
    public int radius;
    public string version;
    public string createdAt;
    public bool dirty;
}

#endregion

public class ServerListLoader : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Dropdown dropdown;
    public Button connectButton;

    [Header("Server API")]
    [Tooltip("Base URL of PetriServer REST API, e.g. http://localhost:8081/api")]
    public string baseUrl = "http://192.168.3.66:8081/api";

    private List<WorldItem> worlds = new();
    private WorldBubbleScaler WorldBubbleScaler;

    void Start()
    {
        // Ищем компонент, отвечающий за отрисовку границы
        WorldBubbleScaler = FindFirstObjectByType<WorldBubbleScaler>();
        if (WorldBubbleScaler == null)
            Debug.LogWarning("⚠️ WorldBorderDrawer not found in scene.");

        // Загружаем список миров при старте
        StartCoroutine(LoadWorldList());
    }

    // ───────────────────────────────
    // 🔹 Загрузка списка миров
    // ───────────────────────────────
    private IEnumerator LoadWorldList()
    {
        string url = $"{baseUrl}/worlds";
        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            Debug.Log($"🌍 Запрос списка миров: {url}");
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"❌ Ошибка загрузки списка миров: {req.error}");
                yield break;
            }

            // Парсим JSON ответа
            var response = JsonUtility.FromJson<WorldResponse>(req.downloadHandler.text);
            if (response == null || response.items == null)
            {
                Debug.LogError("❌ Некорректный ответ сервера при загрузке списка миров.");
                yield break;
            }

            worlds = response.items;
            PopulateDropdown();
            Debug.Log($"✅ Получено миров: {worlds.Count}");
        }
    }

    // ───────────────────────────────
    // 🔹 Заполняем выпадающий список
    // ───────────────────────────────
    private void PopulateDropdown()
    {
        dropdown.ClearOptions();
        List<string> options = new();

        foreach (var w in worlds)
            options.Add($"{w.name} ({w.worldId})");

        dropdown.AddOptions(options);

        connectButton.onClick.RemoveAllListeners();
        connectButton.onClick.AddListener(OnConnectClicked);
    }

    // ───────────────────────────────
    // 🔹 При нажатии "Подключиться"
    // ───────────────────────────────
    private void OnConnectClicked()
    {
        if (worlds.Count == 0)
        {
            Debug.LogWarning("⚠️ Список миров пуст, нечего подключать.");
            return;
        }

        int index = dropdown.value;
        var selected = worlds[index];
        Debug.Log($"🔗 Подключаемся к миру: {selected.name} ({selected.worldId})");
        StartCoroutine(LoadWorldDetail(selected.worldId));
    }

    // ───────────────────────────────
    // 🔹 Загрузка информации о мире
    // ───────────────────────────────
    private IEnumerator LoadWorldDetail(string worldId)
    {
        string url = $"{baseUrl}/world/{worldId}";
        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            Debug.Log($"🌍 Запрос данных мира: {url}");
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"❌ Ошибка загрузки мира: {req.error}");
                yield break;
            }

            // Парсим детали мира
            var detail = JsonUtility.FromJson<WorldDetail>(req.downloadHandler.text);
            if (detail == null)
            {
                Debug.LogError("❌ Некорректный ответ сервера при загрузке мира.");
                yield break;
            }

            Debug.Log($"✅ Получен мир: {detail.name} | radius={detail.radius} | version={detail.version}");

            // Вызов отрисовки рамки
            if (WorldBubbleScaler != null)
            {
                WorldBubbleScaler.ApplyRadius(detail.radius);
            }
            else
            {
                Debug.LogWarning("⚠️ WorldBubbleScaler не найден в сцене — невозможно построить границу мира.");
            }
        }
    }
}
