using UnityEngine;
using UnityEngine.Rendering.Universal;   // needed for Light2D

public class DayNightLighting : MonoBehaviour
{
    [SerializeField] private DayNightCycle cycle;
    [SerializeField] private Light2D globalLight;
    [SerializeField] private float dayIntensity = 1f;
    [SerializeField] private float nightIntensity = 0.2f;
    [SerializeField] private float fadeSpeed = 1f;

    void Update()
    {
        float target = cycle.IsNight ? nightIntensity : dayIntensity;
        globalLight.intensity = Mathf.MoveTowards(globalLight.intensity, target, fadeSpeed * Time.deltaTime);
    }
}