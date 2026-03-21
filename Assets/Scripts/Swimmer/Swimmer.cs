using UnityEngine;

[RequireComponent(typeof(SwimmerStateMachine))]
public class Swimmer : MonoBehaviour
{
    [HideInInspector] public float energy;
    [HideInInspector] public SwimmerType swimmerType;

    private RescueManager manager;
    private SwimmerStateMachine stateMachine;
    private Renderer rend;
    private bool blinkActive;
    private Color baseColor;   // color base segun tipo

    // ── Inicializacion ─────────────────────────────────────────

    public void Initialize(RescueManager mgr, SimulationConfig config)
    {
        manager = mgr;
        swimmerType = config.GetRandomType();
        energy = config.swimmerInitialEnergy;

        SwimmerTypeConfig typeCfg = config.GetConfig(swimmerType);
        baseColor = typeCfg.color;

        stateMachine = GetComponent<SwimmerStateMachine>();
        stateMachine.Initialize(this, config, typeCfg);

        rend = GetComponentInChildren<Renderer>();

        // Resetear visual al reutilizar desde pool
        SetBlinkActive(false);
        SetColor(baseColor);

        // Asegurarse que SwimmerOnLand este desactivado al iniciar
        SwimmerOnLand onLand = GetComponent<SwimmerOnLand>();
        if (onLand != null) onLand.enabled = false;

        // Activar movimiento en agua
        SwimmerMovement move = GetComponent<SwimmerMovement>();
        if (move != null) move.Initialize(mgr.river, config, typeCfg);
    }

    void Update()
    {
        stateMachine.UpdateFSM();
        if (blinkActive) Blink();
    }

    // ── API publica ────────────────────────────────────────────

    public bool IsDrowning() => stateMachine.IsDrowning();
    public float GetDrowningTimeLeft() => stateMachine.GetDrowningTimeLeft();

    public void StartBeingRescued() => stateMachine.ChangeState(SwimmerStateMachine.BEING_RESCUED);

    public void FinishRescue()
    {
        stateMachine.ChangeState(SwimmerStateMachine.RESTING);

        SwimmerMovement move = GetComponent<SwimmerMovement>();
        if (move != null) move.enabled = false;

        SwimmerOnLand onLand = GetComponent<SwimmerOnLand>();
        if (onLand == null)
            gameObject.AddComponent<SwimmerOnLand>();
        else
            onLand.enabled = true;

        SimulationEvents.SwimmerRescued(this);
    }

    public void Died()
    {
        SimulationEvents.SwimmerDied(this);
        manager.RemoveSwimmer(this);
    }

    // ── Visual ─────────────────────────────────────────────────

    public void SetBlinkActive(bool active)
    {
        blinkActive = active;
        if (!active && rend != null)
            rend.material.color = baseColor;
    }

    public void SetColor(Color color)
    {
        if (rend != null)
            rend.material.color = color;
    }

    public void RestoreBaseColor() => SetColor(baseColor);

    void Blink()
    {
        if (rend == null) return;
        float t = Mathf.PingPong(Time.time * 5f, 1f);
        rend.material.color = Color.Lerp(baseColor, Color.red, t);
    }

    // ── Getters ────────────────────────────────────────────────

    public RescueManager GetManager() => manager;
}