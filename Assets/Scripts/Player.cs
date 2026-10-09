using UnityEngine;

public class Player : MonoBehaviour
{
    public GameConfig config;

    Vector3 dashStart;
    Vector3 dashTarget;
    Vector3 baseScale;
    Vector3 dashDirection;
    Vector3 previousDashPosition;

    float dashT;
    float cooldown;

    bool dashing;
    int dashHits;

    TrailRenderer trail;
    GameObject targetMarker;

    const float DashHitRadius = 1.8f;

    public float CooldownNormalized =>
        config == null || config.dashCooldown <= 0f
            ? 1f
            : 1f - cooldown / config.dashCooldown;

    void Awake()
    {
        baseScale = transform.localScale;

        trail = gameObject.AddComponent<TrailRenderer>();
        trail.time = config.dashTrailTime;
        trail.startWidth = config.dashTrailStartWidth;
        trail.endWidth = config.dashTrailEndWidth;

        trail.material = new Material(
            Shader.Find("Universal Render Pipeline/Particles/Unlit")
            ?? Shader.Find("Sprites/Default")
        );

        trail.startColor = new Color(.49f, 1f, .83f, .75f);
        trail.endColor = new Color(.49f, 1f, .83f, 0f);

        targetMarker = GameObject.CreatePrimitive(
            PrimitiveType.Cylinder
        );

        targetMarker.name = "Dash Target";
        targetMarker.transform.localScale =
            new Vector3(.28f, .01f, .28f);

        targetMarker.GetComponent<Renderer>().material =
            new Material(
                Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard")
            );

        targetMarker.GetComponent<Renderer>().material.color =
            new Color(.49f, 1f, .83f, .7f);

        Destroy(targetMarker.GetComponent<Collider>());
        targetMarker.SetActive(false);
    }

    void Update()
    {
        if (GameManager.I == null ||
            GameManager.I.phase != GamePhase.Play)
            return;

        if (cooldown > 0f)
            cooldown = Mathf.Max(0f, cooldown - Time.deltaTime);

        HandleInput();
        UpdateDash();
    }

    void HandleInput()
    {
        bool pressed =
            Input.GetMouseButtonDown(0) ||
            (Input.touchCount > 0 &&
             Input.GetTouch(0).phase == TouchPhase.Began);

        if (!pressed)
            return;

        Vector3 screenPos = Input.touchCount > 0
            ? Input.GetTouch(0).position
            : Input.mousePosition;

        TryDash(ScreenToWorldOnPlane(screenPos));
    }

    void TryDash(Vector3 targetWorld)
    {
        if (cooldown > 0f || dashing)
            return;

        if (Arena.I == null)
        {
            Debug.LogError(
                "Arena.I is missing. Add an Arena component to the scene."
            );
            return;
        }

        Vector3 current = transform.position;

        targetWorld = Arena.I.ClampPosition(targetWorld);

        if (new Vector3(
                targetWorld.x - current.x,
                0f,
                targetWorld.z - current.z
            ).magnitude < 0.1f)
            return;

        dashStart = current;

        dashTarget = new Vector3(
            targetWorld.x,
            current.y,
            targetWorld.z
        );

        dashDirection = (dashTarget - dashStart).normalized;

        transform.localScale = Vector3.Scale(
            baseScale,
            new Vector3(
                1f + Mathf.Abs(dashDirection.x)
                    * config.dashStretchAmount,
                config.dashSquashY,
                1f + Mathf.Abs(dashDirection.z)
                    * config.dashStretchAmount
            )
        );

        dashT = 0f;
        dashing = true;
        cooldown = config.dashCooldown;
        dashHits = 0;

        previousDashPosition = dashStart;

        targetMarker.transform.position =
            dashTarget + Vector3.up * .03f;

        targetMarker.SetActive(true);
        trail.Clear();

        GameFeel.I?.Dash(transform.position, dashDirection);
    }

    void UpdateDash()
    {
        if (!dashing)
            return;

        dashT += Time.deltaTime / config.dashDuration;

        Vector3 newPosition;

        if (dashT >= 1f)
        {
            newPosition = dashTarget;
        }
        else
        {
            float eased = 1f -
                Mathf.Pow(
                    1f - dashT,
                    config.dashEaseExponent
                );

            newPosition = Vector3.Lerp(
                dashStart,
                dashTarget,
                eased
            );
        }

        // Check the entire movement segment before moving.
        CheckDashPath(previousDashPosition, newPosition);

        transform.position = newPosition;
        previousDashPosition = newPosition;

        if (dashT >= 1f)
        {
            dashing = false;
            targetMarker.SetActive(false);

            transform.localScale = Vector3.Scale(
                baseScale,
                new Vector3(.86f, 1.12f, .86f)
            );

            if (dashHits == 0)
            {
                GameManager.I.RegisterMiss();
            }
            else if (dashHits >= config.comboThreshold)
            {
                GameManager.I.TriggerCombo();
                cooldown = 0f;
            }

            return;
        }

        transform.localScale = Vector3.Lerp(
            transform.localScale,
            baseScale,
            Time.deltaTime * config.dashScaleRecovery
        );
    }

    void CheckDashPath(Vector3 from, Vector3 to)
    {
        Ghost[] ghosts = FindObjectsByType<Ghost>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        float hitRadiusSqr = DashHitRadius * DashHitRadius;
        Vector3 segment = to - from;
        float segmentLengthSqr = segment.sqrMagnitude;

        foreach (Ghost ghost in ghosts)
        {
            if (ghost == null || ghost.IsDead)
                continue;

            Vector3 ghostPosition = ghost.transform.position;

            float t = segmentLengthSqr > 0f
                ? Mathf.Clamp01(
                    Vector3.Dot(
                        ghostPosition - from,
                        segment
                    ) / segmentLengthSqr
                )
                : 0f;

            Vector3 closestPoint = from + segment * t;

            if ((ghostPosition - closestPoint).sqrMagnitude
                > hitRadiusSqr)
                continue;

            ghost.Die();

            dashHits++;

            GameFeel.I?.Kill(ghostPosition);
            GameManager.I.RegisterKill(dashHits);
        }
    }

    Vector3 ScreenToWorldOnPlane(Vector3 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);

        Plane ground = new Plane(Vector3.up, Vector3.zero);

        return ground.Raycast(ray, out float distance)
            ? ray.GetPoint(distance)
            : Vector3.zero;
    }
}