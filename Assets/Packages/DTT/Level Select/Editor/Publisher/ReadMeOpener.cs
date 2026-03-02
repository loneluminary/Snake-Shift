#if UNITY_EDITOR

using DTT.PublishingTools;
using UnityEditor;

namespace DTT.LevelSelect.Editor
{
    /// <summary>
    /// Class that handles opening the editor window for the Level Select package.
    /// </summary>
    internal static class ReadMeOpener
    {
        /// <summary>
        /// Opens the readme for this package.
        /// </summary>
        [MenuItem("Tools/DTT/Level Select/ReadMe")]
        private static void OpenReadMe() => DTTEditorConfig.OpenReadMe("dtt.level-select");
    }
}
#endif