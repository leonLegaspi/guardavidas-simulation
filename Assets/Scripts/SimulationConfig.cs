using UnityEngine;
using System;

/// <summary>
/// Configuracion central de la simulacion.
/// Crear desde Assets > Create > Simulation > Config
/// </summary>
[CreateAssetMenu(fileName = "SimulationConfig", menuName = "Simulation/Config")]
public class SimulationConfig : ScriptableObject
{
    [Header("Swimmers")]
    [Tooltip("Cantidad de nadadores al iniciar")]
    public int swimmerCount = 20;

    [Tooltip("Configuracion por tipo de nadador")]
    public SwimmerTypeConfig[] swimmerTypes = new SwimmerTypeConfig[]
    {
        new SwimmerTypeConfig
        {
            type             = SwimmerType.Child,
            label            = "Nino",
            spawnWeight      = 2,
            speed            = 1.5f,
            energyLossMin    = 4f,
            energyLossMax    = 7f,
            exhaustThreshold = 60f,
            drowningTime     = 8f,
            color            = new Color(1f, 0.6f, 0.2f)
        },
        new SwimmerTypeConfig
        {
            type             = SwimmerType.Adult,
            label            = "Adulto",
            spawnWeight      = 5,
            speed            = 2f,
            energyLossMin    = 2f,
            energyLossMax    = 5f,
            exhaustThreshold = 50f,
            drowningTime     = 12f,
            color            = Color.white
        },
        new SwimmerTypeConfig
        {
            type             = SwimmerType.Athlete,
            label            = "Atleta",
            spawnWeight      = 3,
            speed            = 3f,
            energyLossMin    = 0.8f,
            energyLossMax    = 2f,
            exhaustThreshold = 30f,
            drowningTime     = 18f,
            color            = new Color(0.4f, 0.9f, 0.4f)
        }
    };

    [Header("Swimmers - Movimiento")]
    public float swimmerWanderInterval = 3f;
    public float swimmerInitialEnergy = 100f;
    public float borderDetectDist = 2.5f;
    public float borderSteerForce = 4f;
    public float separationRadius = 1.2f;
    public float separationForce = 3f;

    [Header("Swimmers - En tierra")]
    public float landWalkSpeed = 1.5f;
    public float landDirectionChangeTime = 4f;
    public float landWaterAvoidDistance = 1.5f;

    [Header("Lifeguards")]
    public float lifeguardSpeed = 6f;
    public float lifeguardRescueDistance = 1.5f;

    [Header("Dificultad progresiva")]
    public float difficultyInterval = 30f;
    public int difficultySwimmerStep = 3;
    public float difficultyEnergyMult = 1.15f;
    public int maxDifficultyLevel = 5;

    // ── Modo de decision A/B ───────────────────────────────────

    [Header("Modo de decision (A/B Testing)")]
    [Tooltip("Prioritized = por urgencia (tiempo de vida restante)\nNearest = por distancia solamente")]
    public DecisionMode decisionMode = DecisionMode.Prioritized;

    // ── Utilidades ─────────────────────────────────────────────

    public SwimmerTypeConfig GetConfig(SwimmerType type)
    {
        foreach (var cfg in swimmerTypes)
            if (cfg.type == type) return cfg;
        return swimmerTypes[1];
    }

    public SwimmerType GetRandomType()
    {
        int total = 0;
        foreach (var cfg in swimmerTypes)
            total += cfg.spawnWeight;

        int roll = UnityEngine.Random.Range(0, total);
        int acc = 0;

        foreach (var cfg in swimmerTypes)
        {
            acc += cfg.spawnWeight;
            if (roll < acc) return cfg.type;
        }

        return SwimmerType.Adult;
    }
}

// ── Enums ──────────────────────────────────────────────────────

public enum SwimmerType { Child, Adult, Athlete }

/// <summary>
/// Modo A: Prioritized — rescata al nadador con menos tiempo de vida.
/// Modo B: Nearest     — rescata al nadador mas cercano (baseline).
/// </summary>
public enum DecisionMode { Prioritized, Nearest }

[Serializable]
public class SwimmerTypeConfig
{
    public SwimmerType type;
    public string label;

    [Tooltip("Peso de aparicion relativo")]
    public int spawnWeight;
    public float speed;
    public float energyLossMin;
    public float energyLossMax;
    public float exhaustThreshold;
    public float drowningTime;
    public Color color;
}