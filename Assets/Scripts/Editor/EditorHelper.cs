#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class EditorHelper
    {
        public static class EditorMenuHelpers
        {
            [MenuItem("Window/Open Data Path")]
            private static void OpenPersistentDataFolder()
            {
                string path = Application.persistentDataPath;

                if (Directory.Exists(path))
                {
                    EditorUtility.RevealInFinder(path);
                }
                else
                {
                    Debug.LogError("Data folder not found: " + path);
                }
            }
        }
    }
}
#endif