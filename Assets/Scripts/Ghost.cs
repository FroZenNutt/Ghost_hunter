using UnityEngine;

public class Ghost : MonoBehaviour
{
    public GameConfig config;
    Vector3 velocity;
    Vector3 baseScale;
    float wobble;
    public bool IsDead { get; private set; }

    void Start()
    {
        baseScale = transform.localScale;
        transform.localScale = Vector3.zero;
        wobble = Random.value * 6.28f;
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float speed = config.ghostBaseSpeed * Random.Range(0.5f, 1f);
        velocity = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * speed;
    }

    void Update()
    {
        if (GameManager.I == null || GameManager.I.phase != GamePhase.Play) return;
        wobble += Time.deltaTime * config.ghostWobbleSpeed;
        transform.localScale = Vector3.Lerp(transform.localScale, baseScale, Time.deltaTime * config.ghostSpawnPopSpeed);
        transform.position += Vector3.up * Mathf.Sin(wobble) * Time.deltaTime * config.ghostWobbleHeight;
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject == null) return;
        float speedMultiplier = GameManager.I.inPanic ? config.panicSpeedMultiplier : 1f;
        Vector3 toPlayer = playerObject.transform.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude > 0.0001f) velocity += toPlayer.normalized * config.ghostHomingPull * speedMultiplier * Time.deltaTime;
        float maxSpeed = config.ghostBaseSpeed * config.ghostSpeedCap * speedMultiplier;
        if (velocity.magnitude > maxSpeed) velocity = velocity.normalized * maxSpeed;
        transform.position += velocity * Time.deltaTime;
        Vector2 half = config.arenaSize * 0.5f;
        Vector3 p = transform.position;
        if (p.x < -half.x) { p.x = -half.x; velocity.x = Mathf.Abs(velocity.x); }
        if (p.x > half.x) { p.x = half.x; velocity.x = -Mathf.Abs(velocity.x); }
        if (p.z < -half.y) { p.z = -half.y; velocity.z = Mathf.Abs(velocity.z); }
        if (p.z > half.y) { p.z = half.y; velocity.z = -Mathf.Abs(velocity.z); }
        transform.position = p;
    }

    public void Die()
    {
        if (IsDead) return;
        IsDead = true;
        Destroy(gameObject);
    }
}
