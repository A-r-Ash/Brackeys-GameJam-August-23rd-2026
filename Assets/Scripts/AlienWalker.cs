using System.Collections.Generic;
using UnityEngine;

// The dropped alien walks to the camp, then possesses a random innocent crew member
// (turning them into an impostor) before vanishing.
public class AlienWalker : MonoBehaviour
{
    private Transform target;
    private float speed;

    public void Walk(Transform destination, float moveSpeed)
    {
        target = destination;
        speed = moveSpeed;
    }

    void Update()
    {
        if (target == null) return;

        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            PossessRandomInnocent();
            Destroy(gameObject);   // parasite has merged into a crew member
        }
    }

    void PossessRandomInnocent()
    {
        // Collect every crew member that isn't already an impostor
        List<NPCGatherer> innocents = new List<NPCGatherer>();
        foreach (NPCGatherer npc in FindObjectsByType<NPCGatherer>(FindObjectsSortMode.None))
            if (!npc.IsImpostor)
                innocents.Add(npc);

        if (innocents.Count == 0)
        {
            Debug.Log("Alien found no innocent to possess.");
            return;
        }

        NPCGatherer victim = innocents[Random.Range(0, innocents.Count)];
        victim.SetImpostor(true);
        Debug.Log(victim.name + " has been possessed — now an impostor!");
    }
}
