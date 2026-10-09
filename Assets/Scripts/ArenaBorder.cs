using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ArenaBorder : MonoBehaviour
{
    LineRenderer line;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.loop = true;
        line.positionCount = 4;
    }

    void LateUpdate()
    {
        if (Arena.I == null)
            return;

        Vector2 min = Arena.I.ArenaMin;
        Vector2 max = Arena.I.ArenaMax;

        // Draw the rectangle on the X-Z ground plane.
        line.SetPosition(0, new Vector3(min.x, 0.02f, min.y));
        line.SetPosition(1, new Vector3(max.x, 0.02f, min.y));
        line.SetPosition(2, new Vector3(max.x, 0.02f, max.y));
        line.SetPosition(3, new Vector3(min.x, 0.02f, max.y));
    }
}