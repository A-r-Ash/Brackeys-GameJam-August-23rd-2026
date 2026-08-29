using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    private bool paused;

    void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (paused) Resume();
            else Pause();
        }
    }

    void Pause()
    {
        if (Time.timeScale == 0f) return;   // already frozen (win/lose) — don't pause over it

        paused = true;
        if (pausePanel != null) pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    // Hook to the Resume button (and the pause key)
    public void Resume()
    {
        paused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    // Hook to a Restart button
    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Hook to a Main Menu button
    public void ToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }
}
