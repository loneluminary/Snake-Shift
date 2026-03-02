using System;
using System.Collections.ObjectModel;
using UnityEngine;

namespace DTT.LevelSelect
{
    /// <summary>
    /// A data structure for a layout composition. This can be viewed as an area or world in your level select.
    /// </summary>
    [CreateAssetMenu(menuName = "DTT/Level Select/Composition", fileName = "New Composition")]
    public class LevelSelectLayoutComposition : ScriptableObject
    {
        /// <summary>
        /// The layout used for entering this composition. Can be used for making smooth transitions.
        /// </summary>
        [SerializeField]
        [Tooltip("The layout used for entering this composition. Can be used for making smooth transitions.")]
        private LevelSelectLayout _entryLayout;

        /// <summary>
        /// The different layouts that can occur within this world. They are picked randomly.
        /// </summary>
        [SerializeField]
        [Tooltip("The different layouts that can occur within this world. They are picked randomly.")]
        private LevelSelectLayout[] _centreLayouts;

        /// <summary>
        /// The layout used for exiting this composition. Can be used for making smooth transitions.
        /// </summary>
        [SerializeField]
        [Tooltip("The layout used for exiting this composition. Can be used for making smooth transitions.")]
        private LevelSelectLayout _exitLayout;

        /// <summary>
        /// The range of levels this composition exists out of. Make sure the length of this is divisible by the amount of level buttons per layout.
        /// </summary>
        [SerializeField]
        [Tooltip("The range of levels this composition exists out of. Make sure the length of this is divisible by the amount of level buttons per layout.")]
        private Range _levelRange;
        
        /// <summary>
        /// The layout used for entering this composition. Can be used for making smooth transitions.
        /// </summary>
        public LevelSelectLayout EntryLayout => _entryLayout;

        /// <summary>
        /// The different layouts that can occur within this world. They are picked randomly.
        /// </summary>
        public ReadOnlyCollection<LevelSelectLayout> CentreLayouts => Array.AsReadOnly(_centreLayouts);

        /// <summary>
        /// The layout used for exiting this composition. Can be used for making smooth transitions.
        /// </summary>
        public LevelSelectLayout ExitLayout => _exitLayout;

        /// <summary>
        /// The layout used for entering this composition. Can be used for making smooth transitions.
        /// </summary>
        public Range LevelRange => _levelRange;
    }
}