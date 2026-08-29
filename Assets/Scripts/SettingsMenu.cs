using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject hideWhileOpen;   // e.g. the pause panel — hidden while settings is up
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    void Start()
    {
        float music = PlayerPrefs.GetFloat("MusicVol", 1f);
        float sfx   = PlayerPrefs.GetFloat("SFXVol", 1f);

        ApplyMusic(music);
        ApplySfx(sfx);

        if (musicSlider != null) { musicSlider.SetValueWithoutNotify(music); musicSlider.onValueChanged.AddListener(SetMusic); }
        if (sfxSlider   != null) { sfxSlider.SetValueWithoutNotify(sfx);     sfxSlider.onValueChanged.AddListener(SetSfx); }

        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void SetMusic(float v) { ApplyMusic(v); PlayerPrefs.SetFloat("MusicVol", v); }
    public void SetSfx(float v)   { ApplySfx(v);   PlayerPrefs.SetFloat("SFXVol", v); }

    void ApplyMusic(float v) { if (mixer != null) mixer.SetFloat("MusicVol", ToDb(v)); }
    void ApplySfx(float v)   { if (mixer != null) mixer.SetFloat("SFXVol", ToDb(v)); }

    // slider 0..1 → decibels (0 = silent/-80dB, 1 = 0dB)
    float ToDb(float v) => Mathf.Log10(Mathf.Max(v, 0.0001f)) * 20f;

    public void Open()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
        if (hideWhileOpen != null) hideWhileOpen.SetActive(false);   // hide the pause menu behind it
    }

    public void Close()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (hideWhileOpen != null) hideWhileOpen.SetActive(true);    // bring the pause menu back
    }
}
