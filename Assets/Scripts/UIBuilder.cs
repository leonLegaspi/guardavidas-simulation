using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Crea y posiciona toda la UI automaticamente en Awake.
/// No requiere configuracion manual en el editor.
/// Adjuntar a cualquier GameObject vacio en la escena.
/// </summary>
public class UIBuilder : MonoBehaviour
{
    [Header("Referencias (se asignan automaticamente)")]
    public SimulationStats stats;
    public SpawnController spawnController;
    public DifficultyManager difficultyManager;

    // ── Colores ────────────────────────────────────────────────
    private readonly Color panelColor = new Color(0f, 0f, 0f, 0.55f);
    private readonly Color textColor = Color.white;
    private readonly Color buttonColor = new Color(0.2f, 0.6f, 1f, 1f);
    private readonly Color closeColor = new Color(0.8f, 0.15f, 0.15f, 1f);
    private readonly Color summaryColor = new Color(0f, 0f, 0f, 0.85f);

    // ── Referencias internas ───────────────────────────────────
    private Canvas canvas;
    private TextMeshProUGUI rescuedText, diedText, activeText, timeText;
    private TextMeshProUGUI difficultyText;
    private TextMeshProUGUI sumLevel;
    private Slider timeSlider;
    private TextMeshProUGUI timeLabel;
    private GameObject summaryPanel;
    private TextMeshProUGUI sumRescued, sumDied, sumTime, sumRate;

    private bool summaryShown = false;

    // ══════════════════════════════════════════════════════════
    // Construccion de UI
    // ══════════════════════════════════════════════════════════

    void Awake()
    {
        if (stats == null) stats = FindObjectOfType<SimulationStats>();
        if (spawnController == null) spawnController = FindObjectOfType<SpawnController>();
        if (difficultyManager == null) difficultyManager = FindObjectOfType<DifficultyManager>();

        BuildCanvas();
        BuildHUD();
        BuildDifficultyBadge();
        BuildTimeScaleSlider();
        BuildCloseButton();       // X siempre visible
        BuildSummaryPanel();

        if (difficultyManager != null)
            difficultyManager.difficultyText = difficultyText;
    }

    // ── Canvas ─────────────────────────────────────────────────

    void BuildCanvas()
    {
        GameObject go = new GameObject("UICanvas");
        canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        go.AddComponent<GraphicRaycaster>();
    }

    // ── HUD ────────────────────────────────────────────────────

    void BuildHUD()
    {
        // Panel superior izquierda
        GameObject topLeft = MakePanel("HUD_TopLeft", canvas.transform,
            new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(20, -20), new Vector2(220, 70));

        rescuedText = MakeText(topLeft.transform, "RescuedText", "Rescatados: 0", 20);
        diedText = MakeText(topLeft.transform, "DiedText", "Fallecidos: 0", 20);
        LayoutGroup(topLeft, 6);

        // Panel superior derecha — con margen para no chocar con el boton X
        GameObject topRight = MakePanel("HUD_TopRight", canvas.transform,
            new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(-80, -20), new Vector2(220, 70));

        timeText = MakeText(topRight.transform, "TimeText", "Tiempo: 00:00", 20);
        activeText = MakeText(topRight.transform, "ActiveText", "Rescates activos: 0", 18);
        LayoutGroup(topRight, 6);

        SetTextAlignment(timeText, TextAlignmentOptions.Right);
        SetTextAlignment(activeText, TextAlignmentOptions.Right);
    }

    // ── Badge de dificultad (centro superior) ──────────────────

    void BuildDifficultyBadge()
    {
        GameObject panel = MakePanel("HUD_Difficulty", canvas.transform,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0, -20), new Vector2(160, 45));

        difficultyText = MakeText(panel.transform, "DifficultyText", "Nivel: 1", 22);
        difficultyText.fontStyle = FontStyles.Bold;
        SetTextAlignment(difficultyText, TextAlignmentOptions.Center);

        LayoutGroup(panel, 0);
    }

    // ── Boton X siempre visible (esquina superior derecha) ─────

    void BuildCloseButton()
    {
        GameObject go = new GameObject("CloseButton");
        go.transform.SetParent(canvas.transform, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-20, -20);
        rt.sizeDelta = new Vector2(50, 50);

        Image img = go.AddComponent<Image>();
        img.color = closeColor;

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        // Efecto hover
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(1f, 0.3f, 0.3f, 1f);
        colors.pressedColor = new Color(0.6f, 0.1f, 0.1f, 1f);
        btn.colors = colors;

        // Texto X
        GameObject textGO = new GameObject("Label");
        textGO.transform.SetParent(go.transform, false);

        RectTransform trt = textGO.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "✕";
        tmp.fontSize = 26;
        tmp.color = Color.white;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;

        btn.onClick.AddListener(QuitGame);
    }

    // ── Slider de velocidad ────────────────────────────────────

    void BuildTimeScaleSlider()
    {
        GameObject panel = MakePanel("HUD_Speed", canvas.transform,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0, 40), new Vector2(280, 55));

        timeLabel = MakeText(panel.transform, "SpeedLabel", "Velocidad: 1.0x", 18);
        SetTextAlignment(timeLabel, TextAlignmentOptions.Center);

        GameObject sliderGO = new GameObject("TimeSlider");
        sliderGO.transform.SetParent(panel.transform, false);

        RectTransform sliderRect = sliderGO.AddComponent<RectTransform>();
        sliderRect.sizeDelta = new Vector2(240, 20);

        timeSlider = sliderGO.AddComponent<Slider>();
        timeSlider.minValue = 0.5f;
        timeSlider.maxValue = 5f;
        timeSlider.value = 1f;

        GameObject bg = MakeImage("Background", sliderGO.transform,
            new Color(0.3f, 0.3f, 0.3f, 1f), new Vector2(240, 8));
        timeSlider.targetGraphic = bg.GetComponent<Image>();

        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderGO.transform, false);
        RectTransform faRect = fillArea.AddComponent<RectTransform>();
        faRect.anchorMin = Vector2.zero;
        faRect.anchorMax = Vector2.one;
        faRect.sizeDelta = Vector2.zero;

        GameObject fill = MakeImage("Fill", fillArea.transform, buttonColor, new Vector2(0, 8));
        timeSlider.fillRect = fill.GetComponent<RectTransform>();

        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderGO.transform, false);
        RectTransform haRect = handleArea.AddComponent<RectTransform>();
        haRect.anchorMin = Vector2.zero;
        haRect.anchorMax = Vector2.one;
        haRect.sizeDelta = Vector2.zero;

        GameObject handle = MakeImage("Handle", handleArea.transform, Color.white, new Vector2(20, 20));
        timeSlider.handleRect = handle.GetComponent<RectTransform>();

        timeSlider.onValueChanged.AddListener(v =>
        {
            Time.timeScale = v;
            timeLabel.text = $"Velocidad: {v:0.0}x";
        });

        LayoutGroup(panel, 4);
    }

    // ── Panel de resumen ───────────────────────────────────────

    void BuildSummaryPanel()
    {
        summaryPanel = MakePanel("SummaryPanel", canvas.transform,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(420, 420));

        summaryPanel.GetComponent<Image>().color = summaryColor;

        TextMeshProUGUI title = MakeText(summaryPanel.transform, "Title", "RESUMEN FINAL", 28);
        title.fontStyle = FontStyles.Bold;
        SetTextAlignment(title, TextAlignmentOptions.Center);

        sumRescued = MakeText(summaryPanel.transform, "SumRescued", "Rescatados: 0", 22);
        sumDied = MakeText(summaryPanel.transform, "SumDied", "Fallecidos: 0", 22);
        sumTime = MakeText(summaryPanel.transform, "SumTime", "Tiempo total: 00:00", 22);
        sumRate = MakeText(summaryPanel.transform, "SumRate", "Tasa de supervivencia: 0%", 22);
        sumLevel = MakeText(summaryPanel.transform, "SumLevel", "Nivel alcanzado: 1", 22);

        SetTextAlignment(sumRescued, TextAlignmentOptions.Center);
        SetTextAlignment(sumDied, TextAlignmentOptions.Center);
        SetTextAlignment(sumTime, TextAlignmentOptions.Center);
        SetTextAlignment(sumRate, TextAlignmentOptions.Center);
        SetTextAlignment(sumLevel, TextAlignmentOptions.Center);

        // Fila de botones: Reiniciar + Cerrar
        GameObject buttonRow = new GameObject("ButtonRow");
        buttonRow.transform.SetParent(summaryPanel.transform, false);

        RectTransform rowRect = buttonRow.AddComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(380, 50);

        HorizontalLayoutGroup hLayout = buttonRow.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 20;
        hLayout.childAlignment = TextAnchor.MiddleCenter;
        hLayout.childControlWidth = false;
        hLayout.childControlHeight = false;
        hLayout.childForceExpandWidth = false;

        MakeButton(buttonRow.transform, "Reiniciar", "Reiniciar", buttonColor, () =>
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        });

        MakeButton(buttonRow.transform, "Cerrar", "Cerrar", closeColor, QuitGame);

        LayoutGroup(summaryPanel, 14, true);
        summaryPanel.SetActive(false);
    }

    // ══════════════════════════════════════════════════════════
    // Update
    // ══════════════════════════════════════════════════════════

    void Update()
    {
        UpdateHUD();
        CheckSimulationEnd();
    }

    void UpdateHUD()
    {
        if (stats == null) return;

        rescuedText.text = $"Rescatados: {stats.Rescued}";
        diedText.text = $"Fallecidos: {stats.Died}";
        activeText.text = $"Rescates activos: {stats.ActiveRescues}";
        timeText.text = $"Tiempo: {FormatTime(stats.ElapsedTime)}";
    }

    void CheckSimulationEnd()
    {
        if (summaryShown || spawnController == null || stats == null) return;

        if (spawnController.TotalSpawned > 0 &&
            stats.TotalResolved >= spawnController.TotalSpawned)
            ShowSummary();
    }

    void ShowSummary()
    {
        summaryShown = true;
        Time.timeScale = 0f;

        summaryPanel.SetActive(true);

        int total = stats.Rescued + stats.Died;
        float rate = total > 0 ? (float)stats.Rescued / total * 100f : 0f;
        int level = difficultyManager != null ? difficultyManager.CurrentLevel + 1 : 1;

        sumRescued.text = $"Rescatados: {stats.Rescued}";
        sumDied.text = $"Fallecidos: {stats.Died}";
        sumTime.text = $"Tiempo total: {FormatTime(stats.ElapsedTime)}";
        sumRate.text = $"Tasa de supervivencia: {rate:0.0}%";
        sumRate.color = rate >= 50f ? Color.green : Color.red;

        if (sumLevel != null)
            sumLevel.text = $"Nivel alcanzado: {level}";
    }

    // ── Cerrar juego ───────────────────────────────────────────

    void QuitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ══════════════════════════════════════════════════════════
    // Helpers
    // ══════════════════════════════════════════════════════════

    GameObject MakePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
                         Vector2 anchoredPos, Vector2 size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = anchorMin;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Image img = go.AddComponent<Image>();
        img.color = panelColor;

        return go;
    }

    TextMeshProUGUI MakeText(Transform parent, string name, string content, int fontSize)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(200, 30);

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.color = textColor;

        return tmp;
    }

    GameObject MakeImage(string name, Transform parent, Color color, Vector2 size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = size;

        Image img = go.AddComponent<Image>();
        img.color = color;

        return go;
    }

    Button MakeButton(Transform parent, string name, string label, Color color,
                      System.Action onClick)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(160, 45);

        Image img = go.AddComponent<Image>();
        img.color = color;

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(color.r + 0.1f, color.g + 0.1f, color.b + 0.1f, 1f);
        colors.pressedColor = new Color(color.r - 0.1f, color.g - 0.1f, color.b - 0.1f, 1f);
        btn.colors = colors;

        GameObject textGO = new GameObject("Label");
        textGO.transform.SetParent(go.transform, false);

        RectTransform trt = textGO.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 20;
        tmp.color = Color.white;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;

        btn.onClick.AddListener(() => onClick());

        return btn;
    }

    void LayoutGroup(GameObject go, int spacing, bool withPadding = false)
    {
        VerticalLayoutGroup layout = go.AddComponent<VerticalLayoutGroup>();
        layout.spacing = spacing;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        if (withPadding)
            layout.padding = new RectOffset(20, 20, 20, 20);
    }

    void SetTextAlignment(TextMeshProUGUI tmp, TextAlignmentOptions alignment)
    {
        tmp.alignment = alignment;
    }

    string FormatTime(float seconds)
    {
        int m = (int)(seconds / 60);
        int s = (int)(seconds % 60);
        return $"{m:00}:{s:00}";
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}