using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerAccuse : MonoBehaviour
{
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

        // Left-click to eliminate (ignore clicks that land on an actual button)
        if (Input.GetMouseButtonDown(0) && !IsPointerOverButton())
            DoAccuse();
    }

    // True only if the cursor is over a real UI control (Button/Toggle/etc.),
    // not just any full-screen panel or text with Raycast Target on.
    bool IsPointerOverButton()
    {
        if (EventSystem.current == null) return false;

        PointerEventData data = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);

        foreach (RaycastResult r in results)
            if (r.gameObject.GetComponentInParent<Selectable>() != null)
                return true;

        return false;
    }

    // The NPC the mouse cursor is currently over (null if none)
    NPCGatherer NpcUnderMouse()
    {
        if (cam == null) return null;

        Vector3 screen = Input.mousePosition;
        screen.z = -cam.transform.position.z;   // distance from the camera to the z=0 gameplay plane
        Vector2 world = cam.ScreenToWorldPoint(screen);

        foreach (Collider2D h in Physics2D.OverlapPointAll(world))
        {
            NPCGatherer npc = h.GetComponentInParent<NPCGatherer>();
            if (npc != null) return npc;
        }

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
