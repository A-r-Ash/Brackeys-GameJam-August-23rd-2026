using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NPCGatherer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WoodPile pile;
    [SerializeField] private DayNightCycle cycle;
    [SerializeField] private TMP_Text stateText;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private LayerMask wallLayer;   // walls to route around — NPCs detour through entrances

    [Header("Day gathering")]
    [SerializeField] private float gatherTime = 2f;
    [SerializeField] private int woodPerTrip = 1;
    [SerializeField] private float stuckTime = 1.5f;   // no-movement time in GoingToGather before resetting

    [Header("Night wander")]
    [SerializeField] private Vector2 wanderSize = new Vector2(8f, 6f);   // rectangle (width, height) NPCs roam in

    [Header("Impostor")]
    [SerializeField] private bool isImpostor = false;
    [SerializeField] private float sabotageInterval = 6f;  // wander time between thefts
    [SerializeField] private int stealAmount = 3;          // wood taken per theft
    [SerializeField] private float stealDuration = 2f;     // time spent stealing at the pile (catch window)

    public static int Count { get; private set; }

    public enum State { GoingToGather, Gathering, ReturningToPile, GoToEntrance, NightDepositWood, GoToBonfire, Wandering, Stealing, StealingAtPile, Dumping }
    private State state = State.GoingToGather;
    public State CurrentState => state;
    public bool IsImpostor => isImpostor;
    public void SetImpostor(bool value) { isImpostor = value; }

    private List<Transform> gatherSpots = new List<Transform>();
    private List<Transform> entrances = new List<Transform>();
    private Transform currentSpot;
    private Transform dumpEntrance;
    private Transform nightEntrance;
    private float gatherTimer;

    private Vector3 wanderTarget;
    private Vector3 campCenter;
    private float sabotageTimer;
    private float stealTimer;
    private int carriedStolen;
    private bool carryingWood;
    private bool wasNight;
    private Vector3 lastStuckPos;
    private float stuckTimer;

    void OnEnable()  { Count++; }
    void OnDisable() { Count--; }

    void Awake()
    {
        // Auto-find scene refs so runtime-spawned NPCs work without manual wiring
        if (pile == null)  pile  = FindFirstObjectByType<WoodPile>();
        if (cycle == null) cycle = FindFirstObjectByType<DayNightCycle>();

        foreach (Tree t in FindObjectsByType<Tree>(FindObjectsSortMode.None))
            gatherSpots.Add(t.transform);

        foreach (Entrance e in FindObjectsByType<Entrance>(FindObjectsSortMode.None))
            entrances.Add(e.transform);

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
                // Head in through the nearest gate first, then deposit/fire
                nightEntrance = NearestEntrance();
                state = nightEntrance != null ? State.GoToEntrance
                      : carryingWood ? State.NightDepositWood
                      : State.GoToBonfire;
                sabotageTimer = sabotageInterval;
                if (!isImpostor) SoundManager.Instance?.Mumble();   // innocent crew murmurs at night
            }
            else
            {
                state = State.GoingToGather;      // everyone heads out to gather
                currentSpot = PickTree();
            }
        }

        if (night) NightUpdate();
        else       DayUpdate();

        // Stuck safety net: if a "go somewhere" state stalls, reset it
        bool travelling = state == State.GoingToGather
                       || state == State.GoToBonfire
                       || state == State.NightDepositWood;

        float moved = (transform.position - lastStuckPos).magnitude;
        bool notMoving = moved < moveSpeed * 0.1f * Time.deltaTime;   // moved <10% of expected step → frozen

        if (travelling && notMoving)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer >= stuckTime)
            {
                ResetStuck();
                stuckTimer = 0f;
            }
        }
        else stuckTimer = 0f;
        lastStuckPos = transform.position;

        if (stateText != null) stateText.text = state.ToString();
    }

    // ---------- DAY: everyone gathers wood outside ----------
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
                    SoundManager.Instance?.WoodCut();   // chopping the tree
                }
                break;

            case State.Gathering:
                gatherTimer -= Time.deltaTime;
                if (gatherTimer <= 0f)
                {
                    carryingWood = true;             // chopped → now carrying it back
                    state = State.ReturningToPile;
                }
                break;

            case State.ReturningToPile:
                if (MoveTo(pile.transform.position))
                {
                    pile.AddWood(woodPerTrip);
                    SoundManager.Instance?.WoodPut();   // dropping wood in the pile
                    carryingWood = false;
                    currentSpot = PickTree();
                    state = State.GoingToGather;
                }
                break;
        }
    }

    // ---------- NIGHT: everyone wanders inside; impostor steals & dumps at an exit ----------
    void NightUpdate()
    {
        switch (state)
        {
            case State.GoToEntrance:                     // come in through the nearest gate first
                if (nightEntrance == null || MoveTo(nightEntrance.position))
                    state = carryingWood ? State.NightDepositWood : State.GoToBonfire;
                break;

            case State.NightDepositWood:                 // carrying wood → drop it at the pile first
                if (MoveTo(pile.transform.position))
                {
                    pile.AddWood(woodPerTrip);
                    SoundManager.Instance?.WoodPut();
                    carryingWood = false;
                    state = State.GoToBonfire;
                }
                break;

            case State.GoToBonfire:                      // head to the fire, then start wandering around it
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
                        SoundManager.Instance?.ZombieGrowl();   // impostor slinks off to steal
                    }
                }
                break;

            case State.Stealing:                         // walk to the pile
                if (MoveTo(pile.transform.position))
                {
                    stealTimer = stealDuration;          // arrived → start stealing
                    state = State.StealingAtPile;
                }
                break;

            case State.StealingAtPile:                   // stand at the pile and steal over time (catch window)
                stealTimer -= Time.deltaTime;
                if (stealTimer <= 0f)
                {
                    carriedStolen = pile.TakeWood(stealAmount);
                    dumpEntrance = PickEntrance();
                    if (dumpEntrance != null)
                    {
                        state = State.Dumping;
                    }
                    else
                    {
                        carriedStolen = 0;               // no exit found, bail
                        sabotageTimer = sabotageInterval;
                        state = State.Wandering;
                    }
                }
                break;

            case State.Dumping:                          // carry to an exit, drop it (gone for good)
                if (MoveTo(dumpEntrance.position))
                {
                    carriedStolen = 0;
                    sabotageTimer = sabotageInterval;
                    PickWanderTarget();
                    state = State.Wandering;
                }
                break;
        }
    }

    void PickWanderTarget()
    {
        // Keep wander points inside the walls: a valid point has no wall between it and the bonfire
        for (int attempt = 0; attempt < 10; attempt++)
        {
            float x = Random.Range(-wanderSize.x * 0.5f, wanderSize.x * 0.5f);
            float y = Random.Range(-wanderSize.y * 0.5f, wanderSize.y * 0.5f);
            Vector3 candidate = campCenter + new Vector3(x, y, 0f);

            if (!WallBetween(campCenter, candidate))
            {
                wanderTarget = candidate;
                return;
            }
        }
        wanderTarget = campCenter;   // fallback: hug the fire
    }

    void ResetStuck()
    {
        switch (state)
        {
            case State.GoingToGather:
                currentSpot = PickTree();            // new tree to head for
                break;

            case State.GoToBonfire:
            case State.NightDepositWood:
                nightEntrance = NearestEntrance();   // re-enter through the nearest gate
                state = nightEntrance != null ? State.GoToEntrance
                      : carryingWood ? State.NightDepositWood
                      : State.GoToBonfire;
                break;
        }
    }

    private Transform NearestEntrance()
    {
        Transform best = null;
        float bestDist = Mathf.Infinity;
        foreach (Transform e in entrances)
        {
            float d = Vector2.Distance(transform.position, e.position);
            if (d < bestDist) { bestDist = d; best = e; }
        }
        return best;
    }

    private Transform PickTree()
    {
        if (gatherSpots.Count == 0) return null;
        return gatherSpots[Random.Range(0, gatherSpots.Count)];
    }

    private Transform PickEntrance()
    {
        if (entrances.Count == 0) return null;
        return entrances[Random.Range(0, entrances.Count)];
    }

    private bool MoveTo(Vector3 target)
    {
        // If a wall blocks the straight path, head for the best entrance first
        bool blocked = WallBetween(transform.position, target);
        Debug.DrawLine(transform.position, target, blocked ? Color.red : Color.green);  // red = wall detected
        Vector3 step = blocked ? BestEntrance(target) : target;

        transform.position = Vector2.MoveTowards(transform.position, step, moveSpeed * Time.deltaTime);
        return Vector2.Distance(transform.position, target) < 0.05f;
    }

    private bool WallBetween(Vector3 from, Vector3 to)
    {
        return Physics2D.Linecast(from, to, wallLayer);
    }

    // Pick the entrance that gives the shortest total detour to the target
    private Vector3 BestEntrance(Vector3 finalTarget)
    {
        Transform best = null;
        float bestCost = Mathf.Infinity;

        foreach (Transform e in entrances)
        {
            float cost = Vector2.Distance(transform.position, e.position)
                       + Vector2.Distance(e.position, finalTarget);
            if (cost < bestCost) { bestCost = cost; best = e; }
        }
        return best != null ? best.position : finalTarget;
    }

    // Draws the night-wander area (circle around the bonfire) in the Scene view
    void OnDrawGizmos()
    {
        Bonfire fire = FindFirstObjectByType<Bonfire>();
        Vector3 center = fire != null ? fire.transform.position : transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(center, new Vector3(wanderSize.x, wanderSize.y, 0f));
    }
}
