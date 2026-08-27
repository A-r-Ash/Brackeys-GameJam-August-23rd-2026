using UnityEngine;
using UnityEngine.UI;

// The escape goal: deposit wood until it's fixed, then you win.
public class WreckedShip : MonoBehaviour
{
    [SerializeField] private int woodNeeded = 500;
    [SerializeField] private int woodDeposited = 0;
    [SerializeField] private Image progressBar;   // optional fill bar (0..1)

    public int WoodNeeded => woodNeeded;
    public int WoodDeposited => woodDeposited;
    public float Progress => woodNeeded > 0 ? (float)woodDeposited / woodNeeded : 0f;
    public bool IsFixed => woodDeposited >= woodNeeded;

    void Start()
    {
        if (progressBar != null) progressBar.fillAmount = Progress;
    }

    // Player deposits carried wood here
    public void AddWood(int amount)
    {
        if (IsFixed) return;

        woodDeposited = Mathf.Min(woodDeposited + amount, woodNeeded);
        if (progressBar != null) progressBar.fillAmount = Progress;

        if (IsFixed)
            GameStateManager.Instance?.Win();   // ship fixed → escape!
    }
}
