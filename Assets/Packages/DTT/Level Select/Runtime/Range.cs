using System;

namespace DTT.LevelSelect
{
    /// <summary>
    /// A range data structure used for defining value based on two values.
    /// </summary>
    [Serializable]
    public struct Range
    {
        /// <summary>
        /// The start of this range.
        /// </summary>
        public int start;
        
        /// <summary>
        /// The length of this range.
        /// </summary>
        public int length;
        
        /// <summary>
        /// The end point of this range.
        /// </summary>
        public int end => start + length;
    }
}