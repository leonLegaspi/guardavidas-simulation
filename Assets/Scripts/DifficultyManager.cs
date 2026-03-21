using UnityEngine;
using TMPro;

/// <summary>
/// Aumenta la dificultad de la simulacion progresivamente.
/// Cada intervalo sube el nivel: mas nadadores y energia que se pierde mas rapido.
/// </summary>
public class DifficultyManager : MonoBehaviour
{
    [Header("Referencias")]
    public RescueManager rescueManager;
    public SpawnController spawnController;

    [Header("UI (opcional)")]
    public TextMeshProUGUI difficultyText;

    private SimulationConfig config;
    private int currentLevel = 0;
    private float timer = 0f;

    void Start()
    {
        config = rescueManager.GetConfig();
        UpdateUI();
    }

    void Update()
    {
        if (currentLevel >= config.maxDifficultyLevel) return;

        timer += Time.deltaTime;

        if (timer >= config.difficultyInterval)
        {
            timer = 0f;
            LevelUp();
        }
    }

    void LevelUp()
    {
        currentLevel++;

        // Aumentar dificultad en RescueManager
        rescueManager.ApplyDifficultyLevel(currentLevel);

        // Spawnear nadadores extra
        int extra = config.difficultySwimmerStep * currentLevel;
        for (int i = 0; i < extra; i++)
            rescueManager.SpawnOneSwimmer();

        // Aumentar maximo del SpawnController
        if (spawnController != null)
            spawnController.maxSwimmers += config.difficultySwimmerStep;

        Debug.Log($"[DifficultyManager] Nivel {currentLevel} — +{extra} nadadores");

        UpdateUI();
        SimulationEvents.DifficultyLevelUp(currentLevel);
    }

    void UpdateUI()
    {
        if (difficultyText != null)
            difficultyText.text = $"Nivel: {currentLevel + 1}";
    }

    public int CurrentLevel => currentLevel;
}