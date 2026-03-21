using UnityEngine;

public class SwimmerMovement : MonoBehaviour
{
    private float speed;
    private float wanderInterval;
    private float borderDetectDist;
    private float borderSteerForce;
    private float separationRadius;
    private float separationForce;

    private RiverArea river;
    private Vector3 direction;
    private float timer;
    private Collider[] nearbyBuffer = new Collider[16];

    public void Initialize(RiverArea r, SimulationConfig config, SwimmerTypeConfig typeCfg)
    {
        river = r;
        speed = typeCfg.speed;           // velocidad segun tipo
        wanderInterval = config.swimmerWanderInterval;
        borderDetectDist = config.borderDetectDist;
        borderSteerForce = config.borderSteerForce;
        separationRadius = config.separationRadius;
        separationForce = config.separationForce;

        enabled = true;
        PickDirection();
    }

    void Update()
    {
        if (river == null) return;

        timer += Time.deltaTime;
        if (timer > wanderInterval)
        {
            PickDirection();
            timer = 0;
        }

        ApplySeparation();
        ApplyBorderSteering();
        Move();
    }

    void Move()
    {
        direction.y = 0;
        direction.Normalize();

        transform.position += direction * speed * Time.deltaTime;

        Vector3 pos = transform.position;
        pos.y = river.WaterHeight;
        transform.position = pos;

        if (!river.IsInside(transform.position))
        {
            transform.position = river.ClampInsideWater(transform.position);
            direction = (river.transform.position - transform.position).normalized;
        }

        if (direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(direction),
                8f * Time.deltaTime
            );
    }

    void ApplyBorderSteering()
    {
        Vector3 center = river.transform.position;
        Vector3 half = river.size * 0.5f;
        Vector3 pos = transform.position;
        Vector3 steer = Vector3.zero;

        float dLeft = pos.x - (center.x - half.x);
        float dRight = (center.x + half.x) - pos.x;
        float dBack = pos.z - (center.z - half.z);
        float dFront = (center.z + half.z) - pos.z;

        if (dLeft < borderDetectDist) steer.x += (borderDetectDist - dLeft) / borderDetectDist;
        if (dRight < borderDetectDist) steer.x -= (borderDetectDist - dRight) / borderDetectDist;
        if (dBack < borderDetectDist) steer.z += (borderDetectDist - dBack) / borderDetectDist;
        if (dFront < borderDetectDist) steer.z -= (borderDetectDist - dFront) / borderDetectDist;

        direction += steer * borderSteerForce * Time.deltaTime;
    }

    void ApplySeparation()
    {
        int count = Physics.OverlapSphereNonAlloc(
            transform.position, separationRadius, nearbyBuffer);

        Vector3 steer = Vector3.zero;
        int neighbors = 0;

        for (int i = 0; i < count; i++)
        {
            GameObject other = nearbyBuffer[i].gameObject;
            if (other == gameObject) continue;
            if (!other.GetComponent<SwimmerMovement>()) continue;

            Vector3 away = transform.position - other.transform.position;
            away.y = 0;

            float dist = Mathf.Max(away.magnitude, 0.001f);
            steer += away.normalized / dist;
            neighbors++;
        }

        if (neighbors > 0)
            direction += steer * separationForce * Time.deltaTime;
    }

    void PickDirection()
    {
        Vector2 r = Random.insideUnitCircle.normalized;
        direction = new Vector3(r.x, 0, r.y);
    }
}