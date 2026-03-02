using System;
using UnityEngine;

namespace DTT.LevelSelect.UI
{
    /// <summary>
    /// The flip type for defining the different flip settings.
    /// </summary>
    [Flags]
    public enum FlipType
    {
        /// <summary>
        /// Flip type for flipping on the X-axis.
        /// </summary>
        [InspectorName("Flip X")]
        FLIP_X = 1 << 0,

        /// <summary>
        /// Flip type for flipping on the Y-axis.
        /// </summary>
        [InspectorName("Flip Y")]
        FLIP_Y = 1 << 1,

        /// <summary>
        /// Defines that the flip should be done clockwise.
        /// </summary>
        [InspectorName("Clockwise")]
        CLOCKWISE = 1 << 2,

        /// <summary>
        /// Defines that the flip should be done counter clockwise.
        /// </summary>
        [InspectorName("Counter Clockwise")]
        COUNTER_CLOCKWISE = 1 << 3
    }
}