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

    [Header("Night wander")]
    [SerializeField] private Vector2 wanderSize = new Vector2(8f, 6f);   // area NPCs roam around the bonfire

    [Header("Separation")]
    [SerializeField] private float separationRadius = 0.5f;   // how close before NPCs push apart
    [SerializeField] private float separationStrength = 0.8f; // keep below moveSpeed so it never reverses the walk

    [Header("Impostor")]
    [SerializeField] private bool isImpostor = false;
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

    public void Die()
    {
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

    void Awake()
    {
        if (pile == null)  pile  = FindFirstObjectByType<WoodPile>();
        if (cycle == null) cycle = FindFirstObjectByType<DayNightCycle>();

        foreach (Tree t in FindObjectsByType<Tree>(FindObjectsSortMode.None))
            gatherSpots.Add(t.transform);

        currentSpot = PickTree();
        Bonfire fire = FindFirstObjectByType<Bonfire>();
        campCenter = fire != null ? fire.transform.position
                   : pile != null ? pile.transform.position
                   : transform.position;
    }

    void Update()
    {
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

    void OnDrawGizmos()
    {
        Bonfire fire = FindFirstObjectByType<Bonfire>();
        Vector3 center = fire != null ? fire.transform.position : transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(center, new Vector3(wanderSize.x, wanderSize.y, 0f));
    }
}
