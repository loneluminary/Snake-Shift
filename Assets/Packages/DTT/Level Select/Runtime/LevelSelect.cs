using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DTT.Utils.Extensions;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace DTT.LevelSelect
{
    /// <summary>
    /// The central component used for managing a Level Select environment.
    /// </summary>
    public class LevelSelect : MonoBehaviour
    {
        /// <summary>
        /// Called when a button is pressed in the Level Select passing the instance of the button that was pressed.
        /// </summary>
        public static event Action<LevelButton> ButtonPressed;
        
        /// <summary>
        /// Called when a layout is removed.
        /// </summary>
        public event Action LayoutRemoved;
        
        /// <summary>
        /// Called when a layout has been setup.
        /// </summary>
        public event Action LayoutSetup;
        
        /// <summary>
        /// A readonly collection of all the layout instances in the level select environment.
        /// </summary>
        public ReadOnlyCollection<LevelSelectLayout> LayoutInstances => _layoutInstances.AsReadOnly();

        /// <summary>
        /// The compositions used for the different areas/worlds in your level select.
        /// </summary>
        [SerializeField]
        [Tooltip("The compositions used for the different areas/worlds in your level select.")]
        private LevelSelectLayoutComposition[] _compositions;
        
        /// <summary>
        /// The retriever component for retrieving the levels from your game.
        /// </summary>
        [SerializeField]
        [Tooltip("The retriever component for retrieving the levels from your game.")]
        private LevelRetriever _levelRetriever;

        /// <summary>
        /// The parent of which the layouts should be children.
        /// </summary>
        [SerializeField]
        [Tooltip("The parent of which the layouts should be children.")]
        private Transform _layoutParent;

        /// <summary>
        /// The scroll component for managing the scrolling.
        /// </summary>
        [SerializeField]
        [Tooltip("The scroll component for managing the scrolling.")]
        private ScrollRect _scroll;

        /// <summary>
        /// The direction this Level Select is heading in. NOTE: try to match this with the Scroll Rect direction.
        /// </summary>
        [SerializeField]
        [Tooltip("The direction this Level Select is heading in. NOTE: try to match this with the Scroll Rect direction.")]
        private LevelSelectDirection _direction;

        /// <summary>
        /// The level to start viewing.
        /// </summary>
        [SerializeField]
        [Min(1)]
        [Tooltip("The level to start viewing.")]
        private int _startLevel = 1;

        /// <summary>
        /// The amount of buttons per layout. Be careful all your layouts have this amount of buttons.
        /// </summary>
        [SerializeField]
        [Tooltip("The amount of buttons per layout. Be careful all your layouts have this amount of buttons.")]
        private int _buttonsPerLayout = 10;

        /// <summary>
        /// The position of the content in the scroll rect.
        /// </summary>
        private Vector2 _contentPosition;

        /// <summary>
        /// The active amount of layout instances.
        /// </summary>
        private readonly List<LevelSelectLayout> _layoutInstances = new List<LevelSelectLayout>();

        /// <summary>
        /// The coroutine for loading levels.
        /// </summary>
        private Coroutine _loadingLevels;

        /// <summary>
        /// The guard for checking all the necessary fields have been entered.
        /// </summary>
        private bool guard => new Guard(_levelRetriever, _scroll, _layoutParent).Validate();
        
        /// <summary>
        /// Sets up the initial state of the level select.
        /// </summary>
        private void OnEnable()
        {
            if (!guard)
            {
                Debug.LogError("Unassigned Object fields!", this);
                return;
            }
            
            _scroll.onValueChanged.AddListener(OnScrollMoved);

            if (_scroll.horizontal)
            {
                _scroll.content.SetAnchor(RectAnchor.STRETCH_LEFT, true, true);
                _scroll.content.sizeDelta = Vector2.right * _compositions.First().EntryLayout.RectTransform.rect.width;
            }

            if (_scroll.vertical)
            {
                _scroll.content.SetAnchor(RectAnchor.STRETCH_BOTTOM , true, true);
                _scroll.content.sizeDelta = Vector2.up * _compositions.First().EntryLayout.RectTransform.rect.height;
            }
            
            _levelRetriever.RetrieveLevelCount(levelCount =>
            {
                _startLevel = Mathf.Min(_startLevel, levelCount);

                int layoutIndex = Mathf.FloorToInt((_startLevel - 1) / (float)_buttonsPerLayout);
                int layoutFirstLevel = layoutIndex * _buttonsPerLayout + 1;
                int layoutLastLevel = layoutFirstLevel + _buttonsPerLayout - 1;
                Vector2 position = new Vector2(
                    (1 - (int)_direction) * layoutIndex * _compositions.First().EntryLayout.RectTransform.rect.width, 
                    (int)_direction * layoutIndex * _compositions.First().EntryLayout.RectTransform.rect.height
                );
                _scroll.content.anchoredPosition = position * -1;
                
                LevelSelectLayout prefab = GetPrefab(layoutFirstLevel, layoutLastLevel);
                _layoutInstances.Add(Instantiate(prefab, _layoutParent));
                
                LevelSelectLayout firstLayout = _layoutInstances[0];
                firstLayout.RectTransform.anchoredPosition = position;
                
                _levelRetriever.Retrieve(layoutFirstLevel, layoutLastLevel, data =>
                {
                    SetupLayout(firstLayout, data);
                    StartCoroutine(LoadInLayout(data.Last().LevelNumber, true));
                    StartCoroutine(LoadInLayout(data.First().LevelNumber, false));
                });
            });
        }

        /// <summary>
        /// Removes the listener from the scroll rect.
        /// </summary>
        private void OnDisable() => _scroll.onValueChanged.RemoveListener(OnScrollMoved);

        /// <summary>
        /// Loads in a layout based on the last level. Can be either forward or backwards.
        /// </summary>
        /// <param name="lastLevel">The last level to place this in front/behind of.</param>
        /// <param name="forward">Whether to load it in forwards or backwards.</param>
        /// <returns></returns>
        private IEnumerator LoadInLayout(int lastLevel, bool forward)
        {
            int from;
            int to;

            if (forward)
            {
                from = lastLevel + 1;
                to = _compositions.First().EntryLayout.LevelButtons.Count + lastLevel;
            }
            else
            {
                to = lastLevel - 1;
                from = lastLevel - _compositions.First().EntryLayout.LevelButtons.Count;
            }
            bool waitForLayoutSetup = true;
            _levelRetriever.Retrieve(from, to, data =>
            {
                if (data.Length == 0)
                {
                    waitForLayoutSetup = false;
                    return;
                }

                LevelSelectLayout prefab = GetPrefab(from, to);
                
                LevelSelectLayout layout = Instantiate(prefab, _layoutParent);
                float axisPosition = 0;
                if (forward)
                {
                    RectTransform lastRectTransform = _layoutInstances.Last().RectTransform;
                    if (_direction == LevelSelectDirection.HORIZONTAL)
                        axisPosition = lastRectTransform.anchoredPosition.x + lastRectTransform.rect.width;
                    else if (_direction == LevelSelectDirection.VERTICAL)
                        axisPosition = lastRectTransform.anchoredPosition.y + lastRectTransform.rect.height;
                }
                else
                {
                    RectTransform first = _layoutInstances.First().RectTransform;
                    if (_direction == LevelSelectDirection.HORIZONTAL)
                        axisPosition = first.anchoredPosition.x - first.rect.width;
                    else if (_direction == LevelSelectDirection.VERTICAL)
                        axisPosition = first.anchoredPosition.y - first.rect.height;
                }

                Vector3 position = new Vector3();
                position[(int)_direction] = axisPosition;
                layout.RectTransform.anchoredPosition = position;
                _layoutInstances.Insert(forward ? _layoutInstances.Count : 0, layout);
                
                SetupLayout(layout, data);
                waitForLayoutSetup = false;
            });
            yield return new WaitUntil(() => !waitForLayoutSetup);

            RectTransform last = _layoutInstances.Last().RectTransform;
            float sizeAxis = 0;
            
            if (_direction == LevelSelectDirection.HORIZONTAL)
                sizeAxis = last.anchoredPosition.x + last.rect.width;
            else if (_direction == LevelSelectDirection.VERTICAL)
                sizeAxis = last.anchoredPosition.y + last.rect.height;

            Vector3 size = new Vector3();
            size[(int) _direction] = sizeAxis;
            _scroll.content.sizeDelta = size;
            _loadingLevels = null;
        }

        /// <summary>
        /// Gets the layout prefab based on the range of levels given.
        /// </summary>
        /// <param name="from">The start range of the levels.</param>
        /// <param name="to">The end range of the levels.</param>
        /// <returns>The correct prefab.</returns>
        private LevelSelectLayout GetPrefab(int from, int to)
        {
            LevelSelectLayoutComposition selectedComposition = _compositions.First(composition =>
                from >= composition.LevelRange.start && to <= composition.LevelRange.end);
            
            LevelSelectLayout prefab;
            if (selectedComposition.LevelRange.start == from)
                prefab = selectedComposition.EntryLayout;
            else if (selectedComposition.LevelRange.end == to)
                prefab = selectedComposition.ExitLayout;
            else
                prefab = selectedComposition.CentreLayouts[Random.Range(0, selectedComposition.CentreLayouts.Count)];

            return prefab;
        }

        /// <summary>
        /// Called when the scroll is moved and checks for loading in/out any layouts.
        /// </summary>
        /// <param name="moved">The amount moved.</param>
        private void OnScrollMoved(Vector2 moved)
        {
            if (_loadingLevels != null)
                return;
            
            LevelSelectLayout first = _layoutInstances[0];
            LevelSelectLayout second = _layoutInstances[1];
            LevelSelectLayout last = _layoutInstances[_layoutInstances.Count - 1];
            LevelSelectLayout secondToLast = _layoutInstances[_layoutInstances.Count - 2];

            Vector2 deltaMove = _contentPosition - _scroll.content.anchoredPosition;

            if (_direction == LevelSelectDirection.HORIZONTAL && deltaMove.x > 0)
                SetupLayout(last, secondToLast, first, second, true);
            else if (_direction == LevelSelectDirection.HORIZONTAL && deltaMove.x < 0)
                SetupLayout(first, second, last, secondToLast, false);
            else if (_direction == LevelSelectDirection.VERTICAL && deltaMove.y > 0)
                SetupLayout(last, secondToLast, first, second, true);
            else if (_direction == LevelSelectDirection.VERTICAL && deltaMove.y < 0)
                SetupLayout(first, second, last, secondToLast, false);

            _contentPosition = _scroll.content.anchoredPosition;
        }
        
        /// <summary>
        /// Handles setting up the given loaded layouts.
        /// </summary>
        /// <param name="loadInFirst">The first layout that is loaded in.</param>
        /// <param name="loadInSecond">The second layout that is loaded in.</param>
        /// <param name="loadOutFirst">The first layout that is loaded out.</param>
        /// <param name="loadOutSecond">The second layout that is loaded out.</param>
        /// <param name="lookForward">Whether to look forward or backwards. Changes based on the direction moved in.</param>
        private void SetupLayout(LevelSelectLayout loadInFirst, LevelSelectLayout loadInSecond, LevelSelectLayout loadOutFirst, LevelSelectLayout loadOutSecond, bool lookForward)
        {
            float absoluteScroll = Mathf.Abs(_scroll.content.anchoredPosition[(int) _direction]);
            float inSecondAnchorPos = loadInSecond.RectTransform.anchoredPosition[(int) _direction];
            
            bool inForwardCheck = lookForward && absoluteScroll > inSecondAnchorPos;
            bool inBackwardCheck = !lookForward && absoluteScroll < inSecondAnchorPos;
            if (inForwardCheck || inBackwardCheck)
            {
                // Load in levels.
                Func<LevelButton, bool> predicate = (LevelButton button) => button.gameObject.activeSelf;
                int levelNumber = inForwardCheck ? loadInFirst.LevelButtons.Last(predicate).LevelNumber : loadInFirst.LevelButtons.First(predicate).LevelNumber;
                _loadingLevels = StartCoroutine(LoadInLayout(levelNumber, inForwardCheck));
            }

            float outSecondAnchorPos = loadOutSecond.RectTransform.anchoredPosition[(int) _direction];
            if (lookForward)
            {
                outSecondAnchorPos += _direction == LevelSelectDirection.HORIZONTAL
                    ? _compositions.First().EntryLayout.RectTransform.rect.width
                    : _compositions.First().EntryLayout.RectTransform.rect.height;
            }
            else
            {
                outSecondAnchorPos -= _direction == LevelSelectDirection.HORIZONTAL
                    ? _compositions.First().EntryLayout.RectTransform.rect.width
                    : _compositions.First().EntryLayout.RectTransform.rect.height;
            }
            
            bool outForwardCheck = lookForward && absoluteScroll > outSecondAnchorPos;
            bool outBackwardCheck = !lookForward && absoluteScroll < outSecondAnchorPos;
            if (outForwardCheck || outBackwardCheck)
            {
                // Load out levels.
                RemoveLayout(loadOutFirst);
            }
        }

        /// <summary>
        /// Removes the given layout from the level select.
        /// </summary>
        /// <param name="layout">Layout to remove.</param>
        private void RemoveLayout(LevelSelectLayout layout)
        {
            _layoutInstances.Remove(layout);
            for (int i = 0; i < layout.LevelButtons.Count; i++)
                layout.LevelButtons[i].OnClickedThis -= LevelButtonPressed;
            
            Destroy(layout.gameObject);
            LayoutRemoved?.Invoke();
        }

        /// <summary>
        /// Sets up the given layout with the given level data.
        /// </summary>
        /// <param name="layout">The layout to setup.</param>
        /// <param name="data">The data to use for setting up the level.</param>
        private void SetupLayout(LevelSelectLayout layout, LevelData[] data)
        {
            if(_direction == LevelSelectDirection.HORIZONTAL)
                layout.RectTransform.SetAnchor(RectAnchor.MIDDLE_LEFT, true);
            if(_direction == LevelSelectDirection.VERTICAL)
                layout.RectTransform.SetAnchor(RectAnchor.BOTTOM_CENTER, true);
            
            for (int i = 0; i < layout.LevelButtons.Count; i++)
            {
                bool shouldEnableButton = i < data.Length;
                layout.LevelButtons[i].gameObject.SetActive(shouldEnableButton);
                if (!shouldEnableButton)
                    continue;
                
                layout.LevelButtons[i].LevelNumber = data[i].LevelNumber;
                float normalized = data[i].Score / data[i].MaxScore;
                
                layout.LevelButtons[i].EnableStars(Mathf.FloorToInt(normalized * 3));
                layout.LevelButtons[i].IsLocked = data[i].IsLocked;

                bool currLocked = data[i].IsLocked;
                bool nextLocked = i < data.Length - 1 && data[i + 1].IsLocked;
                layout.LevelButtons[i].IsNextLevel = !currLocked && nextLocked;
                layout.LevelButtons[i].OnClickedThis += LevelButtonPressed;
            }
            LayoutSetup?.Invoke();
        }

        /// <summary>
        /// Called when a level button is pressed.
        /// </summary>
        /// <param name="button">The button that was pressed.</param>
        private void LevelButtonPressed(LevelButton button) => ButtonPressed?.Invoke(button);
    }
}