using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;                          // the Player
    [SerializeField] private Vector2 deadzoneSize = new Vector2(4f, 3f); // width, height of the invisible box

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 camPos = transform.position;
        float halfW = deadzoneSize.x * 0.5f;
        float halfH = deadzoneSize.y * 0.5f;

        // How far the player is from the camera's center
        float dx = target.position.x - camPos.x;
        float dy = target.position.y - camPos.y;

        // Move the camera ONLY when the player pushes past an edge of the box
        if (dx > halfW)       camPos.x += dx - halfW;   // pushed right edge
        else if (dx < -halfW) camPos.x += dx + halfW;   // pushed left edge

        if (dy > halfH)       camPos.y += dy - halfH;   // pushed top edge
        else if (dy < -halfH) camPos.y += dy + halfH;   // pushed bottom edge

        transform.position = camPos;
    }

    // Draws the invisible box in the Scene view so you can SEE and tune it
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(deadzoneSize.x, deadzoneSize.y, 0f));
    }
}