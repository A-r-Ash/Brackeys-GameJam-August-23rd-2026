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

    [Header("Camp role")]
    [SerializeField] private bool causesDefeat = true;   // main camp's fire out = lose; side fires just go out
    private bool campActive;                             // false until a Camp component activates it

    public float CurrentFuel => currentFuel;
    public float MaxFuel => maxFuel;
    public int TimesFed { get; private set; }

    void Awake()
    {
        campActive = causesDefeat;   // a lone main fire works without a Camp component
        currentFuel = maxFuel;
    }

    public void CampActivate()
    {
        // Called when a Camp is first found/activated: the fire lights and refills.
        campActive = true;
        currentFuel = maxFuel;
    }

    void Update()
    {
        bool night = cycle != null && cycle.IsNight;

        if (night)
        {
            bool lit = false;

            if (campActive)
            {
                // Active fires only burn down at night
                currentFuel -= drainPerSecond * Time.deltaTime;
                if (currentFuel <= 0f)
                {
                    currentFuel = 0f;
                    if (causesDefeat) GameStateManager.Instance?.Lose();   // only main fire out = defeat
                }
                lit = currentFuel > 0f;                                    // side fires go out and wait for wood
            }

            // Pulse the light at night
            if (fireLight != null)
            {
                fireLight.enabled = lit;
                if (lit) fireLight.intensity = baseIntensity + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            }

            if (fireSource != null && lit && !fireSource.isPlaying) fireSource.Play();
            else if (fireSource != null && !lit && fireSource.isPlaying) fireSource.Pause();

            if (fireAnimator != null) fireAnimator.enabled = lit;             // flames only while lit
            if (fireSprite != null && deadSprite != null && !lit)
                fireSprite.sprite = deadSprite;                               // show the dead-fire sprite
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