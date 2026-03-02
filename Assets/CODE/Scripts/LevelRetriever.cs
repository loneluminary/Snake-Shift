using System;
using System.Collections.ObjectModel;
using DTT.LevelSelect;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-10)]
public class LevelRetriever : DTT.LevelSelect.LevelRetriever
{
    /// The level database used for storing all the level data.
    public LevelDatabase LevelDatabase => _levelDatabase;
        
    /// The level database used for storing all the level data.
    [SerializeField] LevelDatabase _levelDatabase;

    private void Awake()
    {
        LevelSelect.ButtonPressed += level =>
        {
            PlayerPrefs.SetInt(GameManager.CURRENT_LEVEL_PREFS, level.LevelNumber);
            SceneManager.LoadScene(2);
        };
        
        for (int i = 0; i < _levelDatabase.LevelData.Count; i++)
        {
            if (i != 0 ) _levelDatabase.LevelData[i].IsLocked = true;
            _levelDatabase.LevelData[i].Score = 0;
        }

        if (PlayerPrefs.HasKey(GameManager.UNLOCKED_LEVELS_PREFS))
        {
            int unlockedCount = PlayerPrefs.GetInt(GameManager.UNLOCKED_LEVELS_PREFS, 1);
            for (int i = 0; i < unlockedCount; i++)
            {
                _levelDatabase.LevelData[i].IsLocked = false;
                _levelDatabase.LevelData[i].Score = i < unlockedCount - 1 ? 3 : 0; // The very last unlocked level is the "current" one, so it should have 0 stars.
            }
        }
    }

    /// Retrieves levels from the database based on the range.
    /// <param name="from">That start of the range.</param>
    /// <param name="to">The end of the range.</param>
    /// <param name="callback"><inheritdoc/></param>
    public override void Retrieve(int @from, int to, Action<LevelData[]> callback)
    {
        ReadOnlyCollection<LevelData> levelData = _levelDatabase.LevelData;

        if (from > levelData.Count)
        {
            callback?.Invoke(Array.Empty<LevelData>());
            return;
        }

        if (from < 0)
            from = 0;
        if (to < 0)
            to = 0;

        if (to - from <= 0)
        {
            callback?.Invoke(Array.Empty<LevelData>());
            return;
        }
            
        LevelData[] data = new LevelData[Mathf.Min(to, levelData.Count) - from + 1];

        for (int i = 0; i < data.Length; i++)
            data[i] = levelData[i + from - 1];
            

        callback?.Invoke(data);
    }

    /// <summary>
    /// Retrieves the total amount of levels from the database.
    /// </summary>
    /// <param name="callback">Used for passing back the amount of levels.</param>
    public override void RetrieveLevelCount(Action<int> callback) => callback?.Invoke(_levelDatabase.LevelData.Count);
}