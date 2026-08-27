using UnityEngine;

// A food source. When picked clean it hides, waits, then reappears somewhere else in its zone.
public class BerryBush : MonoBehaviour
{
    [SerializeField] private int maxBerries = 5;
    [SerializeField] private float regrowTime = 8f;   // hidden time before reappearing elsewhere

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
        float x = Random.Range(-zoneSize.x * 0.5f, zoneSize.x * 0.5f);
        float y = Random.Range(-zoneSize.y * 0.5f, zoneSize.y * 0.5f);
        transform.position = zoneCenter + new Vector3(x, y, 0f);
    }

    void SetVisible(bool v)
    {
        foreach (SpriteRenderer r in GetComponentsInChildren<SpriteRenderer>()) r.enabled = v;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = v;
    }
}
