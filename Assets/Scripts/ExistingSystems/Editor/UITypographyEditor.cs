using UnityEditor;
using UnityEngine;

namespace Augmentus.UIElements.Editors
{
    [CustomEditor(typeof(UITypography), true)]
    [CanEditMultipleObjects]
    public class UITypographyFromJsonEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space(24);

            if (GUILayout.Button("Populate Typography from JSON (Only use after Font Weights are filled in)", GUILayout.Height(48)))
            {
                foreach (UITypography typography in targets)
                {
                    typography.LoadFromJson();
                }
            }

            EditorGUILayout.Space(24);

            base.OnInspectorGUI();
        }
    }
}