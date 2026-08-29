using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    [SerializeField] private DayNightCycle cycle;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TMP_Text scoreText;   // shows days survived + best (on the end panel)
    [SerializeField] private TMP_Text headline;    // optional: the "Fire went out"/"Ship repaired!" heading
    [SerializeField] private string mainMenuScene = "MainMenu";

    private bool gameOver;

    void Awake() { Instance = this; }

    void Start()
    {
        Time.timeScale = 1f;                  // reset in case we came from a game-over
        if (losePanel != null) losePanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
    }

    // Called when the ship is fixed → escape
    public void Win()
    {
        if (gameOver) return;
        gameOver = true;

        int days = cycle != null ? cycle.DayNumber : 0;
        int best = UpdateBestDays(days);

        if (headline != null) headline.text = "Ship repaired!";
        if (scoreText != null)
            scoreText.text = $"You escaped after {days} days\nBest: {best}";

        GameObject panel = winPanel != null ? winPanel : losePanel;
        if (panel != null) panel.SetActive(true);
        Time.timeScale = 0f;
    }

    // Called when the fire goes out
    public void Lose()
    {
        if (gameOver) return;
        gameOver = true;

        int days = cycle != null ? cycle.DayNumber : 0;
        int best = UpdateBestDays(days);

        if (headline != null) headline.text = "Fire went out";
        if (scoreText != null)
            scoreText.text = $"You survived {days} days\nBest: {best}";

        if (losePanel != null) losePanel.SetActive(true);
        Time.timeScale = 0f;                  // freeze
    }

    int UpdateBestDays(int days)
    {
        int best = PlayerPrefs.GetInt(GameConstants.BestDaysKey, 0);
        if (days > best)
        {
            best = days;
            PlayerPrefs.SetInt(GameConstants.BestDaysKey, best);
        }

        return best;
    }

    // Hook this to the "Restart" button
    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Hook to a "Main Menu" button on the win/lose panels
    public void ToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }
}
