
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameConfig config;
    public GameObject ghostPrefab;
    public Transform ghostContainer;

    float spawnTimer;
    float elapsed;

    void OnEnable()
    {
        SubscribeToGameStart();
    }

    void Start()
    {
        // Ensure subscription works if GameManager initializes later.
        SubscribeToGameStart();
    }

    void OnDisable()
    {
        if (GameManager.I != null)
            GameManager.I.OnGameStart -= ResetSpawner;
    }

    void SubscribeToGameStart()
    {
        if (GameManager.I == null)
            return;

        GameManager.I.OnGameStart -= ResetSpawner;
        GameManager.I.OnGameStart += ResetSpawner;
    }

    void ResetSpawner()
    {
        if (config == null || ghostContainer == null || ghostPrefab == null)
        {
            Debug.LogError(
                "Spawner: Assign Config, Ghost Prefab, and Ghost Container."
            );
            return;
        }

        for (int i = ghostContainer.childCount - 1; i >= 0; i--)
            Destroy(ghostContainer.GetChild(i).gameObject);

        elapsed = 0f;
        spawnTimer = config.initialSpawnInterval;

        int initialCount = Mathf.Min(config.initialGhosts, config.ghostCap);

        for (int i = 0; i < initialCount; i++)
            SpawnGhostRandom();
    }

    void Update()
    {
        if (GameManager.I == null ||
            GameManager.I.phase != GamePhase.Play)
            return;

        if (config == null || ghostContainer == null || ghostPrefab == null)
            return;

        elapsed += Time.deltaTime;

        // Maintain the minimum ghost count without exceeding the cap.
        while (ghostContainer.childCount < config.ghostFloor &&
               ghostContainer.childCount < config.ghostCap)
        {
            SpawnGhostRandom();
        }

        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            if (ghostContainer.childCount < config.ghostCap)
                SpawnGhostRandom();

            spawnTimer = Mathf.Lerp(
                config.initialSpawnInterval,
                config.minSpawnInterval,
                Mathf.Clamp01(elapsed / config.spawnRampDuration)
            );
        }
    }

    void SpawnGhostRandom()
    {
        if (Arena.I == null)
        {
            Debug.LogError("Spawner: Arena.I is missing.");
            return;
        }

        Transform player = GameObject.FindWithTag("Player")?.transform;

        Vector3 spawnPosition = Vector3.zero;
        bool validPosition = false;

        for (int i = 0; i < 20; i++)
        {
            spawnPosition = Arena.I.GetRandomPosition(1.5f);
            spawnPosition.y = 0.5f;

            if (player == null ||
                Vector3.Distance(spawnPosition, player.position) >=
                config.ghostMinSpawnDistance)
            {
                validPosition = true;
                break;
            }
        }

        if (!validPosition && player != null)
        {
            // Do not spawn too close to the player if all attempts fail.
            return;
        }

        GameObject ghost = Instantiate(
            ghostPrefab,
            spawnPosition,
            Quaternion.identity,
            ghostContainer
        );

        Ghost ghostScript = ghost.GetComponent<Ghost>();

        if (ghostScript != null)
        {
            ghostScript.config = config;
        }
        else
        {
            Debug.LogError(
                "Spawner: Ghost prefab is missing the Ghost component."
            );
        }
    }
}