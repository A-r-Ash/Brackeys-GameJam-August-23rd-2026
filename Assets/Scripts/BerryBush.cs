using UnityEngine;

// A food source. When picked clean it hides, waits, then reappears somewhere else in its zone.
public class BerryBush : MonoBehaviour
{
    [SerializeField] private int maxBerries = 5;
    [SerializeField] private float regrowTime = 8f;   // hidden time before reappearing elsewhere
    [SerializeField] private float clearance = 0.6f;  // required empty radius around a spawn spot
    [SerializeField] private int maxTries = 25;       // attempts to find a clear spot

    private int berries;
    private float hideTimer;
    private bool depleted;

    private Vector3 zoneCenter;
    private Vector2 zoneSize;
    private bool hasZone;

    public bool HasBerries => berries > 0;

    void Awake()
    {
        berries = maxBerries;
    }

    // Manager gives the bush its roaming area (and places it once)
    public void SetZone(Vector3 center, Vector2 size)
    {
        zoneCenter = center;
        zoneSize = size;
        hasZone = true;
        Relocate();
    }

    void Update()
    {
        if (!depleted) return;

        hideTimer -= Time.deltaTime;
        if (hideTimer <= 0f)
        {
            Relocate();
            berries = maxBerries;
            depleted = false;
            SetVisible(true);
        }
    }

    public int Pick(int amount)
    {
        int taken = Mathf.Min(amount, berries);
        berries -= taken;

        if (berries <= 0 && !depleted)
        {
            depleted = true;
            hideTimer = regrowTime;
            SetVisible(false);   // hide + block picking while it regrows
        }
        return taken;
    }

    void Relocate()
    {
        if (!hasZone) return;

        Vector3 candidate = transform.position;
        for (int i = 0; i < maxTries; i++)
        {
            float x = Random.Range(-zoneSize.x * 0.5f, zoneSize.x * 0.5f);
            float y = Random.Range(-zoneSize.y * 0.5f, zoneSize.y * 0.5f);
            candidate = zoneCenter + new Vector3(x, y, 0f);

            if (IsClear(candidate)) break;   // found an empty spot
        }
        transform.position = candidate;      // clear spot, or the last try if none found
    }

    // No other collider within the clearance radius (ignores this bush's own collider)
    bool IsClear(Vector3 pos)
    {
        foreach (Collider2D c in Physics2D.OverlapCircleAll(pos, clearance))
            if (c.gameObject != gameObject)
                return false;
        return true;
    }

    void SetVisible(bool v)
    {
        foreach (SpriteRenderer r in GetComponentsInChildren<SpriteRenderer>()) r.enabled = v;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = v;
    }
}
