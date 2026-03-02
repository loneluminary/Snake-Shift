using DTT.PublishingTools;
using UnityEditor;

namespace DTT.LevelSelect.Editor
{
    /// <summary>
    /// Handles drawing the editor for the <see cref="LevelSelect"/> component.
    /// </summary>
    [CustomEditor(typeof(LevelSelect))]
    [DTTHeader("dtt.level-select", "Level Select")]
    public class LevelSelectEditor : DTTInspector
    {
        /// <summary>
        /// Draws the default inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DrawDefaultInspector();
        }
    }
}