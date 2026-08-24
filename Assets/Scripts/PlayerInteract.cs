using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private int carryCapacity = 5;
    [SerializeField] private float fuelPerWood = 10f;
    [SerializeField] private int carriedWood = 0;   // visible so you can watch it

    private WoodPile nearbyPile;
    private Bonfire nearbyFire;

    public int CarriedWood => carriedWood;

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.E)) return;

        // At the fire with wood in hand → dump it in
        if (nearbyFire != null && carriedWood > 0)
        {
            nearbyFire.AddFuel(carriedWood * fuelPerWood);
            carriedWood = 0;
        }
        // At the pile → grab as much as we can carry
        else if (nearbyPile != null)
        {
            int space = carryCapacity - carriedWood;
            if (space > 0)
                carriedWood += nearbyPile.TakeWood(space);
        }
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