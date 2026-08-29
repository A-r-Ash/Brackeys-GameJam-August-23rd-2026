using UnityEngine;

// Central place for shared string/config constants so renaming a scene, a
// PlayerPrefs key, or an audio param only ever needs a single edit.
public static class GameConstants
{
    // Scene names used with SceneManager.LoadScene(string)
    public const string MainMenuScene = "MainMenu";
    public const string GameScene = "SampleScene";

    // PlayerPrefs keys for saved volume (0..1)
    public const string MusicVolKey = "MusicVol";
    public const string SfxVolKey = "SFXVol";

    // Exposed parameter names on Assets/Audio/MainMixer.mixer
    public const string MusicParam = "Music";
    public const string SfxParam = "SFX";

    // PlayerPrefs key for the best-days record shown on the end screens
    public const string BestDaysKey = "BestDays";

    // Legacy volume for any AudioSource not routed through the mixer.
    // These match the 0..1 slider space the mixer paths also use.
    public const float DefaultMusicVolume = 0.5f;
    public const float DefaultSfxVolume = 1f;
}
