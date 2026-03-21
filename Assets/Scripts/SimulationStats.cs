using UnityEngine;

/// <summary>
/// Registra y expone las estadisticas de la simulacion en tiempo real.
/// Se suscribe a SimulationEvents y no depende de ningun otro sistema.
/// </summary>
public class SimulationStats : MonoBehaviour
{
    // ── Propiedades publicas ───────────────────────────────────

    public int Rescued { get; private set; }
    public int Died { get; private set; }
    public int ActiveRescues { get; private set; }
    public float ElapsedTime { get; private set; }

    public int TotalResolved => Rescued + Died;

    // ── Ciclo de vida ──────────────────────────────────────────

    void OnEnable()
    {
        SimulationEvents.OnSwimmerRescued += HandleRescued;
        SimulationEvents.OnSwimmerDied += HandleDied;
        SimulationEvents.OnRescueStarted += HandleRescueStarted;
        SimulationEvents.OnRescueCompleted += HandleRescueCompleted;
    }

    void OnDisable()
    {
        SimulationEvents.OnSwimmerRescued -= HandleRescued;
        SimulationEvents.OnSwimmerDied -= HandleDied;
        SimulationEvents.OnRescueStarted -= HandleRescueStarted;
        SimulationEvents.OnRescueCompleted -= HandleRescueCompleted;
    }

    void Update()
    {
        ElapsedTime += Time.deltaTime;
    }

    // ── Handlers ───────────────────────────────────────────────

    void HandleRescued(Swimmer _) => Rescued++;
    void HandleDied(Swimmer _) => Died++;
    void HandleRescueStarted(Lifeguard _, Swimmer __) => ActiveRescues++;
    void HandleRescueCompleted(Lifeguard _, Swimmer __) => ActiveRescues = Mathf.Max(0, ActiveRescues - 1);

    // ── Reset (para ExperimentRunner) ─────────────────────────

    /// <summary>
    /// Reinicia todas las estadisticas para una nueva run del experimento.
    /// </summary>
    public void Reset()
    {
        Rescued = 0;
        Died = 0;
        ActiveRescues = 0;
        ElapsedTime = 0f;
    }
}