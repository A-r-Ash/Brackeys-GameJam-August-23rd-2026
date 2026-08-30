using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioMixerGroup sfxGroup;   // route one-shots so the SFX slider controls them
    public static SoundManager Instance { get; private set; }

    [Header("One-shot clips")]
    [SerializeField] private AudioClip woodCut;
    [SerializeField] private AudioClip woodPut;
    [SerializeField] private AudioClip woodGather;
    [SerializeField] private AudioClip berryPick;
    [SerializeField] private AudioClip foodDrop;
    [SerializeField] private AudioClip recruit;
    [SerializeField] private AudioClip burn;
    [SerializeField] private AudioClip uvLightOn;
    [SerializeField] private AudioClip uvLightOff;
    [SerializeField] private AudioClip mumble;
    [SerializeField] private AudioClip zombieGrowl;
    [SerializeField] private AudioClip metalHit;
    [SerializeField] private AudioClip trapSnap;
    [SerializeField] private AudioClip dinoDeath;
    [SerializeField] private AudioClip dinoCaught;
    [SerializeField] private AudioClip dinoRoar;
    [SerializeField] private AudioClip dinoScreech;
    [SerializeField] private AudioClip dinoFootstep;
    [SerializeField] private AudioClip dinoWalk;

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
        s.outputAudioMixerGroup = sfxGroup;
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
    public void BerryPick(Vector3 p)   => PlayAt(berryPick, p);
    public void FoodDrop(Vector3 p)    => PlayAt(foodDrop, p);
    public void Recruit(Vector3 p)     => PlayAt(recruit, p);
    public void Burn(Vector3 p)        => PlayAt(burn, p);
    public void UvLightOn(Vector3 p)   => PlayAt(uvLightOn, p);
    public void UvLightOff(Vector3 p)  => PlayAt(uvLightOff, p);
    public void Mumble(Vector3 p)      => PlayAt(mumble, p);
    public void ZombieGrowl(Vector3 p) => PlayAt(zombieGrowl, p);
    public void MetalHit(Vector3 p)    => PlayAt(metalHit, p);
    public void TrapSnap(Vector3 p)    => PlayAt(trapSnap, p);
    public void DinoDeath(Vector3 p)   => PlayAt(dinoDeath, p);
    public void DinoCaught(Vector3 p)  => PlayAt(dinoCaught, p);
    public void DinoRoar(Vector3 p)    => PlayAt(dinoRoar, p);
    public void DinoScreech(Vector3 p) => PlayAt(dinoScreech, p);
    public void DinoFootstep(Vector3 p)=> PlayAt(dinoFootstep, p);
    public void DinoWalk(Vector3 p)    => PlayAt(dinoWalk, p);
}
