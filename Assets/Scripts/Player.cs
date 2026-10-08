using UnityEngine;

public class Player : MonoBehaviour
{
    public GameConfig config;
    Vector3 dashStart, dashTarget, baseScale, dashDirection;
    float dashT, cooldown;
    bool dashing;
    int dashHits;
    TrailRenderer trail;
    GameObject targetMarker;
    public float CooldownNormalized => config == null || config.dashCooldown <= 0f ? 1f : 1f - cooldown / config.dashCooldown;

    void Awake()
    {
        baseScale = transform.localScale;
        trail = gameObject.AddComponent<TrailRenderer>();
        trail.time = config.dashTrailTime; trail.startWidth = config.dashTrailStartWidth; trail.endWidth = config.dashTrailEndWidth;
        trail.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Sprites/Default"));
        trail.startColor = new Color(.49f, 1f, .83f, .75f);
        trail.endColor = new Color(.49f, 1f, .83f, 0f);
        targetMarker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        targetMarker.name = "Dash Target";
        targetMarker.transform.localScale = new Vector3(.28f, .01f, .28f);
        targetMarker.GetComponent<Renderer>().material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        targetMarker.GetComponent<Renderer>().material.color = new Color(.49f, 1f, .83f, .7f);
        Destroy(targetMarker.GetComponent<Collider>());
        targetMarker.SetActive(false);
    }

    void Update()
    {
        if (GameManager.I == null || GameManager.I.phase != GamePhase.Play) return;
        if (cooldown > 0f) cooldown = Mathf.Max(0f, cooldown - Time.deltaTime);
        HandleInput();
        UpdateDash();
    }

    void HandleInput()
    {
        bool pressed = Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
        if (!pressed) return;
        Vector3 screenPos = Input.touchCount > 0 ? Input.GetTouch(0).position : Input.mousePosition;
        TryDash(ScreenToWorldOnPlane(screenPos));
    }

    void TryDash(Vector3 targetWorld)
    {
        if (cooldown > 0f || dashing) return;
        Vector3 current = transform.position;
        if (new Vector3(targetWorld.x - current.x, 0f, targetWorld.z - current.z).magnitude < 0.1f) return;
        Vector2 half = config.arenaSize * 0.5f;
        dashStart = current;
        dashTarget = new Vector3(Mathf.Clamp(targetWorld.x, -half.x, half.x), current.y, Mathf.Clamp(targetWorld.z, -half.y, half.y));
        dashDirection = (dashTarget - dashStart).normalized;
        transform.localScale = Vector3.Scale(baseScale, new Vector3(1f + Mathf.Abs(dashDirection.x) * config.dashStretchAmount, config.dashSquashY, 1f + Mathf.Abs(dashDirection.z) * config.dashStretchAmount));
        dashT = 0f;
        dashing = true;
        cooldown = config.dashCooldown;
        dashHits = 0;
        targetMarker.transform.position = dashTarget + Vector3.up * .03f;
        targetMarker.SetActive(true);
        trail.Clear();
        GameFeel.I?.Dash(transform.position, dashDirection);
    }

    void UpdateDash()
    {
        if (!dashing) return;
        dashT += Time.deltaTime / config.dashDuration;
        if (dashT >= 1f)
        {
            transform.position = dashTarget;
            dashing = false;
            targetMarker.SetActive(false);
            transform.localScale = Vector3.Scale(baseScale, new Vector3(.86f, 1.12f, .86f));
            if (dashHits == 0) GameManager.I.RegisterMiss();
            else if (dashHits >= config.comboThreshold) { GameManager.I.TriggerCombo(); cooldown = 0f; }
            return;
        }
        float eased = 1f - Mathf.Pow(1f - dashT, config.dashEaseExponent);
        transform.position = Vector3.Lerp(dashStart, dashTarget, eased);
        transform.localScale = Vector3.Lerp(transform.localScale, baseScale, Time.deltaTime * config.dashScaleRecovery);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!dashing || !other.CompareTag("Ghost")) return;
        Ghost ghost = other.GetComponent<Ghost>();
        if (ghost == null || ghost.IsDead) return;
        ghost.Die();
        dashHits++;
        GameFeel.I?.Kill(other.transform.position);
        GameManager.I.RegisterKill(dashHits);
    }

    Vector3 ScreenToWorldOnPlane(Vector3 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        Plane ground = new Plane(Vector3.up, Vector3.zero);
        return ground.Raycast(ray, out float distance) ? ray.GetPoint(distance) : Vector3.zero;
    }
}
