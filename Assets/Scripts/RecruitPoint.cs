using UnityEngine;

// The player interacts here to spend food and add an NPC to the crew.
public class RecruitPoint : MonoBehaviour
{
    [SerializeField] private NPCGatherer npcPrefab;
    [SerializeField] private FoodPile foodPile;
    [SerializeField] private int foodCost = 3;
    [SerializeField] private Transform spawnPoint;   // where the new NPC appears (defaults to this point)

    public int FoodCost => foodCost;

    void Awake()
    {
        if (foodPile == null) foodPile = FindFirstObjectByType<FoodPile>();
    }

    // Returns true if a recruit actually happened (enough food + prefab set)
    public bool TryRecruit()
    {
        if (npcPrefab == null || foodPile == null) return false;
        if (!foodPile.TrySpend(foodCost)) return false;   // not enough food

        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
        Instantiate(npcPrefab, pos, Quaternion.identity);
        return true;
    }
}
