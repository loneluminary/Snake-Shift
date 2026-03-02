using System;
using UnityEngine;

namespace DTT.LevelSelect
{
    /// <summary>
    /// Abstract class used for retrieving levels that can be used in <see cref="LevelSelect"/>.
    /// </summary>
    public abstract class LevelRetriever : MonoBehaviour
    {
        /// <summary>
        /// Should retrieve a level from the given range in the provided callback.
        /// </summary>
        /// <param name="from">From which level to start.</param>
        /// <param name="to">The last level for the range that should be retrieved.</param>
        /// <param name="callback">Used for passing back the level data when ready.</param>
        public abstract void Retrieve(int from, int to, Action<LevelData[]> callback);
        
        /// <summary>
        /// Should retrieve the total amount of levels using the provided callback.
        /// </summary>
        /// <param name="callback">When ready invoke this callback with the amount of total levels.</param>
        public abstract void RetrieveLevelCount(Action<int> callback);
    }
}