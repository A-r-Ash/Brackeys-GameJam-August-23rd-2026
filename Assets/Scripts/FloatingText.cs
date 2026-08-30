using UnityEngine;
using TMPro;

// A small world-space label ("+5 Food", "Recruited!") that floats up and fades out.
// No prefab or scene wiring needed: call FloatingText.Show(pos, message[, color]) anywhere.
public class FloatingText : MonoBehaviour
{
    [SerializeField] private float riseSpeed = 1.1f;
    [SerializeField] private float lifetime = 1.4f;
    [Range(0f, 1f)] [SerializeField] private float fadeStart = 0.5f;

    public static readonly Color FoodColor    = new Color(1f, 0.82f, 0.25f);
    public static readonly Color WoodColor    = new Color(0.71f, 0.52f, 0.3f);
    public static readonly Color RecruitColor = new Color(0.4f, 1f, 0.55f);

    private const float WorldFontSize = 1.15f;

    private TMP_Text label;
    private float age;
    private bool spawned;   // only instances created by Show() animate & destroy themselves

    void Awake()
    {
        label = GetComponent<TMP_Text>();
    }

    void Update()
    {
        if (!spawned) return;   // ignore components accidentally placed on scene objects

        age += Time.deltaTime;
        transform.position += Vector3.up * (riseSpeed * Time.deltaTime);

        if (label != null && age > lifetime * fadeStart)
        {
            float t = (age - lifetime * fadeStart) / (lifetime * (1f - fadeStart));
            Color c = label.color;
            c.a = 1f - Mathf.Clamp01(t);
            label.color = c;
        }

        if (age >= lifetime) Destroy(gameObject);
    }

    public static void Show(Vector3 pos, string message)
    {
        Show(pos, message, Color.white, WorldFontSize);
    }

    public static void Show(Vector3 pos, string message, Color color)
    {
        Show(pos, message, color, WorldFontSize);
    }

    public static void Show(Vector3 pos, string message, Color color, float fontSize)
    {
        // No SDF default font → TextMeshPro can't build text and throws NRE. Skip silently.
        if (TMP_Settings.defaultFontAsset == null) return;

        GameObject go = null;
        try
        {
            go = new GameObject("FloatingText");
            go.transform.position = pos + Vector3.up * 0.5f;

            TextMeshPro text = go.AddComponent<TextMeshPro>();
            text.text = message;
            text.color = color;
            text.fontSize = fontSize;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.enableWordWrapping = false;
            text.overflowMode = TextOverflowModes.Overflow;

            // A runtime-added TMP may not carry a RectTransform yet - guard it.
            if (text.rectTransform != null)
                text.rectTransform.sizeDelta = new Vector2(4f, 1.5f);

            Renderer r = go.GetComponent<Renderer>();
            if (r != null) r.sortingOrder = 100;   // render above world sprites

            FloatingText ft = go.AddComponent<FloatingText>();
            ft.spawned = true;
        }
        catch (System.Exception)
        {
            // Cosmetic label: never let a missing font/rect setup crash the game.
            if (go != null) Object.Destroy(go);
        }
    }
}