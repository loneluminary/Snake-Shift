using UnityEngine;
using UnityEngine.EventSystems;

namespace DTT.MinigameBase.Handles
{
    /// <summary>
    /// A handle used for moving the object on the mouse.
    /// This makes sure the object isn't snapped to mouse but just translates with it.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class MoveHandle : Handle
    {
        /// <summary>
        /// The transform of the object to move.
        /// </summary>
        private RectTransform _rectTransform;

        /// <summary>
        /// Retrieves component references.
        /// </summary>
        private void Awake() => _rectTransform = (RectTransform)transform;

        /// <summary>
        /// Subscribes to events.
        /// </summary>
        private void OnEnable()
        {
            Drag += OnDrag;
        }

        /// <summary>
        /// Cleans up subscribed events.
        /// </summary>
        private void OnDisable()
        {
            Drag -= OnDrag;
        }

        /// <summary>
        /// Adds the position change to the object.
        /// </summary>
        /// <param name="eventData">The data about the pointer event.</param>
        private void OnDrag(PointerEventData eventData) => _rectTransform.position += (Vector3)eventData.delta;
    }
}