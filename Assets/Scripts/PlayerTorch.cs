using UnityEngine;

// Turns the player's torch on at night, off during the day.
public class PlayerTorch : MonoBehaviour
{
    [SerializeField] private DayNightCycle cycle;
    [SerializeField] private GameObject torch;   // the torch light object (and any child sprite/particles)

    void Awake()
    {
        if (cycle == null) cycle = FindFirstObjectByType<DayNightCycle>();
    }

    void Update()
    {
        if (cycle != null && torch != null)
            torch.SetActive(cycle.IsNight);
    }
}
