using UnityEngine;

/// <summary>
/// Clase base para zonas del escenario (río, playa, etc.)
/// </summary>
public abstract class Area : MonoBehaviour
{
    public Vector3 size = new Vector3(40, 1, 20);
    public float margin = 1f;

    protected Vector3 center;

    protected virtual void Awake()
    {
        center = transform.position;
    }

    public bool IsInside(Vector3 pos)
    {
        Vector3 half = size * 0.5f;

        return pos.x >= center.x - half.x &&
               pos.x <= center.x + half.x &&
               pos.z >= center.z - half.z &&
               pos.z <= center.z + half.z;
    }

    public Vector3 ClampInside(Vector3 pos)
    {
        Vector3 half = size * 0.5f;

        pos.x = Mathf.Clamp(pos.x, center.x - half.x + margin, center.x + half.x - margin);
        pos.z = Mathf.Clamp(pos.z, center.z - half.z + margin, center.z + half.z - margin);

        return pos;
    }

    public Vector3 GetRandomPoint()
    {
        Vector3 half = size * 0.5f;

        float x = Random.Range(center.x - half.x + margin, center.x + half.x - margin);
        float z = Random.Range(center.z - half.z + margin, center.z + half.z - margin);

        return new Vector3(x, transform.position.y, z);
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.DrawCube(transform.position, size);

        Vector3 inner = size - new Vector3(margin * 2, 0, margin * 2);
        Gizmos.DrawWireCube(transform.position, inner);
    }
}