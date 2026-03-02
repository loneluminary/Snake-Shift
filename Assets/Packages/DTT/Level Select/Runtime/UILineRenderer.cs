using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DTT.LevelSelect
{
    /// <summary>
    /// A line renderer that can be used in the UI to display connections between a set of points.
    /// </summary>
    public class UILineRenderer : Graphic, IList<Vector2>
    {
        /// <summary>
        /// The points to draw line between.
        /// </summary>
        [SerializeField]
        private List<Vector2> _points = new List<Vector2>();

        /// <summary>
        /// The thickness of the line.
        /// </summary>
        [SerializeField]
        private float _thickness;

        /// <summary>
        /// Draws all the segments of the line between each point.
        /// </summary>
        /// <param name="vh">The vertex helper for appending points to.</param>
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (_points.Count < 2)
                return;

            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = color;

            for (int i = 0; i < _points.Count - 1; i++)
                DrawSegment(_points[i], _points[i + 1], vh);

            for (int i = 0; i < _points.Count - 1; i++)
            {
                int offset = i * 4;
                vh.AddTriangle(offset + 0, offset + 1, offset + 2);
                vh.AddTriangle(offset + 2, offset + 3, offset + 0);
            }
        }

        /// <summary>
        /// Draws a line segment between two points and adds the indices to the vertex helper.
        /// </summary>
        /// <param name="pointA">The start point.</param>
        /// <param name="pointB">The end points.</param>
        /// <param name="vh">The vertex helper to add indices to.</param>
        private void DrawSegment(Vector2 pointA, Vector2 pointB, VertexHelper vh)
        {
            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = color;

            float angle = GetAngle(pointA, pointB);
            vertex.position = pointA + new Vector2(Mathf.Cos(angle - Mathf.PI / 2) * _thickness / 2,
                Mathf.Sin(angle - Mathf.PI / 2) * _thickness / 2);
            vertex.position += new Vector3(Mathf.Cos(angle - Mathf.PI) * _thickness / 2,
                Mathf.Sin(angle - Mathf.PI) * _thickness / 2);
            vh.AddVert(vertex);

            vertex.position = pointA + new Vector2(Mathf.Cos(angle + Mathf.PI / 2) * _thickness / 2,
                Mathf.Sin(angle + Mathf.PI / 2) * _thickness / 2);
            vertex.position += new Vector3(Mathf.Cos(angle - Mathf.PI) * _thickness / 2,
                Mathf.Sin(angle - Mathf.PI) * _thickness / 2);
            vh.AddVert(vertex);

            vertex.position = pointB + new Vector2(Mathf.Cos(angle + Mathf.PI / 2) * _thickness / 2,
                Mathf.Sin(angle + Mathf.PI / 2) * _thickness / 2);
            vertex.position -= new Vector3(Mathf.Cos(angle - Mathf.PI) * _thickness / 2,
                Mathf.Sin(angle - Mathf.PI) * _thickness / 2);
            vh.AddVert(vertex);

            vertex.position = pointB + new Vector2(Mathf.Cos(angle - Mathf.PI / 2) * _thickness / 2,
                Mathf.Sin(angle - Mathf.PI / 2) * _thickness / 2);
            vertex.position -= new Vector3(Mathf.Cos(angle - Mathf.PI) * _thickness / 2,
                Mathf.Sin(angle - Mathf.PI) * _thickness / 2);
            vh.AddVert(vertex);
        }

        /// <summary>
        /// Retrieves the angle between two points.
        /// </summary>
        /// <param name="a">Start point.</param>
        /// <param name="b">End point.</param>
        /// <returns>The angle in between.</returns>
        private float GetAngle(Vector2 a, Vector2 b) => Mathf.Atan2(b.y - a.y, b.x - a.x);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public IEnumerator<Vector2> GetEnumerator() => _points.GetEnumerator();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        IEnumerator IEnumerable.GetEnumerator() => _points.GetEnumerator();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="item"><inheritdoc/></param>
        public void Add(Vector2 item)
        {
            SetVerticesDirty();
            _points.Add(item);
        }

        /// <summary>
        /// Adds a range of points to the line renderer.
        /// </summary>
        /// <param name="items">The collection of points to add.</param>
        public void AddRange(Vector2[] items)
        {
            SetVerticesDirty();
            _points.AddRange(items);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Clear() => _points.Clear();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="item"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public bool Contains(Vector2 item) => _points.Contains(item);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="array"><inheritdoc/></param>
        /// <param name="arrayIndex"><inheritdoc/></param>
        public void CopyTo(Vector2[] array, int arrayIndex) => _points.CopyTo(array, arrayIndex);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="item"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public bool Remove(Vector2 item) => _points.Remove(item);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int Count => _points.Count;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="item"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public int IndexOf(Vector2 item) => _points.IndexOf(item);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="index"><inheritdoc/></param>
        /// <param name="item"><inheritdoc/></param>
        public void Insert(int index, Vector2 item) => _points.Insert(index, item);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="index"><inheritdoc/></param>
        public void RemoveAt(int index) => _points.RemoveAt(index);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="index"><inheritdoc/></param>
        public Vector2 this[int index]
        {
            get => _points[index];
            set => _points[index] = value;
        }
    }
}
