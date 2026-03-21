using UnityEngine;

public class BeachArea : Area
{
    protected override void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.8f, 0.3f, 0.35f);
        Gizmos.DrawCube(transform.position, size);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, size);

        Vector3 inner = size - new Vector3(margin * 2, 0, margin * 2);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, inner);
    }
}