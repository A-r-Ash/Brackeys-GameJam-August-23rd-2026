using UnityEngine;

// Spawns a set of berry bushes at random spots inside a zone; each relocates itself when picked clean.
public class BerryBushManager : MonoBehaviour
{
    [SerializeField] private BerryBush bushPrefab;
    [SerializeField] private Transform zoneCenter;                    // defaults to this object
    [SerializeField] private Vector2 zoneSize = new Vector2(12f, 8f);
    [SerializeField] private int bushCount = 5;

    void Start()
    {
        if (bushPrefab == null) return;

        Vector3 center = zoneCenter != null ? zoneCenter.position : transform.position;
        for (int i = 0; i < bushCount; i++)
        {
            BerryBush b = Instantiate(bushPrefab, center, Quaternion.identity);
            b.SetZone(center, zoneSize);   // places it at a random spot in the zone
        }
    }

    void OnDrawGizmos()
    {
        Vector3 c = zoneCenter != null ? zoneCenter.position : transform.position;
        Gizmos.color = new Color(0.4f, 1f, 0.4f);
        Gizmos.DrawWireCube(c, new Vector3(zoneSize.x, zoneSize.y, 0f));
    }
}
