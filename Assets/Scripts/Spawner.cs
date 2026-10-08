using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameConfig config;
    public GameObject ghostPrefab;
    public Transform ghostContainer;
    float spawnTimer, elapsed;

    void OnEnable() { if (GameManager.I != null) GameManager.I.OnGameStart += ResetSpawner; }
    void OnDisable() { if (GameManager.I != null) GameManager.I.OnGameStart -= ResetSpawner; }
    void Start() { if (GameManager.I != null) GameManager.I.OnGameStart -= ResetSpawner; if (GameManager.I != null) GameManager.I.OnGameStart += ResetSpawner; }

    void ResetSpawner()
    {
        for (int i = ghostContainer.childCount - 1; i >= 0; i--) Destroy(ghostContainer.GetChild(i).gameObject);
        elapsed = 0f;
        spawnTimer = config.initialSpawnInterval;
        for (int i = 0; i < config.initialGhosts; i++) SpawnGhostRandom();
    }

    void Update()
    {
        if (GameManager.I == null || GameManager.I.phase != GamePhase.Play) return;
        elapsed += Time.deltaTime;
        while (ghostContainer.childCount < config.ghostFloor) SpawnGhostRandom();
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            if (ghostContainer.childCount < config.ghostCap) SpawnGhostRandom();
            spawnTimer = Mathf.Lerp(config.initialSpawnInterval, config.minSpawnInterval, Mathf.Clamp01(elapsed / config.spawnRampDuration));
        }
    }

    void SpawnGhostRandom()
    {
        Transform player = GameObject.FindWithTag("Player")?.transform;
        Vector2 half = config.arenaSize * 0.5f;
        Vector3 pos = Vector3.zero;
        for (int i = 0; i < 20; i++)
        {
            pos = new Vector3(Random.Range(-half.x, half.x), 0.5f, Random.Range(-half.y, half.y));
            if (player == null || Vector3.Distance(pos, player.position) >= config.ghostMinSpawnDistance) break;
        }
        GameObject ghost = Instantiate(ghostPrefab, pos, Quaternion.identity, ghostContainer);
        ghost.GetComponent<Ghost>().config = config;
    }
}
