using UnityEngine;

/// <summary>
/// Comportamiento del nadador rescatado caminando en la playa.
/// Despues de un tiempo descansando, hace fade out y vuelve al pool.
/// </summary>
public class SwimmerOnLand : MonoBehaviour
{
    private float walkSpeed = 1.5f;
    private float directionChangeTime = 4f;
    private float waterAvoidDistance = 1.5f;

    [Header("Fade out")]
    public float restTime = 6f;    // segundos caminando antes del fade
    public float fadeOutTime = 2f;    // duracion del fade out

    private Vector3 direction;
    private float timer;
    private float restTimer;
    private bool isFading;
    private float fadeTimer;

    private BeachArea beach;
    private RiverArea river;
    private Renderer rend;
    private bool initialized;

    void Awake()
    {
        RescueManager manager = FindObjectOfType<RescueManager>();
        if (manager != null && manager.GetConfig() != null)
        {
            SimulationConfig cfg = manager.GetConfig();
            walkSpeed = cfg.landWalkSpeed;
            directionChangeTime = cfg.landDirectionChangeTime;
            waterAvoidDistance = cfg.landWaterAvoidDistance;
        }

        beach = FindObjectOfType<BeachArea>();
        river = FindObjectOfType<RiverArea>();
        rend = GetComponentInChildren<Renderer>();

        PickDirection();
        initialized = true;
    }

    void OnEnable()
    {
        timer = 0f;
        restTimer = 0f;
        fadeTimer = 0f;
        isFading = false;

        // Asegurarse que el material es completamente opaco al activarse
        SetAlpha(1f);

        if (initialized)
            PickDirection();
    }

    void Update()
    {
        if (isFading)
        {
            HandleFade();
            return;
        }

        restTimer += Time.deltaTime;
        if (restTimer >= restTime)
        {
            StartFade();
            return;
        }

        timer += Time.deltaTime;
        if (timer > directionChangeTime)
        {
            PickDirection();
            timer = 0f;
        }

        Move();
    }

    // ── Movimiento ─────────────────────────────────────────────

    void Move()
    {
        if (direction == Vector3.zero) return;

        Vector3 nextPos = transform.position + direction * walkSpeed * Time.deltaTime;
        Vector3 checkPos = nextPos + direction * waterAvoidDistance;

        if (river != null && river.IsInside(checkPos)) { PickDirection(); return; }
        if (beach != null && !beach.IsInside(nextPos)) { PickDirection(); return; }

        transform.position = nextPos;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(direction),
            8f * Time.deltaTime
        );
    }

    void PickDirection()
    {
        Vector2 r = Random.insideUnitCircle.normalized;
        direction = new Vector3(r.x, 0, r.y);
    }

    // ── Fade out ───────────────────────────────────────────────

    void StartFade()
    {
        isFading = true;
        fadeTimer = 0f;
    }

    void HandleFade()
    {
        fadeTimer += Time.deltaTime;
        float alpha = Mathf.Lerp(1f, 0f, fadeTimer / fadeOutTime);
        SetAlpha(alpha);

        if (fadeTimer >= fadeOutTime)
            ReturnToPool();
    }

    void SetAlpha(float alpha)
    {
        if (rend == null) return;

        Color c = rend.material.color;
        c.a = alpha;
        rend.material.color = c;

        // Cambiar render mode a Fade si alpha < 1
        if (alpha < 1f)
            SetMaterialFadeMode();
    }

    void SetMaterialFadeMode()
    {
        if (rend == null) return;

        Material mat = rend.material;

        // Configurar el shader standard de Unity para transparencia
        mat.SetFloat("_Mode", 2);   // Fade mode
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
    }

    void ReturnToPool()
    {
        // Restaurar material opaco antes de devolver al pool
        SetAlpha(1f);
        if (rend != null)
        {
            Material mat = rend.material;
            mat.SetFloat("_Mode", 0);   // Opaque mode
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            mat.SetInt("_ZWrite", 1);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = -1;
        }

        // Desactivar este componente y devolver el swimmer al pool
        enabled = false;
        RescueManager manager = FindObjectOfType<RescueManager>();
        if (manager != null)
            manager.RemoveSwimmer(GetComponent<Swimmer>());
    }
}