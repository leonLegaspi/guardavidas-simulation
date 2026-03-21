using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Orquesta el experimento A/B completo.
/// Durante el experimento: oculta el Canvas, deshabilita DifficultyManager
/// y toma control exclusivo del timeScale.
/// </summary>
public class ExperimentRunner : MonoBehaviour
{
    [Header("Referencias")]
    public RescueManager rescueManager;
    public SimulationStats stats;
    public DataLogger dataLogger;
    public SpawnController spawnController;

    [Header("Configuracion")]
    public ExperimentConfig experimentConfig;

    [Header("Control")]
    public bool autoStart = false;

    // ── Estado ─────────────────────────────────────────────────
    private bool experimentRunning = false;
    private int totalRuns = 0;
    private int completedRuns = 0;

    private DifficultyManager difficultyManager;
    private Canvas mainCanvas;

    void Start()
    {
        difficultyManager = FindObjectOfType<DifficultyManager>();

        // Buscar el Canvas principal (el que crea UIBuilder)
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (Canvas c in canvases)
            if (c.name == "UICanvas") { mainCanvas = c; break; }

        if (autoStart)
            StartExperiment();
    }

    [ContextMenu("Start Experiment")]
    public void StartExperiment()
    {
        if (experimentRunning)
        {
            Debug.LogWarning("[ExperimentRunner] Ya hay un experimento corriendo.");
            return;
        }

        StartCoroutine(RunAllExperiments());
    }

    IEnumerator RunAllExperiments()
    {
        experimentRunning = true;

        // Ocultar UI completamente y deshabilitar sistemas que interfieren
        if (mainCanvas != null) mainCanvas.gameObject.SetActive(false);
        if (difficultyManager != null) difficultyManager.enabled = false;

        // Asegurar timeScale limpio
        Time.timeScale = 1f;
        yield return null;

        DecisionMode[] modes = experimentConfig.runBothModes
            ? new[] { DecisionMode.Prioritized, DecisionMode.Nearest }
            : new[] { rescueManager.GetConfig().decisionMode };

        totalRuns = experimentConfig.scenarios.Length
                  * modes.Length
                  * experimentConfig.runsPerScenario;

        Debug.Log($"[ExperimentRunner] Iniciando — {totalRuns} runs totales");

        int runId = 0;

        foreach (ScenarioConfig scenario in experimentConfig.scenarios)
        {
            foreach (DecisionMode mode in modes)
            {
                for (int i = 0; i < experimentConfig.runsPerScenario; i++)
                {
                    int seed = experimentConfig.baseSeed + runId;
                    yield return StartCoroutine(RunSingle(runId, seed, scenario, mode));

                    runId++;
                    completedRuns++;
                    Debug.Log($"[ExperimentRunner] Progreso: {completedRuns}/{totalRuns}");
                    yield return null;
                }
            }
        }

        // Restaurar todo al terminar
        Time.timeScale = 1f;
        if (mainCanvas != null) mainCanvas.gameObject.SetActive(true);
        if (difficultyManager != null) difficultyManager.enabled = true;

        experimentRunning = false;
        Debug.Log($"[ExperimentRunner] COMPLETO. CSV en: {dataLogger.GetFilePath()}");
    }

    IEnumerator RunSingle(int runId, int seed, ScenarioConfig scenario, DecisionMode mode)
    {
        // 1. Seed controlado
        Random.InitState(seed);

        // 2. Configurar escenario
        SimulationConfig cfg = rescueManager.GetConfig();
        cfg.decisionMode = mode;
        cfg.swimmerCount = scenario.swimmerCount;

        if (spawnController != null)
        {
            spawnController.maxSwimmers = scenario.swimmerCount;
            spawnController.spawnInterval = scenario.spawnInterval;
        }

        // 3. Reset completo — swimmers, guardavidas, metricas, timer
        rescueManager.ResetSwimmers();
        rescueManager.ResetMetrics();
        stats.Reset();

        // 4. Fast-forward — el runner es el unico que toca timeScale
        Time.timeScale = experimentConfig.fastForwardScale;

        // 5. Correr durante maxRunDuration segundos de juego
        float elapsed = 0f;
        while (elapsed < experimentConfig.maxRunDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 6. Guardar resultado
        dataLogger.LogRun(runId, seed, scenario.name, mode, stats.ElapsedTime);

        // 7. Pausa breve entre runs
        Time.timeScale = 1f;
        yield return new WaitForSecondsRealtime(0.15f);
    }

    // ── Overlay del experimento ────────────────────────────────

    void OnGUI()
    {
        if (!experimentRunning) return;

        // Fondo semitransparente
        GUI.color = new Color(0, 0, 0, 0.7f);
        GUI.DrawTexture(new Rect(0, 0, 320, 80), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUI.skin.box.fontSize = 14;
        GUI.skin.box.alignment = TextAnchor.MiddleCenter;

        GUI.Box(new Rect(0, 0, 320, 80),
            $"Experimento en curso\n" +
            $"Run {completedRuns + 1}/{totalRuns}\n" +
            $"Velocidad: {experimentConfig.fastForwardScale}x");
    }
}