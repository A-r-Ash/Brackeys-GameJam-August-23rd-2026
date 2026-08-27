using System;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public enum Phase { Day, Night }

    [SerializeField] private float dayDuration = 30f;
    [SerializeField] private float nightDuration = 20f;

    [SerializeField] private Phase currentPhase = Phase.Day;  // visible in Inspector
    [SerializeField] private int dayNumber = 1;

    private float timer;

    // Other systems subscribe to these
    public event Action OnDayStart;
    public event Action OnNightStart;

    public Phase CurrentPhase => currentPhase;
    public int DayNumber => dayNumber;
    public bool IsNight => currentPhase == Phase.Night;

    public float PhaseTimeRemaining => (IsNight ? nightDuration : dayDuration) - timer;
    public float PhaseProgress => Mathf.Clamp01(timer / (IsNight ? nightDuration : dayDuration));  // 0→1 through the phase

    void Start()
    {
        timer = 0f;
        currentPhase = Phase.Day;
        OnDayStart?.Invoke();
    }

    void Update()
    {
        timer += Time.deltaTime;
        float duration = IsNight ? nightDuration : dayDuration;

        if (timer >= duration)
        {
            timer = 0f;
            if (currentPhase == Phase.Day)
            {
                currentPhase = Phase.Night;
                Debug.Log("Night falls on day " + dayNumber);
                OnNightStart?.Invoke();
            }
            else
            {
                currentPhase = Phase.Day;
                dayNumber++;
                Debug.Log("Day " + dayNumber + " begins");
                OnDayStart?.Invoke();
            }
        }
    }
}