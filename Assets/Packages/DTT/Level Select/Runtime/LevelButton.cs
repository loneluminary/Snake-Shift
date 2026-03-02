using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DTT.LevelSelect
{
    /// <summary>
    /// The button in the Level Select environment that can be used to enter a level and display the user score.
    /// </summary>
    public class LevelButton : MonoBehaviour
    {
        /// <summary>
        /// The text object for displaying the current level.
        /// </summary>
        [SerializeField]
        [Tooltip("The text object for displaying the current level.")]
        private Text _levelText;

        /// <summary>
        /// All the images for the star score.
        /// </summary>
        [SerializeField]
        [Tooltip("All the images for the star score.")]
        private List<Image> _starImages;

        /// <summary>
        /// The actual button for click events.
        /// </summary>
        [SerializeField]
        [Tooltip("The actual button for click events.")]
        private Button _button;

        /// <summary>
        /// The sprite used to signify the level is completed.
        /// </summary>
        [SerializeField]
        [Tooltip("The sprite used to signify the level is completed.")]
        private Sprite _completedImage;

        /// <summary>
        /// The sprite used to signify the level is available to play.
        /// </summary>
        [SerializeField]
        [Tooltip("The sprite used to signify the level is available to play.")]
        private Sprite _nextLevelImage;

        /// <summary>
        /// The sprite used to signify the level is locked.
        /// </summary>
        [SerializeField]
        [Tooltip("The sprite used to signify the level is locked.")]
        private Sprite _lockedImage;

        /// <summary>
        /// The image for the background of the level so its sprite can be swapped based on the level state.
        /// </summary>
        [SerializeField]
        [Tooltip("The image for the background of the level so its sprite can be swapped based on the level state.")]
        private Image _background;

        /// <summary>
        /// The current level of this object.
        /// </summary>
        private int _levelNumber;

        /// <summary>
        /// Called when the button is clicked.
        /// </summary>
        public event Action OnClick;

        /// <summary>
        /// Called when the button is clicked and it passes itself.
        /// </summary>
        internal event Action<LevelButton> OnClickedThis;

        /// <summary>
        /// The current level of this object.
        /// </summary>
        public int LevelNumber
        {
            get => _levelNumber;
            set
            {
                _levelNumber = value;
                _levelText.text = _levelNumber.ToString();
            }
        }

        /// <summary>
        /// Enables the given amount of stars.
        /// </summary>
        /// <param name="amount">The amount of stars to enable.</param>
        public void EnableStars(int amount)
        {
            for (int i = 0; i < _starImages.Count; i++)
                _starImages[i].enabled = i < amount;
        }

        /// <summary>
        /// Whether this level is locked.
        /// </summary>
        public bool IsLocked
        {
            get => _background.sprite == _lockedImage;
            set
            {
                _button.interactable = !value;
                _levelText.gameObject.SetActive(!value);
                _background.sprite = value ? _lockedImage : _completedImage;
            }
        }

        /// <summary>
        /// Whether this is the next level.
        /// </summary>
        public bool IsNextLevel
        {
            get => _background.sprite == _nextLevelImage;
            set
            {
                if (value)
                {
                    _background.sprite = _nextLevelImage;
                    _levelText.gameObject.SetActive(true);
                }
            }
        }

        /// <summary>
        /// Adds listeners.
        /// </summary>
        private void OnEnable() => _button.onClick.AddListener(OnButtonPressed);

        /// <summary>
        /// Removes listeners.
        /// </summary>
        private void OnDisable() => _button.onClick.RemoveListener(OnButtonPressed);

        /// <summary>
        /// Called when the button is pressed and invokes events.
        /// </summary>
        private void OnButtonPressed()
        {
            OnClick?.Invoke();
            OnClickedThis?.Invoke(this);
        }
    }
}