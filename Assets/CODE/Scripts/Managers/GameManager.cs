using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Utilities.Extensions;

public class GameManager : Singleton<GameManager>
{
    [Title("Levels")]
    [ReadOnly] public int CurrentLevel;
    [SerializeField] private LevelSO[] levels;
    [SerializeField] LevelDatabase levelDatabase;

    [Title("Events")]
    public UnityEvent OnGameWin;
    public UnityEvent<string> OnGameLose;

    public const string UNLOCKED_LEVELS_PREFS = "Unlocked Levels", CURRENT_LEVEL_PREFS = "Level", OWNED_CHARACTER_PREFS = "Characters", SELECTED_CHARACTER_PREFS = "Selected Character", COINS_PREFS = "Coins";

    private void Awake()
    {
        if (!PlayerPrefs.HasKey(CURRENT_LEVEL_PREFS) || PlayerPrefs.GetInt(CURRENT_LEVEL_PREFS) >= levels.Length + 1) PlayerPrefs.SetInt(CURRENT_LEVEL_PREFS, 1);
        CurrentLevel = PlayerPrefs.GetInt(CURRENT_LEVEL_PREFS);

        if (!levels.IsNullOrEmpty())
        {
            var level = levels[CurrentLevel - 1];
            UIManager.Instance.ShowObjective(level.ObjectiveMessage);
            FindFirstObjectByType<ObjectiveTracker>().BeginLevel(level, Instantiate(level.Prefab));

            OnGameWin.AddListener(() =>
            {
                UIManager.Instance.CoinsAddingAnimation(Camera.main!.ViewportToWorldPoint(new(0.5f, 0.5f, 5f))); // Center of the screen.
                CoinsManager.Instance.AddCoins(level.RewardCoins);
            });
        }
    }

    public void Restart()
    {
        Time.timeScale = 1f;

        UIManager.Instance.TogglePauseScreen(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        Time.timeScale = 0f;

        UIManager.Instance.TogglePauseScreen(false);
        SceneManager.LoadScene(0);
    }

    public void UnlockNextLevel()
    {
        // These lines only change data in RAM (Resets when app closes)
        levelDatabase.LevelData[CurrentLevel - 1].Score = 3;
        if (CurrentLevel < levels.Length) levelDatabase.LevelData[CurrentLevel].IsLocked = false;

        // Calculate next level index
        int nextLevel = CurrentLevel + 1;
        if (nextLevel > levels.Length) nextLevel = 1; // Reset to level 1 if we exceeded the count

        // These lines save the progress permanently
        PlayerPrefs.SetInt(CURRENT_LEVEL_PREFS, nextLevel);
        PlayerPrefs.SetInt(UNLOCKED_LEVELS_PREFS, Mathf.Clamp(PlayerPrefs.GetInt(UNLOCKED_LEVELS_PREFS, 1) + 1, 1, levels.Length));
        PlayerPrefs.Save(); // Good practice to force to write to disk on mobile
    }
}