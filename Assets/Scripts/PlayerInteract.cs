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

    [Header("Trap building")]
    [SerializeField] private GameObject trapPrefab;      // trap dropped at the player's feet
    [SerializeField] private int trapCost = 5;           // carried wood spent per trap
    [SerializeField] private KeyCode placeTrapKey = KeyCode.T;

    private bool requireTrapZone;                        // tutorial: only place inside a marked TrapZone
    private TrapZone nearbyTrapZone;
    private int trapsPlaced;

    private WoodPile nearbyPile;
    private FoodPile nearbyFoodPile;
    private Bonfire nearbyFire;
    private BerryBush nearbyBush;
    private RecruitPoint nearbyRecruit;
    private WreckedShip nearbyShip;

    public int CarriedWood => carriedWood;
    public int CarriedFood => carriedFood;
    public int TrapsPlaced => trapsPlaced;
    public bool InTrapZone => nearbyTrapZone != null;
    public void GiveWood(int n) { carriedWood += n; }
    public void SetRequireTrapZone(bool v) { requireTrapZone = v; }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) DoInteract();
        if (Input.GetKeyDown(placeTrapKey)) PlaceTrap();

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
            int added = carriedWood;
            nearbyShip.AddWood(added);
            carriedWood = 0;
            SoundManager.Instance?.WoodPut(transform.position);    // placeholder repair sfx
            FloatingText.Show(nearbyShip.transform.position, "+" + added + " Wood", FloatingText.WoodColor);
        }
        else if (nearbyFoodPile != null && carriedFood > 0)        // at food pile → drop food
        {
            int dropped = carriedFood;
            nearbyFoodPile.AddFood(dropped);
            carriedFood = 0;
            SoundManager.Instance?.FoodDrop(transform.position);
            FloatingText.Show(nearbyFoodPile.transform.position, "+" + dropped + " Food", FloatingText.FoodColor);
        }
        else if (nearbyPile != null && carriedWood < carryCapacity) // at wood pile → grab wood
        {
            int taken = nearbyPile.TakeWood(1);
            carriedWood += taken;
            if (taken > 0)
            {
                SoundManager.Instance?.WoodGather(transform.position);
                FloatingText.Show(nearbyPile.transform.position, "+" + taken + " Wood", FloatingText.WoodColor);
            }
        }
        else if (nearbyBush != null && carriedFood < carryCapacity) // at bush → pick berries
        {
            int picked = nearbyBush.Pick(1);
            carriedFood += picked;
            if (picked > 0)
            {
                SoundManager.Instance?.BerryPick(transform.position);
                FloatingText.Show(nearbyBush.transform.position, "+" + picked + " Food", FloatingText.FoodColor);
            }
        }
    }

    // Live check: is the player's position inside any TrapZone right now?
    bool InsideTrapZone()
    {
        foreach (Collider2D c in Physics2D.OverlapPointAll(transform.position))
            if (c.GetComponentInParent<TrapZone>() != null)
                return true;
        return false;
    }

    // Called by the T key AND a mobile "build trap" button, drops a trap at the player, spending wood
    public void PlaceTrap()
    {
        if (trapPrefab == null) return;

        if (requireTrapZone && !InsideTrapZone())
        {
            FloatingText.Show(transform.position, "Place it on the marked spot", Color.yellow);
            return;
        }

        if (carriedWood < trapCost)
        {
            FloatingText.Show(transform.position, "Need " + trapCost + " Wood", FloatingText.WoodColor);
            return;
        }

        carriedWood -= trapCost;
        trapsPlaced++;
        Instantiate(trapPrefab, transform.position, Quaternion.identity);
        FloatingText.Show(transform.position, "Trap set!", FloatingText.WoodColor);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out WoodPile pile)) nearbyPile = pile;
        if (other.TryGetComponent(out FoodPile food)) nearbyFoodPile = food;
        if (other.TryGetComponent(out Bonfire fire)) nearbyFire = fire;
        if (other.TryGetComponent(out BerryBush bush)) nearbyBush = bush;
        if (other.TryGetComponent(out RecruitPoint recruit)) nearbyRecruit = recruit;
        if (other.TryGetComponent(out WreckedShip ship)) nearbyShip = ship;

        TrapZone zone = other.GetComponentInParent<TrapZone>();
        if (zone != null) nearbyTrapZone = zone;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out WoodPile pile) && pile == nearbyPile) nearbyPile = null;
        if (other.TryGetComponent(out FoodPile food) && food == nearbyFoodPile) nearbyFoodPile = null;
        if (other.TryGetComponent(out Bonfire fire) && fire == nearbyFire) nearbyFire = null;
        if (other.TryGetComponent(out BerryBush bush) && bush == nearbyBush) nearbyBush = null;
        if (other.TryGetComponent(out RecruitPoint recruit) && recruit == nearbyRecruit) nearbyRecruit = null;
        if (other.TryGetComponent(out WreckedShip ship) && ship == nearbyShip) nearbyShip = null;

        TrapZone zone = other.GetComponentInParent<TrapZone>();
        if (zone != null && zone == nearbyTrapZone) nearbyTrapZone = null;
    }
}
