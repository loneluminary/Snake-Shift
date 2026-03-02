using UnityEngine;

namespace DTT.LevelSelect
{
    /// <summary>
    /// The direction for the Level Select to go in.
    /// </summary>
    public enum LevelSelectDirection
    {
        /// <summary>
        /// Horizontal direction.
        /// </summary>
        [InspectorName("Horizontal")]
        HORIZONTAL = 0,

        /// <summary>
        /// Vertical direction.
        /// </summary>
        [InspectorName("Vertical")]
        VERTICAL = 1
    }
}