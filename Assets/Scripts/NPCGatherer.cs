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

    [Header("Night wander")]
    [SerializeField] private Vector2 wanderSize = new Vector2(8f, 6f);   // rectangle (width, height) NPCs roam in

    [Header("Impostor")]
    [SerializeField] private bool isImpostor = false;
    [SerializeField] private float sabotageInterval = 6f;  // wander time between thefts
    [SerializeField] private int stealAmount = 3;          // wood taken per theft

    public static int Count { get; private set; }

    public enum State { GoingToGather, Gathering, ReturningToPile, Wandering, Stealing, Dumping }
    private State state = State.GoingToGather;
    public State CurrentState => state;
    public bool IsImpostor => isImpostor;
    public void SetImpostor(bool value) { isImpostor = value; }

    private List<Transform> gatherSpots = new List<Transform>();
    private List<Transform> entrances = new List<Transform>();
    private Transform currentSpot;
    private Transform dumpEntrance;
    private float gatherTimer;

    private Vector3 wanderTarget;
    private Vector3 campCenter;
    private float sabotageTimer;
    private int carriedStolen;
    private bool wasNight;

    void OnEnable()  { Count++; }
    void OnDisable() { Count--; }

    void Awake()
    {
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
                state = State.Wandering;         // everyone comes inside and wanders
                PickWanderTarget();
                sabotageTimer = sabotageInterval;
            }
            else
            {
                state = State.GoingToGather;      // everyone heads out to gather
                currentSpot = PickTree();
            }
        }

        if (night) NightUpdate();
        else       DayUpdate();

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
                }
                break;

            case State.Gathering:
                gatherTimer -= Time.deltaTime;
                if (gatherTimer <= 0f)
                    state = State.ReturningToPile;
                break;

            case State.ReturningToPile:
                if (MoveTo(pile.transform.position))
                {
                    pile.AddWood(woodPerTrip);
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
            case State.Wandering:
                if (MoveTo(wanderTarget))
                    PickWanderTarget();

                if (isImpostor)
                {
                    sabotageTimer -= Time.deltaTime;
                    if (sabotageTimer <= 0f)
                        state = State.Stealing;
                }
                break;

            case State.Stealing:                         // go to pile, grab wood
                if (MoveTo(pile.transform.position))
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
        float x = Random.Range(-wanderSize.x * 0.5f, wanderSize.x * 0.5f);
        float y = Random.Range(-wanderSize.y * 0.5f, wanderSize.y * 0.5f);
        wanderTarget = campCenter + new Vector3(x, y, 0f);
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
