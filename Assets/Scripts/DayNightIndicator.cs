using UnityEngine;
using UnityEngine.UI;

// Drives a filled Image that counts down the current phase and tints day vs night.
public class DayNightIndicator : MonoBehaviour
{
    [SerializeField] private DayNightCycle cycle;
    [SerializeField] private Image fill;
    [SerializeField] private Color dayColor   = new Color(1f, 0.82f, 0.30f);
    [SerializeField] private Color nightColor = new Color(0.30f, 0.42f, 0.80f);

    void Update()
    {
        if (cycle == null || fill == null) return;

        fill.fillAmount = 1f - cycle.PhaseProgress;              // drains as the phase runs out
        fill.color = cycle.IsNight ? nightColor : dayColor;      // yellow by day, blue by night
    }
}
