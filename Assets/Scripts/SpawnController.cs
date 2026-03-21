using UnityEngine;

/// <summary>
/// Reintroduce nadadores gradualmente cuando la simulacion se queda sin ellos.
/// Mantiene la simulacion corriendo indefinidamente.
/// </summary>
public class SpawnController : MonoBehaviour
{
    [Header("Referencias")]
    public RescueManager manager;

    [Header("Configuracion")]
    [Tooltip("Intervalo en segundos entre cada nuevo nadador")]
    public float spawnInterval = 5f;

    [Tooltip("Maximo de nadadores activos al mismo tiempo")]
    public int maxSwimmers = 20;

    private float timer;

    /// <summary>
    /// Total de nadadores spawneados desde el inicio (para la pantalla de resumen).
    /// </summary>
    public int TotalSpawned { get; private set; }

    void Start()
    {
        // Contar los nadadores del spawn inicial
        TotalSpawned = manager != null ? manager.ActiveSwimmerCount : 0;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0f;
            TrySpawn();
        }
    }

    void TrySpawn()
    {
        if (manager.ActiveSwimmerCount >= maxSwimmers) return;

        manager.SpawnOneSwimmer();
        TotalSpawned++;
    }
}