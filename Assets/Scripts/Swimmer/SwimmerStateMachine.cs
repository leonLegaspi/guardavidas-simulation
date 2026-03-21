using UnityEngine;

[RequireComponent(typeof(FSM))]
public class SwimmerStateMachine : MonoBehaviour
{
    public const string SWIMMING = "Swimming";
    public const string EXHAUSTED = "Exhausted";
    public const string DROWNING = "Drowning";
    public const string BEING_RESCUED = "BeingRescued";
    public const string RESTING = "Resting";

    private FSM fsm;
    private Swimmer swimmer;

    // Referencia al estado Drowning para consultar tiempo restante
    private DrowningState drowningState;

    public void Initialize(Swimmer sw, SimulationConfig config, SwimmerTypeConfig typeCfg)
    {
        swimmer = sw;
        fsm = GetComponent<FSM>();

        float energyLoss = Random.Range(typeCfg.energyLossMin, typeCfg.energyLossMax);

        drowningState = new DrowningState(swimmer, typeCfg.drowningTime);

        fsm.CreateState(SWIMMING, new SwimmingState(swimmer, energyLoss, typeCfg));
        fsm.CreateState(EXHAUSTED, new ExhaustedState(swimmer, energyLoss));
        fsm.CreateState(DROWNING, drowningState);
        fsm.CreateState(BEING_RESCUED, new BeingRescuedState());
        fsm.CreateState(RESTING, new RestingState(swimmer, config));

        fsm.ChangeState(SWIMMING);
    }

    public void UpdateFSM() => fsm.UpdateFSM();
    public void ChangeState(string state) => fsm.ChangeState(state);
    public bool IsDrowning() => fsm.GetCurrentState() == DROWNING;

    /// <summary>
    /// Tiempo restante antes de morir. Usado por RescueManager para priorizar rescates.
    /// </summary>
    public float GetDrowningTimeLeft() => drowningState?.TimeLeft ?? float.MaxValue;

    // ══════════════════════════════════════════════════════════
    // Estados
    // ══════════════════════════════════════════════════════════

    class SwimmingState : IState
    {
        readonly Swimmer swimmer;
        readonly float energyLoss;
        readonly float exhaustThreshold;

        public SwimmingState(Swimmer sw, float loss, SwimmerTypeConfig cfg)
        {
            swimmer = sw;
            energyLoss = loss;
            exhaustThreshold = cfg.exhaustThreshold;
        }

        public void OnEnter() { }
        public void OnExit() { }

        public void OnUpdate()
        {
            swimmer.energy -= energyLoss * Time.deltaTime;
            if (swimmer.energy < exhaustThreshold)
                swimmer.GetComponent<SwimmerStateMachine>().ChangeState(EXHAUSTED);
        }
    }

    // ──────────────────────────────────────────────────────────

    class ExhaustedState : IState
    {
        readonly Swimmer swimmer;
        readonly float energyLoss;

        public ExhaustedState(Swimmer sw, float loss) { swimmer = sw; energyLoss = loss; }

        public void OnEnter() => swimmer.SetColor(Color.yellow);
        public void OnExit() => swimmer.RestoreBaseColor();

        public void OnUpdate()
        {
            swimmer.energy -= energyLoss * Time.deltaTime;
            if (swimmer.energy <= 0)
                swimmer.GetComponent<SwimmerStateMachine>().ChangeState(DROWNING);
        }
    }

    // ──────────────────────────────────────────────────────────

    class DrowningState : IState
    {
        readonly Swimmer swimmer;
        readonly float drowningTime;
        float timer;

        public float TimeLeft => timer;

        public DrowningState(Swimmer sw, float time) { swimmer = sw; drowningTime = time; }

        public void OnEnter()
        {
            timer = drowningTime;
            swimmer.SetBlinkActive(true);
            swimmer.GetComponent<SwimmerMovement>().enabled = false;
            swimmer.GetManager().RequestRescue(swimmer);
            SimulationEvents.SwimmerDrowning(swimmer);
        }

        public void OnUpdate()
        {
            timer -= Time.deltaTime;
            if (timer <= 0) swimmer.Died();
        }

        public void OnExit() => swimmer.SetBlinkActive(false);
    }

    // ──────────────────────────────────────────────────────────

    class BeingRescuedState : IState
    {
        public void OnEnter() { }
        public void OnUpdate() { }
        public void OnExit() { }
    }

    // ──────────────────────────────────────────────────────────

    class RestingState : IState
    {
        readonly Swimmer swimmer;
        readonly float initialEnergy;

        public RestingState(Swimmer sw, SimulationConfig cfg)
        {
            swimmer = sw;
            initialEnergy = cfg.swimmerInitialEnergy;
        }

        public void OnEnter()
        {
            swimmer.energy = initialEnergy;
            swimmer.RestoreBaseColor();
        }

        public void OnUpdate() { }
        public void OnExit() { }
    }
}