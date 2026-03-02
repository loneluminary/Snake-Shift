using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DTT.LevelSelect
{
    /// <summary>
    /// Helps with drawing a path for the Level Select component.
    /// </summary>
    [RequireComponent(typeof(UILineRenderer))]
    public class LevelPathDrawer : MonoBehaviour
    {
        /// <summary>
        /// The line renderer used to draw a path.
        /// </summary>
        [SerializeField]
        [Tooltip("The line renderer used to draw a path.")]
        private UILineRenderer _lineRenderer;

        /// <summary>
        /// The resolution of the path for the Bezier curve.
        /// </summary>
        [SerializeField]
        [Range(0, 64)]
        [Tooltip("The resolution of the path for the Bezier curve.")]
        private int _pathResolution = 32;

        /// <summary>
        /// The Level Select component for which to draw the path between the levels.
        /// </summary>
        [SerializeField]
        [Tooltip("The Level Select component for which to draw the path between the levels.")]
        private LevelSelect _levelSelect;

        /// <summary>
        /// Subscribes listeners.
        /// </summary>
        private void OnEnable()
        {
            _levelSelect.LayoutSetup += OnLayoutSetup;
            _levelSelect.LayoutRemoved += OnLayoutRemoved;
        }

        /// <summary>
        /// Unsubscribes listeners.
        /// </summary>
        private void OnDisable()
        {
            _levelSelect.LayoutSetup -= OnLayoutSetup;
            _levelSelect.LayoutRemoved -= OnLayoutRemoved;
        }

        /// <summary>
        /// Called when a layout has been setup and redraws the path.
        /// </summary>
        private void OnLayoutSetup() => Draw(_levelSelect.LayoutInstances);
        
        /// <summary>
        /// Called when a layout has been removed and redraws the path.
        /// </summary>
        private void OnLayoutRemoved() => Draw(_levelSelect.LayoutInstances);

        /// <summary>
        /// Draws a path between all the LevelButtons in the given LevelSelectLayouts.
        /// </summary>
        /// <param name="instances">Used for drawing a path between its LevelButtons.</param>
        public void Draw(IEnumerable<LevelSelectLayout> instances)
        {
            Vector2[] positions = instances.SelectMany(instance => instance.LevelButtons).Where(button => button.gameObject.activeSelf).Select(button => (Vector2)transform.parent.InverseTransformPoint(button.transform.position)).ToArray();
            BezierUtils.GetCurveControlPoints(positions, out Vector2[] firstControlPoints, out Vector2[] secondControlPoints);
            
            _lineRenderer.Clear();

            for (int i = 0; i < positions.Length - 1; i++)
            {
                for (int j = 0; j < _pathResolution; j++)
                {
                    Vector2 pos = BezierUtils.CubicBezier(positions[i], positions[i + 1], firstControlPoints[i],
                        secondControlPoints[i], j / (float)_pathResolution);
                    _lineRenderer.Add(pos);
                }
            }
        }
    }
}