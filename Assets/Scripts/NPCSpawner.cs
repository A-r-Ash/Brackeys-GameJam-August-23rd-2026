using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [SerializeField] private DayNightCycle cycle;
    [SerializeField] private NPCGatherer npcPrefab;
    [SerializeField] private Transform[] spawnPoints;   // where new crew arrive from
    [SerializeField] private int firstSpawnDay = 2;     // first morning they appear
    [SerializeField] private int countPerMorning = 2;   // how many join each morning

    void OnEnable()
    {
        if (cycle != null) cycle.OnDayStart += HandleMorning;
    }

    void OnDisable()
    {
        if (cycle != null) cycle.OnDayStart -= HandleMorning;
    }

    void HandleMorning()
    {
        if (cycle.DayNumber < firstSpawnDay) return;
        if (npcPrefab == null || spawnPoints == null || spawnPoints.Length == 0) return;

        for (int i = 0; i < countPerMorning; i++)
        {
            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Instantiate(npcPrefab, point.position, Quaternion.identity);
        }
    }
}
