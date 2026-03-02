using System;
using UnityEngine;

namespace DTT.LevelSelect
{
    /// <summary>
    /// The data structure for a level.
    /// </summary>
    [Serializable]
    public class LevelData
    {
        /// <summary>
        /// The number of the level.
        /// </summary>
        [SerializeField]
        private int _levelNumber;
        
        /// <summary>
        /// The maximum score you can achieve for this level.
        /// </summary>
        [SerializeField]
        private float _maxScore;

        /// <summary>
        /// Whether this level is locked and can't be played yet.
        /// </summary>
        [SerializeField]
        private bool _isLocked;

        /// <summary>
        /// The current score a user has for this level.
        /// </summary>
        [SerializeField]
        private float _score;

        /// <summary>
        /// The number of the level.
        /// </summary>
        public int LevelNumber => _levelNumber;

        /// <summary>
        /// The current score a user has for this level.
        /// </summary>
        public float Score
        {
            get => _score;
            set => _score = Mathf.Clamp(value, 0, MaxScore);
        }

        /// <summary>
        /// The maximum score you can achieve for this level.
        /// </summary>
        public float MaxScore => _maxScore;

        /// <summary>
        /// Whether this level is locked and can't be played yet.
        /// </summary>
        public bool IsLocked
        {
            get => _isLocked;
            set => _isLocked = value;
        }

        /// <summary>
        /// Creates a new level data instance.
        /// </summary>
        /// <param name="levelNumber">The number of the level.</param>
        /// <param name="score">The current score a user has for this level.</param>
        /// <param name="maxScore">The maximum score you can achieve for this level.</param>
        /// <param name="isLocked">Whether this level is locked and can't be played yet.</param>
        public LevelData(int levelNumber, float score, float maxScore, bool isLocked)
        {
            _levelNumber = levelNumber;
            _score = score;
            _maxScore = maxScore;
            _isLocked = isLocked;
        }
    }
}