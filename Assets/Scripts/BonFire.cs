using UnityEngine;

public class Bonfire : MonoBehaviour
{
    [SerializeField] private float maxFuel = 100f;
    [SerializeField] private float drainPerSecond = 2f;
    [SerializeField] private float currentFuel;   // visible in Inspector so you can watch it drain

    public float CurrentFuel => currentFuel;
    public float MaxFuel => maxFuel;

    void Start()
    {
        currentFuel = maxFuel;
    }

    void Update()
    {
        currentFuel -= drainPerSecond * Time.deltaTime;

        if (currentFuel <= 0f)
        {
            currentFuel = 0f;
            Debug.Log("The fire went out! Game Over.");
            // TODO: trigger real game over later
        }
    }

    // Player calls this when feeding wood to the fire
    public void AddFuel(float amount)
    {
        currentFuel = Mathf.Min(currentFuel + amount, maxFuel);
    }
}