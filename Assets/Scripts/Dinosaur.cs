using UnityEngine;

// Wanders its territory; chases the player when close. Tramples NPCs it touches.
// Dies (giving food) when it hits a trap.
public class Dinosaur : MonoBehaviour
{
    [SerializeField] private int foodValue = 2;         // big = 5, small = 2
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float detectRange = 5f;    // starts chasing the player within this
    [SerializeField] private float wanderRadius = 4f;
    [SerializeField] private GameObject deathVfx;        // optional death animation prefab

    private Transform player;
    private FoodPile foodPile;
    private Vector3 homeCenter;
    private Vector3 wanderTarget;

    void Awake()
    {
        homeCenter = transform.position;
        foodPile = FindFirstObjectByType<FoodPile>();

        PlayerInteract p = FindFirstObjectByType<PlayerInteract>();
        if (p != null) player = p.transform;

        PickWander();
    }

    void Update()
    {
        if (player != null && Vector2.Distance(transform.position, player.position) < detectRange)
        {
            MoveTo(player.position);            // chase
        }
        else if (MoveTo(wanderTarget))
        {
            PickWander();                       // reached wander point → pick a new one
        }
    }

    bool MoveTo(Vector3 target)
    {
        transform.position = Vector2.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
        return Vector2.Distance(transform.position, target) < 0.1f;
    }

    void PickWander()
    {
        Vector2 r = Random.insideUnitCircle * wanderRadius;
        wanderTarget = homeCenter + new Vector3(r.x, r.y, 0f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Trap trap))
        {
            if (foodPile != null) foodPile.AddFood(foodValue);   // caught → food to the pile
            if (deathVfx != null) Instantiate(deathVfx, transform.position, Quaternion.identity);
            trap.Spring();
            Destroy(gameObject);
        }
        else if (other.TryGetComponent(out NPCGatherer npc))
        {
            npc.Die();                                            // trampled a crew member
        }
    }
}
