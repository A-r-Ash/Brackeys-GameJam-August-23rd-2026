using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;   // for Light2D

public class Bonfire : MonoBehaviour
{
    [SerializeField] private float maxFuel = 100f;
    [SerializeField] private float drainPerSecond = 2f;
    [SerializeField] private float currentFuel;
    [SerializeField] private Slider fuelSlider;

    [Header("Night light")]
    [SerializeField] private DayNightCycle cycle;
    [SerializeField] private Light2D fireLight;
    [SerializeField] private float baseIntensity = 1f;
    [SerializeField] private float pulseAmount = 0.3f;   // how far it swings
    [SerializeField] private float pulseSpeed = 3f;      // how fast it pulses

    public float CurrentFuel => currentFuel;
    public float MaxFuel => maxFuel;

    void Start()
    {
        currentFuel = maxFuel;
    }

    void Update()
    {
        if (cycle != null && cycle.IsNight)
        {
            // Fire only burns down at night
            currentFuel -= drainPerSecond * Time.deltaTime;
            if (currentFuel <= 0f)
            {
                currentFuel = 0f;
                Debug.Log("The fire went out! Game Over.");
            }

            // Pulse the light at night
            if (fireLight != null)
            {
                fireLight.enabled = true;
                fireLight.intensity = baseIntensity + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            }
        }
        else
        {
            // Daytime: no drain, light off
            if (fireLight != null)
                fireLight.enabled = false;
        }

        if (fuelSlider != null)
            fuelSlider.value = currentFuel / maxFuel;
    }

    public void AddFuel(float amount)
    {
        currentFuel = Mathf.Min(currentFuel + amount, maxFuel);
    }
}