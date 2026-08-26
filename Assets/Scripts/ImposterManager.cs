using UnityEngine;

public class ImposterManager : MonoBehaviour
{
    void Start()
    {
        NPCGatherer[] npcs = FindObjectsByType<NPCGatherer>(FindObjectsSortMode.None);
        if (npcs.Length == 0) return;

        // Clear everyone first, then pick exactly one at random
        foreach (NPCGatherer n in npcs) n.SetImpostor(false);

        int pick = Random.Range(0, npcs.Length);
        npcs[pick].SetImpostor(true);

        Debug.Log("Impostor is: " + npcs[pick].name);   // dev-only; remove before release
    }
}