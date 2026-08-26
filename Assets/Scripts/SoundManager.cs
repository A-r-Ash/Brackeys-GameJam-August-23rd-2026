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

    private AudioSource source;

    void Awake()
    {
        Instance = this;
        source = GetComponent<AudioSource>();
    }

    private void Play(AudioClip clip)
    {
        if (clip != null && source != null) source.PlayOneShot(clip);
    }

    public void WoodCut()     => Play(woodCut);
    public void WoodPut()     => Play(woodPut);
    public void WoodGather()  => Play(woodGather);
    public void Burn()        => Play(burn);
    public void UvLightOn()   => Play(uvLightOn);
    public void UvLightOff()  => Play(uvLightOff);
    public void Mumble()      => Play(mumble);
    public void ZombieGrowl() => Play(zombieGrowl);
    public void MetalHit()    => Play(metalHit);
}