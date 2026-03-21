using UnityEngine;

[RequireComponent(typeof(FSM))]
public class LifeguardStateMachine : MonoBehaviour
{
    public const string IDLE = "Idle";
    public const string GOING_TO_RESCUE = "GoingToRescue";
    public const string BRINGING_BACK = "BringingBack";
    public const string RETURNING = "Returning";

    private FSM fsm;
    private Lifeguard lifeguard;

    public void Initialize(Lifeguard lg)
    {
        lifeguard = lg;
        fsm = GetComponent<FSM>();

        fsm.CreateState(IDLE, new IdleState());
        fsm.CreateState(GOING_TO_RESCUE, new GoingToRescueState(lifeguard));
        fsm.CreateState(BRINGING_BACK, new BringingBackState(lifeguard));
        fsm.CreateState(RETURNING, new ReturningState(lifeguard));

        fsm.ChangeState(IDLE);
    }

    public void UpdateFSM() => fsm.UpdateFSM();
    public void ChangeState(string state) => fsm.ChangeState(state);

    // ══════════════════════════════════════════════════════════
    // Estados
    // ══════════════════════════════════════════════════════════

    class IdleState : IState
    {
        public void OnEnter() { }
        public void OnUpdate() { }
        public void OnExit() { }
    }

    // ──────────────────────────────────────────────────────────

    class GoingToRescueState : IState
    {
        readonly Lifeguard lifeguard;
        public GoingToRescueState(Lifeguard lg) { lifeguard = lg; }

        public void OnEnter() { }
        public void OnExit() { }

        public void OnUpdate()
        {
            Swimmer target = lifeguard.GetTarget();

            if (target == null)
            {
                lifeguard.GetComponent<LifeguardStateMachine>().ChangeState(RETURNING);
                return;
            }

            lifeguard.MoveTo(target.transform.position);

            if (lifeguard.IsNear(target.transform.position))
            {
                target.StartBeingRescued();
                lifeguard.GetComponent<LifeguardStateMachine>().ChangeState(BRINGING_BACK);
            }
        }
    }

    // ──────────────────────────────────────────────────────────

    class BringingBackState : IState
    {
        readonly Lifeguard lifeguard;
        public BringingBackState(Lifeguard lg) { lifeguard = lg; }

        public void OnEnter() { }
        public void OnExit() { }

        public void OnUpdate()
        {
            Swimmer target = lifeguard.GetTarget();

            if (target == null)
            {
                lifeguard.GetComponent<LifeguardStateMachine>().ChangeState(RETURNING);
                return;
            }

            target.transform.position =
                lifeguard.transform.position + lifeguard.transform.forward * 0.8f;

            lifeguard.MoveTo(lifeguard.GetTowerPosition());

            if (lifeguard.IsNear(lifeguard.GetTowerPosition()))
            {
                lifeguard.CompleteRescue(target);
                target.FinishRescue();
                lifeguard.ClearTarget();
                lifeguard.GetComponent<LifeguardStateMachine>().ChangeState(RETURNING);
            }
        }
    }

    // ──────────────────────────────────────────────────────────

    class ReturningState : IState
    {
        readonly Lifeguard lifeguard;
        public ReturningState(Lifeguard lg) { lifeguard = lg; }

        public void OnEnter() { }
        public void OnExit() { }

        public void OnUpdate()
        {
            lifeguard.MoveTo(lifeguard.GetTowerPosition());
            if (lifeguard.IsNear(lifeguard.GetTowerPosition()))
                lifeguard.GetComponent<LifeguardStateMachine>().ChangeState(IDLE);
        }
    }
}