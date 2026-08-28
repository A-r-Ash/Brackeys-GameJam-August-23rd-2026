using System.Collections.Generic;
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
        }

        if (npcText != null)      npcText.text = NPCGatherer.Count.ToString();
        if (impostorText != null) impostorText.text = CountImpostors().ToString();
        if (pile != null && woodText != null)     woodText.text = pile.Count.ToString();
        if (foodPile != null && foodText != null) foodText.text = foodPile.Count.ToString();
        if (bonfire != null && fireText != null)  fireText.text = Mathf.CeilToInt(bonfire.CurrentFuel).ToString();
    }

    int CountImpostors()
    {
        List<NPCGatherer> npcs = new List<NPCGatherer>(FindObjectsByType<NPCGatherer>(FindObjectsSortMode.None));

        int count = 0;
        foreach (NPCGatherer npc in npcs)
            if (npc.IsImpostor)
                count++;

        return count;
    }
}
