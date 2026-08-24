using System.Collections.Generic;
using UnityEngine;

public class NPCGatherer : MonoBehaviour
{
    [SerializeField] private WoodPile pile;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float gatherTime = 2f;
    [SerializeField] private int woodPerTrip = 1;

    private List<Transform> gatherSpots = new List<Transform>();
    private Transform currentSpot;

    private enum State { GoingToGather, Gathering, ReturningToPile }
    private State state = State.GoingToGather;
    private float gatherTimer;

    void Awake()
    {
        // Find every Tree in the scene, store their transforms
        Tree[] trees = FindObjectsByType<Tree>(FindObjectsSortMode.None);
        foreach (Tree t in trees)
            gatherSpots.Add(t.transform);

        currentSpot = PickTree();
    }

    void Update()
    {
        if (currentSpot == null) return;   // no trees found

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
                    currentSpot = PickTree();   // choose a new tree for next trip
                    state = State.GoingToGather;
                }
                break;
        }
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