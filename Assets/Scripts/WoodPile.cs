using UnityEngine;

public class WoodPile : MonoBehaviour
{
    [SerializeField] private int woodCount = 0;   // watch it grow in the Inspector

    

    public void AddWood(int amount)
    {
        woodCount += amount;
    }

    // Player (and later the impostor) will call this to grab wood
    public int TakeWood(int amount)
    {
        int taken = Mathf.Min(amount, woodCount);
        woodCount -= taken;
        return taken;
    }

    public int Count => woodCount;
}