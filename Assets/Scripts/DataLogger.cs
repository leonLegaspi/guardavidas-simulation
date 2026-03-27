using UnityEngine;
using System.IO;
using System.Text;

/// <summary>
/// Exporta los resultados de cada run del experimento a un archivo CSV.
/// Guarda en la carpeta de n8n para integracion automatica con el pipeline.
/// Formato: run_id, seed, scenario, mode, success_rate, avg_rescue_time, rescued, deaths, duration
/// </summary>
public class DataLogger : MonoBehaviour
{
    [Header("Referencias")]
    public RescueManager rescueManager;
    public SimulationStats stats;

    [Header("Configuracion")]
    public ExperimentConfig experimentConfig;

    private string filePath;
    private bool headerWritten = false;

    void Awake()
    {
        // Guardar directamente en la carpeta de n8n
        string n8nFolder = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
            ".n8n-files"
        );

        Directory.CreateDirectory(n8nFolder);

        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        filePath = Path.Combine(n8nFolder, $"experiment_{timestamp}.csv");

        Debug.Log($"[DataLogger] CSV en: {filePath}");
    }

    /// <summary>
    /// Registra una run completa. Llamado por ExperimentRunner al terminar cada run.
    /// </summary>
    public void LogRun(int runId, int seed, string scenario, DecisionMode mode, float duration)
    {
        if (!headerWritten)
        {
            File.WriteAllText(filePath,
                "run_id,seed,scenario,mode,success_rate,avg_rescue_time,rescued,deaths,duration\n",
                Encoding.UTF8);
            headerWritten = true;
        }

        int rescued = stats.Rescued;
        int deaths = stats.Died;
        int total = rescued + deaths;
        float successRate = total > 0 ? (float)rescued / total * 100f : 0f;
        float avgTime = rescueManager.AverageRescueTime;

        string line = string.Format("{0},{1},{2},{3},{4:0.00},{5:0.00},{6},{7},{8:0.00}",
            runId, seed, scenario, mode,
            successRate, avgTime,
            rescued, deaths, duration
        );

        File.AppendAllText(filePath, line + "\n", Encoding.UTF8);

        Debug.Log($"[DataLogger] Run {runId} — {scenario} / {mode} — " +
                  $"Exito: {successRate:0.0}% | AvgTime: {avgTime:0.0}s | " +
                  $"Rescatados: {rescued} | Muertes: {deaths}");
    }

    public string GetFilePath() => filePath;
}