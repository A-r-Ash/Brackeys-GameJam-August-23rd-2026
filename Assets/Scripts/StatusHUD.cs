using UnityEngine;
using TMPro;

public class StatusHUD : MonoBehaviour
{
    [SerializeField] private DayNightCycle cycle;
    [SerializeField] private WoodPile pile;
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
            $"Wood: {pile.Count}\n" +
            $"Fire: {Mathf.CeilToInt(bonfire.CurrentFuel)} / {Mathf.CeilToInt(bonfire.MaxFuel)}";
    }
}