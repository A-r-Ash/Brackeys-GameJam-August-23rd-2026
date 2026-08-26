using UnityEngine;

public class PlayerAccuse : MonoBehaviour
{
    [SerializeField] private float accuseRadius = 1.5f;
    [SerializeField] private KeyCode accuseKey = KeyCode.F;
    [SerializeField] private SpriteRenderer selector;   // ring/arrow marker

    private NPCGatherer target;

    void Start()
    {
        if (selector != null) selector.gameObject.SetActive(false);   // hidden until a target is found
    }

    void Update()
    {
        target = FindClosestNPC();
        UpdateSelector();

        if (Input.GetKeyDown(accuseKey)) DoAccuse();
    }

    // Called by the F key AND the mobile Accuse button
    public void DoAccuse()
    {
        if (target != null) Accuse(target);
    }

    NPCGatherer FindClosestNPC()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, accuseRadius);
        NPCGatherer closest = null;
        float best = Mathf.Infinity;

        foreach (Collider2D h in hits)
        {
            if (h.TryGetComponent(out NPCGatherer npc))
            {
                float d = Vector2.Distance(transform.position, npc.transform.position);
                if (d < best) { best = d; closest = npc; }
            }
        }
        return closest;
    }

    void UpdateSelector()
    {
        if (selector == null) return;

        selector.gameObject.SetActive(target != null);   // toggles the marker AND its children
        if (target != null)
            selector.transform.position = target.transform.position;
    }

    void Accuse(NPCGatherer npc)
    {
        if (npc.IsImpostor)
            Debug.Log("CAUGHT the impostor! Threat removed.");
        else
            Debug.Log("Wrong! " + npc.name + " was innocent — you lost a crew member.");

        SoundManager.Instance?.MetalHit();   // the exile blow
        Destroy(npc.gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, accuseRadius);
    }
}