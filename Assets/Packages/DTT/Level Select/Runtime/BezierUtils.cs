using System;
using UnityEngine;

namespace DTT.LevelSelect
{
    /// <summary>
    /// Utility methods for working with Beziers.
    /// </summary>
    public static class BezierUtils
    {
        /// <summary>
        /// Computes a point on a cubic Bezier.
        /// </summary>
        /// <param name="pointA">The first point.</param>
        /// <param name="pointB">The second point.</param>
        /// <param name="controlPointA">Control point for point A.</param>
        /// <param name="controlPointB">Control point for point B.</param>
        /// <param name="t">The normalized value for how far on the line the point should be computed (between 0 and 1.)</param>
        /// <returns>Point on the Bezier curve.</returns>
        public static Vector2 CubicBezier(Vector2 pointA, Vector2 pointB, Vector2 controlPointA, Vector2 controlPointB, float t)
        {
            return Mathf.Pow(1 - t, 3) * pointA + 3 * Mathf.Pow(1 - t, 2) * t * controlPointA +
                   3 * (1 - t) * Mathf.Pow(t, 2) * controlPointB + Mathf.Pow(t, 3) * pointB;
        }

        /// <summary>
        /// Get open-ended Bezier Spline Control Points.
        /// </summary>
        /// <param name="knots">Input Knot Bezier spline points.</param>
        /// <param name="firstControlPoints">Output First Control points
        /// array of knots.Length - 1 length.</param>
        /// <param name="secondControlPoints">Output Second Control points
        /// array of knots.Length - 1 length.</param>
        /// <exception cref="ArgumentNullException"><paramref name="knots"/>
        /// parameter must be not null.</exception>
        /// <exception cref="ArgumentException"><paramref name="knots"/>
        /// array must contain at least two points.</exception>
        public static void GetCurveControlPoints(Vector2[] knots, out Vector2[] firstControlPoints, out Vector2[] secondControlPoints)
        {
            if (knots == null)
                throw new ArgumentNullException(nameof(knots));
            int n = knots.Length - 1;
            if (n < 1)
                throw new ArgumentException
                ("At least two knot points required", nameof(knots));
            if (n == 1)
            { // Special case: Bezier curve should be a straight line.
                firstControlPoints = new Vector2[1];
                // 3P1 = 2P0 + P3.
                firstControlPoints[0].x = (2 * knots[0].x + knots[1].x) / 3;
                firstControlPoints[0].y = (2 * knots[0].y + knots[1].y) / 3;

                secondControlPoints = new Vector2[1];
                // P2 = 2P1 – P0.
                secondControlPoints[0].x = 2 *
                    firstControlPoints[0].x - knots[0].x;
                secondControlPoints[0].y = 2 *
                    firstControlPoints[0].y - knots[0].y;
                return;
            }

            // Calculate first Bezier control points.
            // Right hand side vector.
            float[] rhs = new float[n];

            // Set right hand side X values.
            for (int i = 1; i < n - 1; ++i)
                rhs[i] = 4 * knots[i].x + 2 * knots[i + 1].x;
            rhs[0] = knots[0].x + 2 * knots[1].x;
            rhs[n - 1] = (8 * knots[n - 1].x + knots[n].x) / 2.0f;
            // Get first control points X-values.
            float[] x = GetFirstControlPoints(rhs);

            // Set right hand side Y values.
            for (int i = 1; i < n - 1; ++i)
                rhs[i] = 4 * knots[i].y + 2 * knots[i + 1].y;
            rhs[0] = knots[0].y + 2 * knots[1].y;
            rhs[n - 1] = (8 * knots[n - 1].y + knots[n].y) / 2.0f;
            // Get first control points Y-values.
            float[] y = GetFirstControlPoints(rhs);

            // Fill output arrays.
            firstControlPoints = new Vector2[n];
            secondControlPoints = new Vector2[n];
            for (int i = 0; i < n; ++i)
            {
                // First control point.
                firstControlPoints[i] = new Vector2(x[i], y[i]);
                // Second control point.
                if (i < n - 1)
                    secondControlPoints[i] = new Vector2(2 * knots
                        [i + 1].x - x[i + 1], 2 *
                        knots[i + 1].y - y[i + 1]);
                else
                    secondControlPoints[i] = new Vector2((knots
                        [n].x + x[n - 1]) / 2,
                        (knots[n].y + y[n - 1]) / 2);
            }
        }

        /// <summary>
        /// Solves a tridiagonal system for one of coordinates (x or y)
        /// of first Bezier control points.
        /// </summary>
        /// <param name="rhs">Right hand side vector.</param>
        /// <returns>Solution vector.</returns>
        private static float[] GetFirstControlPoints(float[] rhs)
        {
            int n = rhs.Length;

            // Solution vector.
            float[] x = new float[n];

            // Temp workspace.
            float[] tmp = new float[n]; 

            float b = 2.0f;
            x[0] = rhs[0] / b;

            // Decomposition and forward substitution.
            for (int i = 1; i < n; i++) 
            {
                tmp[i] = 1 / b;
                b = (i < n - 1 ? 4.0f : 3.5f) - tmp[i];
                x[i] = (rhs[i] - x[i - 1]) / b;
            }

            // Backsubstitution.
            for (int i = 1; i < n; i++)
                x[n - i - 1] -= tmp[n - i] * x[n - i]; 

            return x;
        }
    }
}