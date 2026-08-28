using UnityEngine;
using TMPro;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private int carryCapacity = 5;
    [SerializeField] private float fuelPerWood = 10f;
    [SerializeField] private int carriedWood = 0;
    [SerializeField] private int carriedFood = 0;
    [SerializeField] private TMP_Text carryingText;      // world-space text under the player

    [Header("Carry visuals")]
    [SerializeField] private GameObject woodCarryIcon;   // shown while carrying wood
    [SerializeField] private GameObject foodCarryIcon;   // shown while carrying food (placeholder)

    private WoodPile nearbyPile;
    private FoodPile nearbyFoodPile;
    private Bonfire nearbyFire;
    private BerryBush nearbyBush;
    private RecruitPoint nearbyRecruit;
    private WreckedShip nearbyShip;

    public int CarriedWood => carriedWood;
    public int CarriedFood => carriedFood;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) DoInteract();

        if (woodCarryIcon != null) woodCarryIcon.SetActive(carriedWood > 0);
        if (foodCarryIcon != null) foodCarryIcon.SetActive(carriedFood > 0);

        if (carryingText != null)
        {
            string t = "";
            if (carriedWood > 0) t += "Wood " + carriedWood + "  ";
            if (carriedFood > 0) t += "Food " + carriedFood;
            carryingText.text = t;
        }
    }

    // Called by the E key AND the mobile Interact button
    public void DoInteract()
    {
        if (nearbyRecruit != null)                                 // at recruit point → spend food, add crew
        {
            if (nearbyRecruit.TryRecruit())
                SoundManager.Instance?.Recruit(transform.position);
        }
        else if (nearbyFire != null && carriedWood > 0)            // at fire → dump wood in
        {
            nearbyFire.AddFuel(carriedWood * fuelPerWood);
            carriedWood = 0;
            SoundManager.Instance?.Burn(transform.position);
        }
        else if (nearbyShip != null && carriedWood > 0)            // at ship → repair with wood
        {
            nearbyShip.AddWood(carriedWood);
            carriedWood = 0;
            SoundManager.Instance?.WoodPut(transform.position);    // placeholder repair sfx
        }
        else if (nearbyFoodPile != null && carriedFood > 0)        // at food pile → drop food
        {
            nearbyFoodPile.AddFood(carriedFood);
            carriedFood = 0;
            SoundManager.Instance?.FoodDrop(transform.position);
        }
        else if (nearbyPile != null && carriedWood < carryCapacity) // at wood pile → grab wood
        {
            int taken = nearbyPile.TakeWood(1);
            carriedWood += taken;
            if (taken > 0) SoundManager.Instance?.WoodGather(transform.position);
        }
        else if (nearbyBush != null && carriedFood < carryCapacity) // at bush → pick berries
        {
            int picked = nearbyBush.Pick(1);
            carriedFood += picked;
            if (picked > 0) SoundManager.Instance?.BerryPick(transform.position);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out WoodPile pile)) nearbyPile = pile;
        if (other.TryGetComponent(out FoodPile food)) nearbyFoodPile = food;
        if (other.TryGetComponent(out Bonfire fire)) nearbyFire = fire;
        if (other.TryGetComponent(out BerryBush bush)) nearbyBush = bush;
        if (other.TryGetComponent(out RecruitPoint recruit)) nearbyRecruit = recruit;
        if (other.TryGetComponent(out WreckedShip ship)) nearbyShip = ship;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out WoodPile pile) && pile == nearbyPile) nearbyPile = null;
        if (other.TryGetComponent(out FoodPile food) && food == nearbyFoodPile) nearbyFoodPile = null;
        if (other.TryGetComponent(out Bonfire fire) && fire == nearbyFire) nearbyFire = null;
        if (other.TryGetComponent(out BerryBush bush) && bush == nearbyBush) nearbyBush = null;
        if (other.TryGetComponent(out RecruitPoint recruit) && recruit == nearbyRecruit) nearbyRecruit = null;
        if (other.TryGetComponent(out WreckedShip ship) && ship == nearbyShip) nearbyShip = null;
    }
}
