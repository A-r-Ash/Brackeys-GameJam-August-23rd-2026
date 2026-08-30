using UnityEngine;

// Wanders its territory; chases the player when close. Tramples NPCs it touches.
// Dies (giving food) when it hits a trap.
public class Dinosaur : MonoBehaviour
{
    [SerializeField] private int foodValue = 2;         // big = 5, small = 2
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float detectRange = 5f;    // starts chasing the player within this
    [SerializeField] private float wanderRadius = 4f;
    [SerializeField] private GameObject deathVfx;        // optional: extra death particle prefab

    [Header("Animation")]
    [SerializeField] private Animator animator;           // walk + death animator
    [SerializeField] private SpriteRenderer sprite;       // flipped to face the walk direction

    [Header("Audio")]
    [SerializeField] private AudioSource walkSource;      // looping "dino walk" clip (assign clip + loop in editor)
    [SerializeField] private float footstepInterval = 0.5f;
    [SerializeField] private bool smallDino;              // screech (small) instead of roar (big)

    [Header("Death")]
    [SerializeField] private string deathTrigger = "Die"; // animator trigger for the death animation
    [SerializeField] private float destroyDelay = 1f;     // how long to let the death animation play

    private Transform player;
    private FoodPile foodPile;
    private Vector3 homeCenter;
    private Vector3 wanderTarget;

    private bool dead;
    private bool chasing;
    private bool dormant;             // side-camp dino sleeps until Camp.Activate()
    private float footstepTimer;
    private Vector3 lastPos;

    public static int CaughtCount { get; private set; }

    public void SetDormant(bool value) => dormant = value;

    public void Activate() => SetDormant(false);

    // Binds this dino to a specific food pile (its own camp's) instead of the first one found.
    public void SetFoodPile(FoodPile p)
    {
        if (p != null) foodPile = p;
    }

    void Awake()
    {
        homeCenter = transform.position;

        // Drop food at the nearest camp's pile, not the first one found.
        FoodPile bestPile = null;
        float bestD = float.MaxValue;
        foreach (FoodPile fp in FindObjectsByType<FoodPile>(FindObjectsSortMode.None))
        {
            float d = Vector2.SqrMagnitude(fp.transform.position - transform.position);
            if (d < bestD) { bestD = d; bestPile = fp; }
        }
        if (bestPile != null) foodPile = bestPile;

        PlayerInteract p = FindFirstObjectByType<PlayerInteract>();
        if (p != null) player = p.transform;

        PickWander();
    }

    void Update()
    {
        if (dead || dormant) return;   // dormant side-camp dinos wait for activation

        bool isChasing = player != null && Vector2.Distance(transform.position, player.position) < detectRange;
        if (isChasing)
        {
            if (!chasing) Vocalize();             // roar/screech the first moment it spots you
            MoveTo(player.position);
        }
        else if (MoveTo(wanderTarget))
        {
            PickWander();                         // reached wander point → pick a new one
        }
        chasing = isChasing;

        UpdateAnimation();
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

    void UpdateAnimation()
    {
        Vector3 delta = transform.position - lastPos;
        lastPos = transform.position;
        bool moving = delta.magnitude / Time.deltaTime > 0.01f;

        if (animator != null) animator.SetBool("IsMoving", moving);

        // Face the direction of travel (sprite is authored facing left)
        if (sprite != null && Mathf.Abs(delta.x) > 0.0001f)
            sprite.flipX = delta.x > 0f;

        if (moving)
        {
            if (walkSource != null && !walkSource.isPlaying) walkSource.Play();
            footstepTimer += Time.deltaTime;
            if (footstepTimer >= footstepInterval)
            {
                footstepTimer = 0f;
                SoundManager.Instance?.DinoFootstep(transform.position);
            }
        }
        else if (walkSource != null && walkSource.isPlaying)
        {
            walkSource.Pause();
        }
    }

    void Vocalize()
    {
        if (smallDino) SoundManager.Instance?.DinoScreech(transform.position);
        else           SoundManager.Instance?.DinoRoar(transform.position);
    }

    void Die()
    {
        if (dead) return;
        dead = true;
        CaughtCount++;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (walkSource != null) walkSource.Stop();
        if (animator != null) animator.SetTrigger(deathTrigger);

        if (foodPile != null) foodPile.AddFood(foodValue);
        FloatingText.Show(transform.position, "+" + foodValue + " Food", FloatingText.FoodColor);
        if (deathVfx != null) Instantiate(deathVfx, transform.position, Quaternion.identity);

        SoundManager.Instance?.DinoCaught(transform.position);
        SoundManager.Instance?.DinoDeath(transform.position);

        Destroy(gameObject, destroyDelay);          // let the death animation finish first
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Trap trap))
        {
            trap.Spring();
            Die();                                    // caught → food to the pile
        }
        else if (other.TryGetComponent(out NPCGatherer npc))
        {
            npc.Die();                                // trampled a crew member
        }
    }

    void OnDrawGizmos()
    {
        // Detection / chase range (red) — around the dino's current position
        Gizmos.color = new Color(1f, 0.3f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, detectRange);

        // Wander range (cyan) — around home (its spawn point)
        Vector3 center = Application.isPlaying ? homeCenter : transform.position;
        Gizmos.color = new Color(0.3f, 0.8f, 1f);
        Gizmos.DrawWireSphere(center, wanderRadius);
    }
}