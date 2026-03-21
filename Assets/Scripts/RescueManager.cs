using UnityEngine;
using System.Collections.Generic;

public class RescueManager : MonoBehaviour
{
    [Header("Prefabs")]
    public Lifeguard lifeguardPrefab;
    public Swimmer swimmerPrefab;

    [Header("Parents")]
    public Transform lifeguardsParent;
    public Transform swimmersParent;

    [Header("Environment")]
    public RiverArea river;

    [Header("Torres")]
    public List<Tower> towers = new();

    [Header("Configuracion")]
    public SimulationConfig config;

    // ── Listas y pool ──────────────────────────────────────────
    private List<Lifeguard> lifeguards = new();
    private List<Swimmer> swimmers = new();

    /// <summary>
    /// Cola de rescates ordenada por urgencia (menos tiempo de vida = mas urgente).
    /// </summary>
    private List<Swimmer> rescueQueue = new();

    private ObjectPool<Swimmer> swimmerPool;

    // ── Dificultad ─────────────────────────────────────────────
    private float difficultyEnergyMultiplier = 1f;

    public int ActiveSwimmerCount => swimmers.Count;
    public float DifficultyEnergyMultiplier => difficultyEnergyMultiplier;

    // ── Ciclo de vida ──────────────────────────────────────────

    void Start()
    {
        swimmerPool = new ObjectPool<Swimmer>(
            swimmerPrefab,
            swimmersParent,
            config.swimmerCount
        );

        SpawnLifeguards();
        SpawnSwimmers();
    }

    void Update()
    {
        ProcessRescueQueue();
    }

    void OnDestroy()
    {
        SimulationEvents.ClearAll();
    }

    // ── Spawn ──────────────────────────────────────────────────

    void SpawnLifeguards()
    {
        foreach (Tower tower in towers)
        {
            foreach (Transform spawnPoint in tower.spawnPoints)
            {
                Lifeguard lg = Instantiate(
                    lifeguardPrefab,
                    spawnPoint.position,
                    Quaternion.identity,
                    lifeguardsParent
                );

                lg.Initialize(this, spawnPoint.position, config);
                lifeguards.Add(lg);
            }
        }
    }

    void SpawnSwimmers()
    {
        for (int i = 0; i < config.swimmerCount; i++)
            SpawnOneSwimmer();
    }

    public void SpawnOneSwimmer()
    {
        Vector3 pos = river.GetRandomPoint();
        Swimmer sw = swimmerPool.Get(pos, Quaternion.identity);
        sw.Initialize(this, config);
        swimmers.Add(sw);
    }

    // ── Cola de rescates con prioridad ─────────────────────────

    /// <summary>
    /// Llamado por un nadador al ahogarse.
    /// Intenta asignar un guardavidas libre; si no hay, encola por urgencia.
    /// </summary>
    public void RequestRescue(Swimmer swimmer)
    {
        if (TryAssignLifeguard(swimmer)) return;

        if (!rescueQueue.Contains(swimmer))
        {
            rescueQueue.Add(swimmer);
            Debug.Log($"[RescueManager] Nadador encolado. Cola: {rescueQueue.Count}");
        }
    }

    /// <summary>
    /// Cada frame: ordena la cola por urgencia y asigna guardavidas disponibles.
    /// </summary>
    void ProcessRescueQueue()
    {
        if (rescueQueue.Count == 0) return;

        // Limpiar nadadores que ya no necesitan rescate
        rescueQueue.RemoveAll(s => s == null || !s.IsDrowning());

        if (rescueQueue.Count == 0) return;

        // Ordenar por tiempo de vida restante (mas urgente primero)
        rescueQueue.Sort((a, b) =>
            a.GetDrowningTimeLeft().CompareTo(b.GetDrowningTimeLeft()));

        // Asignar guardavidas disponibles
        for (int i = rescueQueue.Count - 1; i >= 0; i--)
        {
            if (TryAssignLifeguard(rescueQueue[i]))
                rescueQueue.RemoveAt(i);
        }
    }

    /// <summary>
    /// Asigna el guardavidas libre MAS CERCANO al nadador.
    /// </summary>
    bool TryAssignLifeguard(Swimmer swimmer)
    {
        Lifeguard closest = null;
        float minDist = Mathf.Infinity;

        foreach (Lifeguard lg in lifeguards)
        {
            if (lg.IsBusy()) continue;

            float dist = Vector3.Distance(lg.transform.position, swimmer.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = lg;
            }
        }

        if (closest == null) return false;

        closest.AssignRescue(swimmer);
        return true;
    }

    // ── Dificultad ─────────────────────────────────────────────

    /// <summary>
    /// Llamado por DifficultyManager para aumentar la dificultad.
    /// </summary>
    public void ApplyDifficultyLevel(int level)
    {
        difficultyEnergyMultiplier = Mathf.Pow(config.difficultyEnergyMult, level);
        Debug.Log($"[RescueManager] Dificultad nivel {level} — multiplicador energia: {difficultyEnergyMultiplier:0.00}x");
    }

    // ── Utilidades ─────────────────────────────────────────────

    public Swimmer GetClosestDrowning(Vector3 pos)
    {
        Swimmer best = null;
        float minDist = Mathf.Infinity;

        foreach (Swimmer sw in swimmers)
        {
            if (!sw.IsDrowning()) continue;

            float dist = Vector3.Distance(pos, sw.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                best = sw;
            }
        }

        return best;
    }

    public void RemoveSwimmer(Swimmer swimmer)
    {
        swimmers.Remove(swimmer);
        rescueQueue.Remove(swimmer);
        swimmerPool.Return(swimmer);
    }

    public SimulationConfig GetConfig() => config;
}