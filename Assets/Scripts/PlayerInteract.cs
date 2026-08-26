using UnityEngine;
using TMPro;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private int carryCapacity = 5;
    [SerializeField] private float fuelPerWood = 10f;
    [SerializeField] private int carriedWood = 0;
    [SerializeField] private TMP_Text carryingText;   // world-space text under the player

    private WoodPile nearbyPile;
    private Bonfire nearbyFire;

    public int CarriedWood => carriedWood;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (nearbyFire != null && carriedWood > 0)          // at fire → dump wood in
            {
                nearbyFire.AddFuel(carriedWood * fuelPerWood);
                carriedWood = 0;
            }
            else if (nearbyPile != null)                        // at pile → grab wood
            {
                if (carriedWood < carryCapacity)
                {
                    carriedWood += nearbyPile.TakeWood(1);
                }
            }
        }

        // Show text only while carrying, blank otherwise
        if (carryingText != null)
            carryingText.text = carriedWood > 0 ? "Carrying: " + carriedWood : "";
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out WoodPile pile)) nearbyPile = pile;
        if (other.TryGetComponent(out Bonfire fire)) nearbyFire = fire;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out WoodPile pile) && pile == nearbyPile) nearbyPile = null;
        if (other.TryGetComponent(out Bonfire fire) && fire == nearbyFire) nearbyFire = null;
    }
}