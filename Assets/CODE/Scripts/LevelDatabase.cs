using System.Collections.Generic;
using System.Collections.ObjectModel;
using DTT.LevelSelect;
using UnityEngine;

[CreateAssetMenu(fileName = "Level Database", menuName = "DTT/Level Selection/(Demo) Level Database")]
public class LevelDatabase : ScriptableObject
{
    /// All the data of the levels.
    public List<LevelData> _levelData;

    /// A readonly collection for all the data of the levels.
    public ReadOnlyCollection<LevelData> LevelData => _levelData.AsReadOnly();
}