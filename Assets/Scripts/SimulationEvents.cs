using System;

/// <summary>
/// Bus de eventos estatico de la simulacion.
/// Cualquier sistema puede suscribirse sin conocer al emisor.
/// Patron Observer desacoplado.
/// </summary>
public static class SimulationEvents
{
    public static event Action<Swimmer> OnSwimmerDrowning;
    public static event Action<Swimmer> OnSwimmerRescued;
    public static event Action<Swimmer> OnSwimmerDied;

    public static event Action<Lifeguard, Swimmer> OnRescueStarted;
    public static event Action<Lifeguard, Swimmer> OnRescueCompleted;

    public static event Action<int> OnDifficultyLevelUp;

    // ── Disparadores ───────────────────────────────────────────

    public static void SwimmerDrowning(Swimmer swimmer) => OnSwimmerDrowning?.Invoke(swimmer);
    public static void SwimmerRescued(Swimmer swimmer) => OnSwimmerRescued?.Invoke(swimmer);
    public static void SwimmerDied(Swimmer swimmer) => OnSwimmerDied?.Invoke(swimmer);
    public static void RescueStarted(Lifeguard lg, Swimmer sw) => OnRescueStarted?.Invoke(lg, sw);
    public static void RescueCompleted(Lifeguard lg, Swimmer sw) => OnRescueCompleted?.Invoke(lg, sw);
    public static void DifficultyLevelUp(int level) => OnDifficultyLevelUp?.Invoke(level);

    /// <summary>
    /// Limpia todas las suscripciones. Llamar al destruir la escena.
    /// </summary>
    public static void ClearAll()
    {
        OnSwimmerDrowning = null;
        OnSwimmerRescued = null;
        OnSwimmerDied = null;
        OnRescueStarted = null;
        OnRescueCompleted = null;
        OnDifficultyLevelUp = null;
    }
}