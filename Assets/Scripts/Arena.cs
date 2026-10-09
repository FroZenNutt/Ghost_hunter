using UnityEngine;

public class Arena : MonoBehaviour
{
    public static Arena I { get; private set; }

    [Header("Camera")]
    [SerializeField] Camera mainCamera;

    [Header("Arena Margins (World Units)")]
    [SerializeField] float horizontalMargin = 1f;
    [SerializeField] float verticalMargin = 1f;

    public Vector2 ArenaSize { get; private set; }
    public Vector2 ArenaMin { get; private set; }
    public Vector2 ArenaMax { get; private set; }

    void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;

        if (mainCamera == null)
            mainCamera = Camera.main;

        UpdateArenaSize();
    }

    public void UpdateArenaSize()
    {
        if (mainCamera == null)
            return;

        float halfHeight = mainCamera.orthographicSize;
        float halfWidth = halfHeight * mainCamera.aspect;

        ArenaMin = new Vector2(
            -halfWidth + horizontalMargin,
            -halfHeight + verticalMargin
        );

        ArenaMax = new Vector2(
            halfWidth - horizontalMargin,
            halfHeight - verticalMargin
        );

        ArenaSize = ArenaMax - ArenaMin;
    }

    public Vector3 ClampPosition(Vector3 position)
    {
        position.x = Mathf.Clamp(
            position.x, ArenaMin.x, ArenaMax.x
        );

        position.z = Mathf.Clamp(
            position.z, ArenaMin.y, ArenaMax.y
        );

        return position;
    }

    public Vector3 GetRandomPosition(float padding = 0f)
    {
        return new Vector3(
            Random.Range(ArenaMin.x + padding, ArenaMax.x - padding),
            0f,
            Random.Range(ArenaMin.y + padding, ArenaMax.y - padding)
        );
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        horizontalMargin = Mathf.Max(0f, horizontalMargin);
        verticalMargin = Mathf.Max(0f, verticalMargin);
    }
#endif
}