using UnityEngine;

public class PlayerAccuse : MonoBehaviour
{
    [SerializeField] private KeyCode accuseKey = KeyCode.F;
    [SerializeField] private SpriteRenderer selector;   // glow marker
    [SerializeField] private Camera cam;                // defaults to Camera.main

    private NPCGatherer target;

    void Start()
    {
        if (cam == null) cam = Camera.main;
        if (selector != null) selector.gameObject.SetActive(false);   // hidden until hovering an NPC
    }

    void Update()
    {
        target = NpcUnderMouse();
        UpdateSelector();

        if (Input.GetKeyDown(accuseKey)) DoAccuse();
    }

    // The NPC the mouse cursor is currently over (null if none)
    NPCGatherer NpcUnderMouse()
    {
        if (cam == null) return null;

        Vector3 screen = Input.mousePosition;
        screen.z = -cam.transform.position.z;   // distance from the camera to the z=0 gameplay plane
        Vector2 world = cam.ScreenToWorldPoint(screen);

        foreach (Collider2D h in Physics2D.OverlapPointAll(world))
            if (h.TryGetComponent(out NPCGatherer npc))
                return npc;

        return null;
    }

    // Called by the F key AND the mobile Accuse button — exiles whoever is hovered
    public void DoAccuse()
    {
        if (target != null) Accuse(target);
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

        SoundManager.Instance?.MetalHit(npc.transform.position);   // the exile blow
        npc.Die();                            // spawns death effect, then removes
    }
}
