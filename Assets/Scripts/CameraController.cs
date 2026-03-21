using UnityEngine;

/// <summary>
/// Camara con paneo (click derecho + arrastrar) y zoom (scroll).
/// Ideal para demos y presentaciones.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Zoom")]
    public float zoomSpeed = 5f;
    public float minZoom = 5f;
    public float maxZoom = 60f;

    [Header("Paneo")]
    public float panSpeed = 0.5f;

    [Header("Limites de paneo (opcional)")]
    public bool limitPan = true;
    public float panMinX = -30f;
    public float panMaxX = 30f;
    public float panMinZ = -30f;
    public float panMaxZ = 30f;

    private Camera cam;
    private Vector3 lastMousePos;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
            cam = Camera.main;
    }

    void Update()
    {
        HandleZoom();
        HandlePan();
    }

    // ── Zoom con scroll ────────────────────────────────────────

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) < 0.001f) return;

        if (cam.orthographic)
        {
            cam.orthographicSize -= scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }
        else
        {
            Vector3 pos = transform.position;
            pos.y -= scroll * zoomSpeed * 10f;
            pos.y = Mathf.Clamp(pos.y, minZoom, maxZoom);
            transform.position = pos;
        }
    }

    // ── Paneo con click derecho + arrastrar ────────────────────

    void HandlePan()
    {
        if (Input.GetMouseButtonDown(1))
            lastMousePos = Input.mousePosition;

        if (Input.GetMouseButton(1))
        {
            Vector3 delta = Input.mousePosition - lastMousePos;
            lastMousePos = Input.mousePosition;

            Vector3 move = new Vector3(-delta.x, 0, -delta.y) * panSpeed * Time.deltaTime;
            Vector3 pos = transform.position + move;

            if (limitPan)
            {
                pos.x = Mathf.Clamp(pos.x, panMinX, panMaxX);
                pos.z = Mathf.Clamp(pos.z, panMinZ, panMaxZ);
            }

            transform.position = pos;
        }
    }
}