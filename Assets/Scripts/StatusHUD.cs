using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusHUD : MonoBehaviour
{
    [Header("Sources")]
    [SerializeField] private DayNightCycle cycle;
    [SerializeField] private WoodPile pile;
    [SerializeField] private FoodPile foodPile;
    [SerializeField] private Bonfire bonfire;

    [Header("Value texts (next to each icon)")]
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text timerText;   // countdown to next phase, by the day/night icon
    [SerializeField] private TMP_Text npcText;
    [SerializeField] private TMP_Text impostorText;
    [SerializeField] private TMP_Text woodText;
    [SerializeField] private TMP_Text foodText;
    [SerializeField] private TMP_Text fireText;

    [Header("Day/night icon (swaps sprite)")]
    [SerializeField] private Image dayNightIcon;
    [SerializeField] private Sprite daySprite;
    [SerializeField] private Sprite nightSprite;

    void Update()
    {
        if (cycle != null)
        {
            if (dayText != null) dayText.text = "Day " + cycle.DayNumber;
            if (dayNightIcon != null && daySprite != null && nightSprite != null)
                dayNightIcon.sprite = cycle.IsNight ? nightSprite : daySprite;
            if (timerText != null)
                timerText.text = cycle.IsPaused ? "" : Mathf.CeilToInt(cycle.PhaseTimeRemaining) + "s";
        }

        if (npcText != null)      npcText.text = NPCGatherer.Count.ToString();
        if (impostorText != null) impostorText.text = NPCGatherer.ImpostorCount.ToString();

        // Each count is summed across BOTH camps' piles.
        if (woodText != null) woodText.text = TotalWood().ToString();
        if (foodText != null) foodText.text = TotalFood().ToString();

        if (bonfire != null && fireText != null)  fireText.text = Mathf.CeilToInt(bonfire.CurrentFuel).ToString();
    }

    // Wood/food across every camp pile; falls back to the serialized single pile.
    int TotalWood()
    {
        var all = FindObjectsByType<WoodPile>(FindObjectsSortMode.None);
        if (all.Length == 0) return pile != null ? pile.Count : 0;
        int total = 0;
        foreach (WoodPile p in all) total += p.Count;
        return total;
    }

    int TotalFood()
    {
        var all = FindObjectsByType<FoodPile>(FindObjectsSortMode.None);
        if (all.Length == 0) return foodPile != null ? foodPile.Count : 0;
        int total = 0;
        foreach (FoodPile p in all) total += p.Count;
        return total;
    }
}
