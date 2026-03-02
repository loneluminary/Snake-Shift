using System;
using System.Collections.ObjectModel;
using UnityEngine;

namespace DTT.MinigameBase.LevelSelect
{
    /// <summary>
    /// Stores data about levels that can be used to populate a <see cref="LevelSelect"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "New Level Database", menuName = "DTT/Minigame Base/Level Database")]
    public class LevelDatabase : ScriptableObject
    {
        /// <summary>
        /// The initial amount of levels that will show up when creating the database.
        /// </summary>
        private const int INITIAL_LEVELS = 60;

        /// <summary>
        /// The level data inside of the database.
        /// </summary>
        public ReadOnlyCollection<LevelData> Data => Array.AsReadOnly(_data);

        /// <summary>
        /// The level data inside of the database.
        /// </summary>
        [SerializeField]
        [Tooltip("The level data inside of the database.")]
        private LevelData[] _data = new LevelData[INITIAL_LEVELS];

        /// <summary>
        /// Resets the state of the database as how it should be initially.
        /// </summary>
        private void Reset()
        {
            for (int i = 0; i < _data.Length; i++)
            {
                _data[i].levelNumber = i + 1;
                _data[i].locked = true;
            }

            // Initial level is unlocked.
            if (_data.Length > 1)
                _data[0].locked = false;
        }

        /// <summary>
        /// Sets the score of the index of a level.
        /// </summary>
        /// <param name="index">The index of the level. Generally should be one minus the level number.</param>
        /// <param name="score">The score to set the level to.</param>
        public void SetScore(int index, float score) => _data[index].score = score;

        /// <summary>
        /// Sets the locked state of the index of a level.
        /// </summary>
        /// <param name="index">The index of the level. Generally should be one minus the level number.</param>
        /// <param name="isLocked">Whether this level is locked.</param>
        public void SetLocked(int index, bool isLocked) => _data[index].locked = isLocked;
    }
}