using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("One-shot clips")]
    [SerializeField] private AudioClip woodCut;
    [SerializeField] private AudioClip woodPut;
    [SerializeField] private AudioClip woodGather;
    [SerializeField] private AudioClip burn;
    [SerializeField] private AudioClip uvLightOn;
    [SerializeField] private AudioClip uvLightOff;
    [SerializeField] private AudioClip mumble;
    [SerializeField] private AudioClip zombieGrowl;
    [SerializeField] private AudioClip metalHit;

    [Header("Spatial")]
    [SerializeField] private float maxHearDistance = 12f;   // beyond this, you can't hear it
    [SerializeField] private float volume = 1f;

    void Awake() { Instance = this; }

    // Spawns a short-lived 3D audio source at the event's location, so volume falls off with distance
    void PlayAt(AudioClip clip, Vector3 pos)
    {
        if (clip == null) return;

        GameObject go = new GameObject("SFX");
        go.transform.position = pos;

        AudioSource s = go.AddComponent<AudioSource>();
        s.clip = clip;
        s.volume = volume;
        s.spatialBlend = 1f;                    // fully 3D → distance-based
        s.dopplerLevel = 0f;
        s.rolloffMode = AudioRolloffMode.Linear;
        s.minDistance = maxHearDistance * 0.2f;                 // full volume only very close
        s.maxDistance = Mathf.Max(maxHearDistance, s.minDistance + 0.01f);  // silent past here
        s.Play();

        Destroy(go, clip.length + 0.1f);
    }

    public void WoodCut(Vector3 p)     => PlayAt(woodCut, p);
    public void WoodPut(Vector3 p)     => PlayAt(woodPut, p);
    public void WoodGather(Vector3 p)  => PlayAt(woodGather, p);
    public void Burn(Vector3 p)        => PlayAt(burn, p);
    public void UvLightOn(Vector3 p)   => PlayAt(uvLightOn, p);
    public void UvLightOff(Vector3 p)  => PlayAt(uvLightOff, p);
    public void Mumble(Vector3 p)      => PlayAt(mumble, p);
    public void ZombieGrowl(Vector3 p) => PlayAt(zombieGrowl, p);
    public void MetalHit(Vector3 p)    => PlayAt(metalHit, p);
}
