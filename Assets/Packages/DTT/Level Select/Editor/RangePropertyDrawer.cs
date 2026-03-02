using UnityEditor;
using UnityEngine;

namespace DTT.LevelSelect.Editor
{
    /// <summary>
    /// Handles drawing a min and max int property for custom editors.
    /// </summary>
    [CustomPropertyDrawer(typeof(Range))]
    public class RangePropertyDrawer : PropertyDrawer
    {
        /// <summary>
        /// Draws a min and max int property in the editor.
        /// </summary>
        /// <param name="position">Position of the property.</param>
        /// <param name="property">SerializedProperty of type <see cref="Range"/>.</param>
        /// <param name="label">Label displayed next to the property.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            int start = property.FindPropertyRelative("start").intValue;
            int end = property.FindPropertyRelative("length").intValue + start;

            position = EditorGUI.PrefixLabel(position, label);

            const int SPACING = 12;
            
            Rect startPosition = position;
            startPosition.width /= 2;
            startPosition.width -= SPACING / 2;

            Rect endPosition = startPosition;
            endPosition.x += startPosition.width + SPACING;
            
            start = EditorGUI.IntField(startPosition, start);
            end = EditorGUI.IntField(endPosition, end);
            
            property.FindPropertyRelative("length").intValue = end - start;
            property.FindPropertyRelative("start").intValue = start;
        }
    }
}