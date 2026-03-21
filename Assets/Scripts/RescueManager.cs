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
    private List<Swimmer> rescueQueue = new();
    private ObjectPool<Swimmer> swimmerPool;

    // ── Dificultad ─────────────────────────────────────────────
    private float difficultyEnergyMultiplier = 1f;

    // ── Metricas ───────────────────────────────────────────────
    private float totalRescueTime = 0f;
    private int rescueTimeCount = 0;
    private Dictionary<Swimmer, float> rescueStartTimes = new();

    public int ActiveSwimmerCount => swimmers.Count;
    public float DifficultyEnergyMultiplier => difficultyEnergyMultiplier;
    public float AverageRescueTime => rescueTimeCount > 0 ? totalRescueTime / rescueTimeCount : 0f;

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

    // ── Reset completo de swimmers (para ExperimentRunner) ─────

    /// <summary>
    /// Devuelve todos los swimmers al pool y los respawnea desde cero.
    /// Garantiza condiciones iniciales limpias para cada run.
    /// </summary>
    public void ResetSwimmers()
    {
        // Devolver todos al pool
        for (int i = swimmers.Count - 1; i >= 0; i--)
        {
            Swimmer sw = swimmers[i];
            if (sw != null)
                swimmerPool.Return(sw);
        }

        swimmers.Clear();
        rescueQueue.Clear();
        rescueStartTimes.Clear();

        // Tambien resetear guardavidas a sus torres
        foreach (Lifeguard lg in lifeguards)
        {
            lg.ClearTarget();
            lg.transform.position = lg.GetTowerPosition();
        }

        // Respawnear cantidad correcta segun config actual
        SpawnSwimmers();
    }

    // ── Cola de rescates con A/B ───────────────────────────────

    public void RequestRescue(Swimmer swimmer)
    {
        if (!rescueStartTimes.ContainsKey(swimmer))
            rescueStartTimes[swimmer] = Time.time;

        if (TryAssignLifeguard(swimmer)) return;

        if (!rescueQueue.Contains(swimmer))
        {
            rescueQueue.Add(swimmer);
            Debug.Log($"[RescueManager] Nadador encolado. Cola: {rescueQueue.Count}");
        }
    }

    void ProcessRescueQueue()
    {
        if (rescueQueue.Count == 0) return;

        rescueQueue.RemoveAll(s => s == null || !s.IsDrowning());
        if (rescueQueue.Count == 0) return;

        // Modo A: Prioritized — ordena por tiempo de vida restante
        if (config.decisionMode == DecisionMode.Prioritized)
        {
            rescueQueue.Sort((a, b) =>
                a.GetDrowningTimeLeft().CompareTo(b.GetDrowningTimeLeft()));
        }
        // Modo B: Nearest — sin reordenar, TryAssign elige por distancia

        for (int i = rescueQueue.Count - 1; i >= 0; i--)
        {
            if (TryAssignLifeguard(rescueQueue[i]))
                rescueQueue.RemoveAt(i);
        }
    }

    bool TryAssignLifeguard(Swimmer swimmer)
    {
        Lifeguard best = null;
        float minDist = Mathf.Infinity;

        foreach (Lifeguard lg in lifeguards)
        {
            if (lg.IsBusy()) continue;

            float dist = Vector3.Distance(lg.transform.position, swimmer.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                best = lg;
            }
        }

        if (best == null) return false;

        best.AssignRescue(swimmer);
        return true;
    }

    public void RegisterRescueComplete(Swimmer swimmer)
    {
        if (rescueStartTimes.TryGetValue(swimmer, out float startTime))
        {
            totalRescueTime += Time.time - startTime;
            rescueTimeCount++;
            rescueStartTimes.Remove(swimmer);
        }
    }

    // ── Dificultad ─────────────────────────────────────────────

    public void ApplyDifficultyLevel(int level)
    {
        difficultyEnergyMultiplier = Mathf.Pow(config.difficultyEnergyMult, level);
        Debug.Log($"[RescueManager] Nivel {level} — multiplicador: {difficultyEnergyMultiplier:0.00}x");
    }

    // ── Reset de metricas ──────────────────────────────────────

    public void ResetMetrics()
    {
        totalRescueTime = 0f;
        rescueTimeCount = 0;
        rescueStartTimes.Clear();
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
            if (dist < minDist) { minDist = dist; best = sw; }
        }

        return best;
    }

    public void RemoveSwimmer(Swimmer swimmer)
    {
        swimmers.Remove(swimmer);
        rescueQueue.Remove(swimmer);
        rescueStartTimes.Remove(swimmer);
        swimmerPool.Return(swimmer);
    }

    public SimulationConfig GetConfig() => config;
}