using UnityEngine;

/// <summary>
/// Configuracion del experimento A/B.
/// Separado del SimulationConfig para no mezclar parametros de simulacion
/// con parametros del experimento.
/// Crear desde Assets > Create > Simulation > ExperimentConfig
/// </summary>
[CreateAssetMenu(fileName = "ExperimentConfig", menuName = "Simulation/ExperimentConfig")]
public class ExperimentConfig : ScriptableObject
{
    [Header("Control del experimento")]
    [Tooltip("Seed base. Cada run usa seed + runIndex para ser reproducible")]
    public int baseSeed = 42;

    [Tooltip("Cantidad de runs por escenario y modo")]
    public int runsPerScenario = 30;

    [Tooltip("Duracion maxima de cada run en segundos de simulacion")]
    public float maxRunDuration = 120f;

    [Tooltip("Multiplicador de velocidad durante el experimento")]
    public float fastForwardScale = 10f;

    [Header("Escenarios")]
    public ScenarioConfig[] scenarios = new ScenarioConfig[]
    {
        new ScenarioConfig { name = "Baja",  swimmerCount = 10, spawnInterval = 8f  },
        new ScenarioConfig { name = "Media", swimmerCount = 20, spawnInterval = 5f  },
        new ScenarioConfig { name = "Alta",  swimmerCount = 35, spawnInterval = 3f  },
    };

    [Header("Modos de decision")]
    [Tooltip("Si true, corre ambos modos. Si false, solo el modo activo en SimulationConfig")]
    public bool runBothModes = true;

    [Header("Output")]
    [Tooltip("Carpeta donde se guardan los CSV (relativa a Application.persistentDataPath)")]
    public string outputFolder = "ExperimentResults";
}

[System.Serializable]
public class ScenarioConfig
{
    public string name;
    public int swimmerCount;
    public float spawnInterval;
}