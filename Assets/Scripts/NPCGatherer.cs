using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NPCGatherer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WoodPile pile;
    [SerializeField] private DayNightCycle cycle;
    [SerializeField] private TMP_Text stateText;
    [SerializeField] private GameObject carryIcon;   // shown while the NPC is carrying wood

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;

    [Header("Day gathering")]
    [SerializeField] private float gatherTime = 2f;
    [SerializeField] private int woodPerTrip = 1;
    [SerializeField] private float gatherRadius = 1000f;   // only scavenge trees this close to the camp fire

    [Header("Night wander")]
    [SerializeField] private Vector2 wanderSize = new Vector2(8f, 6f);   // area NPCs roam around the bonfire

    [Header("Separation")]
    [SerializeField] private float separationRadius = 0.5f;   // how close before NPCs push apart
    [SerializeField] private float separationStrength = 0.8f; // keep below moveSpeed so it never reverses the walk

    [Header("Impostor")]
    [SerializeField] private bool isImpostor = false;

    [Header("Camp")]
    [SerializeField] private bool dormant;            // sleeping side-camp member until Camp.Activate()
    [SerializeField] private float sabotageInterval = 6f;  // wander time between thefts
    [SerializeField] private int stealAmount = 3;          // wood taken per theft
    [SerializeField] private float stealDuration = 2f;     // time spent stealing at the pile (catch window)

    [Header("Death")]
    [SerializeField] private GameObject innocentDeathVfx;
    [SerializeField] private GameObject impostorDeathVfx;

    public static int Count { get; private set; }
    public static int ImpostorCount { get; private set; }

    public enum State { GoingToGather, Gathering, ReturningToPile, NightDepositWood, GoToBonfire, Wandering, Stealing, StealingAtPile }
    private State state = State.GoingToGather;
    public State CurrentState => state;
    public bool IsImpostor => isImpostor;
    public void SetImpostor(bool value)
    {
        if (isImpostor == value) return;
        isImpostor = value;
        if (isActiveAndEnabled) ImpostorCount += value ? 1 : -1;
    }

    // Side-camp NPCs sleep until their camp is found.
    public void SetDormant(bool value)
    {
        dormant = value;
        if (dormant && carryIcon != null) carryIcon.SetActive(false);
    }

    public void WakeCampMember() => SetDormant(false);

    // Binds this NPC to a specific woodpile (its own camp's) instead of the first one found.
    public void SetPile(WoodPile p)
    {
        if (p != null) pile = p;
    }

    public void Die()
    {
        if (dormant) return;   // sleeping crew can't be trampled
        GameObject vfx = isImpostor ? impostorDeathVfx : innocentDeathVfx;
        if (vfx != null) Instantiate(vfx, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    private List<Transform> gatherSpots = new List<Transform>();
    private Transform currentSpot;
    private float gatherTimer;

    private Vector3 wanderTarget;
    private Vector3 campCenter;
    private float sabotageTimer;
    private float stealTimer;
    private bool carryingWood;
    private bool wasNight;

    void OnEnable()
    {
        Count++;
        if (isImpostor) ImpostorCount++;
    }

    void OnDisable()
    {
        Count--;
        if (isImpostor) ImpostorCount--;
    }

    static T Nearest<T>(Vector3 from) where T : Component
    {
        T best = null;
        float bestD = float.MaxValue;
        foreach (T t in FindObjectsByType<T>(FindObjectsSortMode.None))
        {
            float d = Vector2.SqrMagnitude((Vector2)t.transform.position - (Vector2)from);
            if (d < bestD) { bestD = d; best = t; }
        }
        return best;
    }

    void Awake()
    {
        if (cycle == null) cycle = FindFirstObjectByType<DayNightCycle>();
        if (pile == null)  pile  = Nearest<WoodPile>(transform.position);

        Bonfire fire = Nearest<Bonfire>(transform.position);
        campCenter = fire != null ? fire.transform.position
                   : pile != null ? pile.transform.position
                   : transform.position;

        RebindTrees();

        currentSpot = PickTree();
    }

    // Collect the trees this NPC may harvest, limited to the camp's area.
    void RebindTrees()
    {
        gatherSpots.Clear();
        foreach (Tree t in FindObjectsByType<Tree>(FindObjectsSortMode.None))
            if (Vector2.Distance(campCenter, t.transform.position) <= gatherRadius)
                gatherSpots.Add(t.transform);
    }

    // Bind this NPC to a specific camp bonfire - the crew then gathers wood to,
    // and wanders around, THIS camp instead of the first fire found.
    public void SetBonfire(Bonfire fire)
    {
        if (fire == null) return;
        campCenter = fire.transform.position;
        RebindTrees();
    }

    void Update()
    {
        if (dormant) return;   // side-camp crew do nothing until their camp is activated

        bool night = cycle != null && cycle.IsNight;

        if (night != wasNight)
        {
            wasNight = night;
            if (night)
            {
                // Carrying wood? drop it at the pile first. Otherwise straight to the fire.
                state = carryingWood ? State.NightDepositWood : State.GoToBonfire;
                sabotageTimer = sabotageInterval;
                if (!isImpostor) SoundManager.Instance?.Mumble(transform.position);
            }
            else
            {
                state = State.GoingToGather;
                currentSpot = PickTree();
            }
        }

        if (night) NightUpdate();
        else       DayUpdate();

        ApplySeparation();

        if (carryIcon != null) carryIcon.SetActive(carryingWood);

        if (stateText != null) stateText.text = state.ToString();
    }

    // ---------- DAY: gather wood from trees, drop at the pile ----------
    void DayUpdate()
    {
        if (currentSpot == null) return;

        switch (state)
        {
            case State.GoingToGather:
                if (MoveTo(currentSpot.position))
                {
                    gatherTimer = gatherTime;
                    state = State.Gathering;
                    SoundManager.Instance?.WoodCut(transform.position);
                }
                break;

            case State.Gathering:
                gatherTimer -= Time.deltaTime;
                if (gatherTimer <= 0f)
                {
                    carryingWood = true;
                    state = State.ReturningToPile;
                }
                break;

            case State.ReturningToPile:
                if (MoveTo(pile.transform.position))
                {
                    pile.AddWood(woodPerTrip);
                    SoundManager.Instance?.WoodPut(transform.position);
                    FloatingText.Show(pile.transform.position, "+" + woodPerTrip + " Wood", FloatingText.WoodColor);
                    carryingWood = false;
                    currentSpot = PickTree();
                    state = State.GoingToGather;
                }
                break;
        }
    }

    // ---------- NIGHT: gather at the bonfire and wander; impostor steals from the pile ----------
    void NightUpdate()
    {
        switch (state)
        {
            case State.NightDepositWood:                 // carrying wood → drop at the pile first
                if (MoveTo(pile.transform.position))
                {
                    pile.AddWood(woodPerTrip);
                    SoundManager.Instance?.WoodPut(transform.position);
                    FloatingText.Show(pile.transform.position, "+" + woodPerTrip + " Wood", FloatingText.WoodColor);
                    carryingWood = false;
                    state = State.GoToBonfire;
                }
                break;

            case State.GoToBonfire:                      // head to the fire, then wander around it
                if (MoveTo(campCenter))
                {
                    PickWanderTarget();
                    state = State.Wandering;
                }
                break;

            case State.Wandering:
                if (MoveTo(wanderTarget))
                    PickWanderTarget();

                if (isImpostor)
                {
                    sabotageTimer -= Time.deltaTime;
                    if (sabotageTimer <= 0f)
                    {
                        state = State.Stealing;
                        SoundManager.Instance?.ZombieGrowl(transform.position);
                    }
                }
                break;

            case State.Stealing:                         // walk to the pile
                if (MoveTo(pile.transform.position))
                {
                    stealTimer = stealDuration;
                    state = State.StealingAtPile;
                }
                break;

            case State.StealingAtPile:                   // linger and steal (catch window), then slip away
                stealTimer -= Time.deltaTime;
                if (stealTimer <= 0f)
                {
                    pile.TakeWood(stealAmount);
                    sabotageTimer = sabotageInterval;
                    PickWanderTarget();
                    state = State.Wandering;
                }
                break;
        }
    }

    // Soft push so cavemen don't stack on the same spot
    void ApplySeparation()
    {
        Vector2 push = Vector2.zero;
        foreach (Collider2D c in Physics2D.OverlapCircleAll(transform.position, separationRadius))
        {
            if (c.gameObject == gameObject) continue;
            if (c.TryGetComponent(out NPCGatherer other))
            {
                Vector2 away = (Vector2)(transform.position - other.transform.position);
                float d = away.magnitude;
                if (d > 0.001f)
                    push += away.normalized * (1f - d / separationRadius);   // stronger the closer they are
            }
        }
        if (push == Vector2.zero) return;
        push = Vector2.ClampMagnitude(push, 1f);   // multiple neighbors can't stack into a huge shove
        transform.position += (Vector3)(push * separationStrength * Time.deltaTime);
    }

    void PickWanderTarget()
    {
        float x = Random.Range(-wanderSize.x * 0.5f, wanderSize.x * 0.5f);
        float y = Random.Range(-wanderSize.y * 0.5f, wanderSize.y * 0.5f);
        wanderTarget = campCenter + new Vector3(x, y, 0f);
    }

    private Transform PickTree()
    {
        if (gatherSpots.Count == 0) return null;
        return gatherSpots[Random.Range(0, gatherSpots.Count)];
    }

    private bool MoveTo(Vector3 target)
    {
        transform.position = Vector2.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
        return Vector2.Distance(transform.position, target) < 0.05f;
    }

    // Editor/scene-view center: the bound camp fire when known, else the first fire found.
    Vector3 GetDisplayCenter()
    {
        if (campCenter != Vector3.zero) return campCenter;
        Bonfire fire = FindFirstObjectByType<Bonfire>();
        return fire != null ? fire.transform.position : transform.position;
    }

    void OnDrawGizmos()
    {
        Vector3 center = GetDisplayCenter();
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(center, new Vector3(wanderSize.x, wanderSize.y, 0f));
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.3f, 1f, 0.4f, 0.85f);
        Gizmos.DrawWireSphere(GetDisplayCenter(), gatherRadius);
    }
}
