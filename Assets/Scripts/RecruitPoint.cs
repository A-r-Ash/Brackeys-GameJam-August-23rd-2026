using UnityEngine;

// The player interacts here to spend food and add an NPC to the crew.
public class RecruitPoint : MonoBehaviour
{
    [SerializeField] private NPCGatherer npcPrefab;
    [SerializeField] private FoodPile foodPile;
    [SerializeField] private int foodCost = 3;
    [SerializeField] private Transform spawnPoint;   // where the new NPC appears (defaults to this point)

    private bool campActive = true;                  // false until its Camp is found (side camp)
    private WoodPile npcWoodPile;                    // the camp's woodpile, so recruits gather to the right pile
    private Bonfire npcBonfire;                      // the camp's bonfire, so recruits gather/stay at this camp

    public int FoodCost => foodCost;

    void Awake()
    {
        if (foodPile == null)
        {
            // Fall back to the closest food pile so a recruit spends THIS camp's food.
            FoodPile best = null;
            float bestD = float.MaxValue;
            foreach (FoodPile fp in FindObjectsByType<FoodPile>(FindObjectsSortMode.None))
            {
                float d = Vector2.SqrMagnitude(fp.transform.position - transform.position);
                if (d < bestD) { bestD = d; best = fp; }
            }
            foodPile = best;
        }
    }

    public void SetCampActive(bool value) => campActive = value;

    public void SetNpcWoodPile(WoodPile pile) => npcWoodPile = pile;

    public void SetCampBonfire(Bonfire fire) => npcBonfire = fire;

    // Spend food from anywhere: own pile first, then the other camps' piles.
    bool SpendFood(int amount)
    {
        int remaining = amount;

        if (foodPile != null && foodPile.Count > 0)
        {
            int take = Mathf.Min(foodPile.Count, remaining);
            if (foodPile.TrySpend(take)) remaining -= take;
        }

        foreach (FoodPile fp in FindObjectsByType<FoodPile>(FindObjectsSortMode.None))
        {
            if (remaining <= 0) break;
            if (fp == foodPile) continue;
            int take = Mathf.Min(fp.Count, remaining);
            if (take > 0 && fp.TrySpend(take)) remaining -= take;
        }

        return remaining <= 0;
    }

    // Returns true if a recruit actually happened (camp found + enough food + prefab set).
    // Shows a floating-text reason when it fails so it's never a silent mystery.
    public bool TryRecruit()
    {
        if (!campActive)
        {
            FloatingText.Show(transform.position, "Camp not active", Color.yellow);
            return false;
        }
        if (npcPrefab == null)
        {
            Debug.LogWarning("RecruitPoint has no NPC prefab assigned.", this);
            FloatingText.Show(transform.position, "No NPC prefab set!", Color.red);
            return false;
        }
        if (foodPile == null && FindObjectsByType<FoodPile>(FindObjectsSortMode.None).Length == 0)
        {
            FloatingText.Show(transform.position, "No food piles exist!", Color.red);
            return false;
        }

        int totalFood = 0;
        foreach (FoodPile fp in FindObjectsByType<FoodPile>(FindObjectsSortMode.None)) totalFood += fp.Count;
        if (totalFood < foodCost)
        {
            // Check WITHOUT spending - keep the player's food when it isn't enough.
            FloatingText.Show(transform.position, "Need " + foodCost + " Food (have " + totalFood + ")", FloatingText.FoodColor);
            return false;
        }
        SpendFood(foodCost);   // confirmed enough, now actually deduct

        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
        NPCGatherer npc = Instantiate(npcPrefab, pos, Quaternion.identity);
        if (npc != null)
        {
            if (npcWoodPile != null) npc.SetPile(npcWoodPile);   // crew works at THIS camp's pile
            if (npcBonfire != null) npc.SetBonfire(npcBonfire);  // and congregates at THIS camp's fire
            npc.SetDormant(false);
        }
        FloatingText.Show(pos, "Recruited!", FloatingText.RecruitColor);
        SoundManager.Instance?.Recruit(pos);
        return true;
    }
}