using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StatusHUD : MonoBehaviour
{
    [SerializeField] private DayNightCycle cycle;
    [SerializeField] private WoodPile pile;
    [SerializeField] private FoodPile foodPile;
    [SerializeField] private Bonfire bonfire;
    [SerializeField] private TMP_Text statusText;

    void Update()
    {
        string phase = cycle.IsNight ? "Night" : "Day";
        int timeLeft = Mathf.CeilToInt(cycle.PhaseTimeRemaining);

        statusText.text =
            $"Day {cycle.DayNumber}\n" +
            $"{phase} ({timeLeft}s)\n" +
            $"Crew: {NPCGatherer.Count}\n" +
            $"Imposter: {CountImpostors()}\n" +
            $"Wood: {pile.Count}\n" +
            $"Food: {(foodPile != null ? foodPile.Count : 0)}\n" +
            $"Fire: {Mathf.CeilToInt(bonfire.CurrentFuel)} / {Mathf.CeilToInt(bonfire.MaxFuel)}";
    }

    int CountImpostors()
    {
        // Gather every NPC, then count the ones flagged as impostors
        List<NPCGatherer> npcs = new List<NPCGatherer>(FindObjectsByType<NPCGatherer>(FindObjectsSortMode.None));

        int count = 0;
        foreach (NPCGatherer npc in npcs)
            if (npc.IsImpostor)
                count++;

        return count;
    }
}