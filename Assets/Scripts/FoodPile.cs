using UnityEngine;

public class FoodPile : MonoBehaviour
{
    [SerializeField] private int foodCount = 0;

    public void AddFood(int amount)
    {
        foodCount += amount;
    }

    // Try to spend food (e.g. recruiting). Returns true if there was enough.
    public bool TrySpend(int amount)
    {
        if (foodCount < amount) return false;
        foodCount -= amount;
        return true;
    }

    public int Count => foodCount;
}
