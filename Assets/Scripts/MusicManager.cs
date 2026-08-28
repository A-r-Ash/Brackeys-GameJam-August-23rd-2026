using UnityEngine;

// Plays day + night tracks at once and crossfades between them with the cycle.
public class MusicManager : MonoBehaviour
{
    [SerializeField] private DayNightCycle cycle;
    [SerializeField] private AudioSource dayMusic;
    [SerializeField] private AudioSource nightMusic;
    [SerializeField] private float volume = 0.5f;
    [SerializeField] private float fadeSpeed = 0.5f;

    void Start()
    {
        if (dayMusic != null)   { dayMusic.loop = true;   dayMusic.volume = volume; dayMusic.Play(); }
        if (nightMusic != null) { nightMusic.loop = true; nightMusic.volume = 0f;   nightMusic.Play(); }
    }

    void Update()
    {
        if (cycle == null) return;
        bool night = cycle.IsNight;

        if (dayMusic != null)
            dayMusic.volume = Mathf.MoveTowards(dayMusic.volume, night ? 0f : volume, fadeSpeed * Time.deltaTime);
        if (nightMusic != null)
            nightMusic.volume = Mathf.MoveTowards(nightMusic.volume, night ? volume : 0f, fadeSpeed * Time.deltaTime);
    }
}
