using UnityEngine;

// Groups a camp's piles, recruit point and bonfire. A non-main camp stays off
// until the player first enters its area, then it switches on.
// Dinosaurs are NOT part of a camp - they roam freely wherever they want.
//
// Recruited NPC crew live in their camp automatically: the recruit point gives
// each new NPC this camp's woodpile + bonfire, so they gather and stay there.
public class Camp : MonoBehaviour
{
    [Header("Camp")]
    [SerializeField] private bool isMainCamp;
    [SerializeField] private float activationRadius = 8f;

    [Header("Members")]
    [SerializeField] private WoodPile woodPile;
    [SerializeField] private FoodPile foodPile;
    [SerializeField] private RecruitPoint recruit;
    [SerializeField] private Bonfire bonfire;

    private bool active;

    public bool IsMainCamp => isMainCamp;
    public bool IsActive => active;
    public WoodPile WoodPile => woodPile;
    public FoodPile FoodPile => foodPile;
    public Bonfire Bonfire => bonfire;

    void Start()
    {
        if (recruit != null)
        {
            recruit.SetNpcWoodPile(woodPile);
            recruit.SetCampBonfire(bonfire);
            recruit.SetCampActive(isMainCamp);
        }

        if (isMainCamp) Activate();
    }

    void Update()
    {
        if (active) return;

        PlayerInteract player = FindFirstObjectByType<PlayerInteract>();
        if (player == null) return;

        if (Vector2.Distance(transform.position, player.transform.position) <= activationRadius)
            Activate();
    }

    public void Activate()
    {
        if (active) return;
        active = true;

        if (bonfire != null)
        {
            bonfire.CampActivate();
            if (SoundManager.Instance != null)
                SoundManager.Instance.Burn(bonfire.transform.position);
        }

        if (recruit != null) recruit.SetCampActive(true);
    }
}