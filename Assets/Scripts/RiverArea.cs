using UnityEngine;

public class RiverArea : Area
{
    public float WaterHeight => transform.position.y;

    protected override void Awake()
    {
        base.Awake();
    }

    public new Vector3 GetRandomPoint()
    {
        Vector3 point = base.GetRandomPoint();
        point.y = WaterHeight;
        return point;
    }

    public Vector3 ClampInsideWater(Vector3 pos)
    {
        Vector3 clamped = ClampInside(pos);
        clamped.y = WaterHeight;
        return clamped;
    }

    protected override void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 0.4f, 1, 0.35f);
        Gizmos.DrawCube(transform.position, size);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, size);

        Vector3 inner = size - new Vector3(margin * 2, 0, margin * 2);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, inner);
    }
}