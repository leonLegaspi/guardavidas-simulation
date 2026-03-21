using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Maneja la UI en tiempo real y la pantalla de resumen final.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Referencias")]
    public SimulationStats stats;
    public SpawnController spawnController;

    [Header("HUD - Textos")]
    public TextMeshProUGUI rescuedText;
    public TextMeshProUGUI diedText;
    public TextMeshProUGUI activeRescuesText;
    public TextMeshProUGUI elapsedTimeText;

    [Header("HUD - Velocidad")]
    public Slider timeScaleSlider;
    public TextMeshProUGUI timeScaleLabel;

    [Header("Pantalla de resumen")]
    public GameObject summaryPanel;
    public TextMeshProUGUI summaryRescuedText;
    public TextMeshProUGUI summaryDiedText;
    public TextMeshProUGUI summaryTimeText;
    public TextMeshProUGUI summarySurvivalRateText;
    public Button restartButton;

    private bool summaryShown = false;

    // ── Ciclo de vida ──────────────────────────────────────────

    void Start()
    {
        if (summaryPanel != null)
            summaryPanel.SetActive(false);

        if (timeScaleSlider != null)
        {
            timeScaleSlider.minValue = 0.5f;
            timeScaleSlider.maxValue = 5f;
            timeScaleSlider.value = 1f;
            timeScaleSlider.onValueChanged.AddListener(OnTimeScaleChanged);
        }

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartSimulation);
    }

    void Update()
    {
        UpdateHUD();
        CheckSimulationEnd();
    }

    // ── HUD ────────────────────────────────────────────────────

    void UpdateHUD()
    {
        if (stats == null) return;

        if (rescuedText != null) rescuedText.text = $"Rescatados: {stats.Rescued}";
        if (diedText != null) diedText.text = $"Fallecidos: {stats.Died}";
        if (activeRescuesText != null) activeRescuesText.text = $"Rescates activos: {stats.ActiveRescues}";
        if (elapsedTimeText != null) elapsedTimeText.text = $"Tiempo: {FormatTime(stats.ElapsedTime)}";
    }

    void OnTimeScaleChanged(float value)
    {
        Time.timeScale = value;

        if (timeScaleLabel != null)
            timeScaleLabel.text = $"Velocidad: {value:0.0}x";
    }

    // ── Resumen final ──────────────────────────────────────────

    void CheckSimulationEnd()
    {
        if (summaryShown) return;
        if (spawnController == null || stats == null) return;

        // Mostrar resumen cuando se resolvieron todos los nadadores iniciales
        int total = stats.TotalResolved;
        int spawned = spawnController.TotalSpawned;

        if (spawned > 0 && total >= spawned)
            ShowSummary();
    }

    void ShowSummary()
    {
        summaryShown = true;
        Time.timeScale = 0f;   // pausar la simulacion

        if (summaryPanel == null) return;
        summaryPanel.SetActive(true);

        int total = stats.Rescued + stats.Died;
        float survivalRate = total > 0 ? (float)stats.Rescued / total * 100f : 0f;

        if (summaryRescuedText != null) summaryRescuedText.text = $"Rescatados: {stats.Rescued}";
        if (summaryDiedText != null) summaryDiedText.text = $"Fallecidos: {stats.Died}";
        if (summaryTimeText != null) summaryTimeText.text = $"Tiempo total: {FormatTime(stats.ElapsedTime)}";
        if (summarySurvivalRateText != null) summarySurvivalRateText.text = $"Tasa de supervivencia: {survivalRate:0.0}%";
    }

    void RestartSimulation()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    // ── Utilidades ─────────────────────────────────────────────

    string FormatTime(float seconds)
    {
        int m = (int)(seconds / 60);
        int s = (int)(seconds % 60);
        return $"{m:00}:{s:00}";
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;   // siempre restaurar al salir
    }
}