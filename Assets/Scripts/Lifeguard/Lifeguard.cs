using UnityEngine;

[RequireComponent(typeof(LifeguardStateMachine))]
public class Lifeguard : MonoBehaviour
{
    private float speed;
    private float rescueDistance;

    private Swimmer targetSwimmer;
    private Vector3 towerPos;
    private RescueManager manager;
    private LifeguardStateMachine stateMachine;
    private RiverArea river;

    // ── Inicializacion ─────────────────────────────────────────

    public void Initialize(RescueManager mgr, Vector3 tower, SimulationConfig config)
    {
        manager = mgr;
        towerPos = tower;
        river = mgr.river;
        speed = config.lifeguardSpeed;
        rescueDistance = config.lifeguardRescueDistance;

        stateMachine = GetComponent<LifeguardStateMachine>();
        stateMachine.Initialize(this);
    }

    void Update()
    {
        stateMachine.UpdateFSM();
    }

    // ── API publica ────────────────────────────────────────────

    public bool IsBusy() => targetSwimmer != null;

    public void AssignRescue(Swimmer swimmer)
    {
        targetSwimmer = swimmer;
        stateMachine.ChangeState(LifeguardStateMachine.GOING_TO_RESCUE);
        SimulationEvents.RescueStarted(this, swimmer);
    }

    public void CompleteRescue(Swimmer swimmer)
    {
        SimulationEvents.RescueCompleted(this, swimmer);
    }

    public Swimmer GetTarget() => targetSwimmer;
    public void ClearTarget() => targetSwimmer = null;

    public void MoveTo(Vector3 pos)
    {
        Vector3 target = pos;
        target.y = transform.position.y;

        transform.position = Vector3.MoveTowards(
            transform.position, target, speed * Time.deltaTime);

        Vector3 dir = target - transform.position;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(dir),
                10f * Time.deltaTime
            );
    }

    public bool IsNear(Vector3 pos)
    {
        Vector3 a = transform.position; a.y = 0;
        Vector3 b = pos; b.y = 0;
        return Vector3.Distance(a, b) < rescueDistance;
    }

    public Vector3 GetTowerPosition() => towerPos;
    public bool IsInWater() => river != null && river.IsInside(transform.position);
    public RescueManager GetManager() => manager;

    // ── Gizmos ─────────────────────────────────────────────────

    void OnDrawGizmos()
    {
        // Radio de rescate en verde
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, rescueDistance);

        // Linea hacia el target en rojo si esta ocupado
        if (targetSwimmer != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, targetSwimmer.transform.position);
            Gizmos.DrawSphere(targetSwimmer.transform.position, 0.3f);
        }

        // Linea hacia la torre en gris
        Gizmos.color = new Color(1f, 1f, 1f, 0.3f);
        Gizmos.DrawLine(transform.position, towerPos);
    }
}