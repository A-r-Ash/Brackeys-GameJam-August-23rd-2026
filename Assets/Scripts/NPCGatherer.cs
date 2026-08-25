using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NPCGatherer : MonoBehaviour
{
    [SerializeField] private WoodPile pile;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float gatherTime = 2f;
    [SerializeField] private int woodPerTrip = 1;
    [SerializeField] private TMP_Text stateText;   // world-space text under the NPC

    public static int Count { get; private set; }

    public enum State { GoingToGather, Gathering, ReturningToPile }
    private State state = State.GoingToGather;
    public State CurrentState => state;

    private List<Transform> gatherSpots = new List<Transform>();
    private Transform currentSpot;
    private float gatherTimer;

    void OnEnable()  { Count++; }
    void OnDisable() { Count--; }

    void Awake()
    {
        Tree[] trees = FindObjectsByType<Tree>(FindObjectsSortMode.None);
        foreach (Tree t in trees)
            gatherSpots.Add(t.transform);

        currentSpot = PickTree();
    }

    void Update()
    {
        if (currentSpot != null)
        {
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

        if (stateText != null)
            stateText.text = state.ToString();   // "GoingToGather", "Gathering", etc.
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
}