using System;
using System.Collections.ObjectModel;
using UnityEngine;

namespace DTT.LevelSelect
{
    /// <summary>
    /// A predefined layout for the Level Select environment.
    /// </summary>
    [ExecuteAlways]
    public class LevelSelectLayout : MonoBehaviour
    {
        /// <summary>
        /// The level buttons inside of the layout.
        /// </summary>
        [SerializeField, HideInInspector]
        private LevelButton[] _levelButtons;

        /// <summary>
        /// A readonly collection for retrieving all the level buttons inside this layout.
        /// </summary>
        public ReadOnlyCollection<LevelButton> LevelButtons => Array.AsReadOnly(_levelButtons);

        /// <summary>
        /// The recttransform of this layout.
        /// </summary>
        public RectTransform RectTransform => (RectTransform)transform;

        /// <summary>
        /// Gets all the level buttons.
        /// </summary>
        private void OnEnable() => _levelButtons = GetComponentsInChildren<LevelButton>();
    }
}