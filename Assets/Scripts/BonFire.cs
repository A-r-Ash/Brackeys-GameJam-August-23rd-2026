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
    [SerializeField] private AudioSource fireSource;     // looping fire crackle (plays at night)
    [SerializeField] private ParticleSystem feedEffect;  // burst when wood is added

    [Header("Fire sprite")]
    [SerializeField] private Animator fireAnimator;      // the lit-fire animation
    [SerializeField] private SpriteRenderer fireSprite;
    [SerializeField] private Sprite deadSprite;          // shown by day (fire is out)

    public float CurrentFuel => currentFuel;
    public float MaxFuel => maxFuel;
    public int TimesFed { get; private set; }

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
                GameStateManager.Instance?.Lose();   // fire out → game over
            }

            // Pulse the light at night
            if (fireLight != null)
            {
                fireLight.enabled = true;
                fireLight.intensity = baseIntensity + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            }

            if (fireSource != null && !fireSource.isPlaying) fireSource.Play();   // crackle at night

            if (fireAnimator != null) fireAnimator.enabled = true;               // animate the flames
        }
        else
        {
            // Daytime: no drain, light off, fire is dead
            if (fireLight != null)
                fireLight.enabled = false;

            if (fireSource != null && fireSource.isPlaying) fireSource.Pause();

            if (fireAnimator != null) fireAnimator.enabled = false;              // stop the animation
            if (fireSprite != null && deadSprite != null)
                fireSprite.sprite = deadSprite;                                   // show the dead-fire sprite
        }

        if (fuelSlider != null)
            fuelSlider.value = currentFuel / maxFuel;
    }

    public void AddFuel(float amount)
    {
        currentFuel = Mathf.Min(currentFuel + amount, maxFuel);
        TimesFed++;
        if (feedEffect != null) feedEffect.Play();   // spark burst when fed
    }
}