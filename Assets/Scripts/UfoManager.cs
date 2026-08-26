using UnityEngine;

public class UfoManager : MonoBehaviour
{
    [SerializeField] private DayNightCycle cycle;
    [SerializeField] private Ufo ufoPrefab;

    [Header("Random hover area")]
    [SerializeField] private Transform stopArea;                       // center of where the UFO can hover
    [SerializeField] private Vector2 stopAreaSize = new Vector2(10f, 4f); // width, height of that area
    [SerializeField] private float entryDistance = 12f;                // how far off-screen it enters from (above)

    [Header("How many per night (linear scaling)")]
    [SerializeField] private int firstSpawnNight = 1;        // first night UFOs appear
    [SerializeField] private int baseUfos = 1;              // UFOs on the first spawn night
    [SerializeField] private float ufoGrowthPerNight = 0.5f; // extra UFOs added per night (0.5 = +1 every 2 nights)
    [SerializeField] private int maxUfos = 5;               // hard cap so it never explodes

    void OnEnable()
    {
        if (cycle != null) cycle.OnNightStart += HandleNight;
    }

    void OnDisable()
    {
        if (cycle != null) cycle.OnNightStart -= HandleNight;
    }

    void HandleNight()
    {
        if (cycle.DayNumber < firstSpawnNight) return;

        int nightsSince = cycle.DayNumber - firstSpawnNight;                       // 0 on the first spawn night
        int count = baseUfos + Mathf.FloorToInt(nightsSince * ufoGrowthPerNight);
        count = Mathf.Clamp(count, 1, maxUfos);

        for (int i = 0; i < count; i++)
            SpawnUfo();
    }

    void SpawnUfo()
    {
        Vector3 stopPos = RandomStopPos();
        Vector3 entryPos = stopPos + Vector3.up * entryDistance;   // enters from above, exits back up

        Ufo ufo = Instantiate(ufoPrefab, entryPos, Quaternion.identity);
        ufo.Launch(stopPos, entryPos);
    }

    Vector3 RandomStopPos()
    {
        Vector3 c = stopArea != null ? stopArea.position : transform.position;
        float x = Random.Range(-stopAreaSize.x * 0.5f, stopAreaSize.x * 0.5f);
        float y = Random.Range(-stopAreaSize.y * 0.5f, stopAreaSize.y * 0.5f);
        return c + new Vector3(x, y, 0f);
    }

    // Shows the hover area (green) in the Scene view so you can size it
    void OnDrawGizmos()
    {
        Vector3 c = stopArea != null ? stopArea.position : transform.position;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(c, new Vector3(stopAreaSize.x, stopAreaSize.y, 0f));
    }
}
