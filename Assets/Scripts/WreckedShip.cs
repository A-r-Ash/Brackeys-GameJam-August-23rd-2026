using UnityEngine;
using TMPro;

// The escape goal: deposit wood until it's fixed, then you win.
public class WreckedShip : MonoBehaviour
{
    [SerializeField] private int woodNeeded = 500;
    [SerializeField] private int woodDeposited = 0;
    [SerializeField] private TMP_Text progressText;   // shows "X / 500"

    public int WoodNeeded => woodNeeded;
    public int WoodDeposited => woodDeposited;
    public float Progress => woodNeeded > 0 ? (float)woodDeposited / woodNeeded : 0f;
    public bool IsFixed => woodDeposited >= woodNeeded;

    void Start()
    {
        UpdateText();
    }

    // Player deposits carried wood here
    public void AddWood(int amount)
    {
        if (IsFixed) return;

        woodDeposited = Mathf.Min(woodDeposited + amount, woodNeeded);
        UpdateText();

        if (IsFixed)
            GameStateManager.Instance?.Win();   // ship fixed → escape!
    }

    void UpdateText()
    {
        if (progressText != null)
            progressText.text = woodDeposited + " / " + woodNeeded;
    }
}
