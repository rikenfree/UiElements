using TMPro.EditorUtilities;
using UnityEditor;
using UnityEngine;

namespace Augmentus.UIElements.Editors
{
    [CustomEditor(typeof(AugmentusTextMeshPro), true)]
    [CanEditMultipleObjects]
    public class AugmentusTextMeshProEditor : TMP_EditorPanelUI
    {
        private SerializedProperty _typographyAsset;
        private SerializedProperty _roleAssetProp;
        private SerializedProperty _textEmphasis;
        private SerializedProperty _colorManagerToken;

        protected override void OnEnable()
        {
            base.OnEnable();

            _typographyAsset = serializedObject.FindProperty("typographyAsset");
            _roleAssetProp = serializedObject.FindProperty("role");
            _textEmphasis = serializedObject.FindProperty("textEmphasis");
            _colorManagerToken = serializedObject.FindProperty("colorManagerToken");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            GUILayout.Label(new GUIContent("<b>Custom Typography</b>"), TMP_UIStyleManager.sectionHeader);
            EditorGUILayout.PropertyField(_typographyAsset);
            EditorGUILayout.PropertyField(_roleAssetProp);
            EditorGUILayout.PropertyField(_textEmphasis);

            if (((AugmentusTextMeshPro)target).Emphasis == TextEmphasis.ColorManager)
            {
                EditorGUILayout.PropertyField(_colorManagerToken);
            }

            EditorGUILayout.Space();
            if (serializedObject.ApplyModifiedProperties() || m_HavePropertiesChanged)
            {
                ApplyTypography();
            }

            if (GUILayout.Button("Force Update Typography"))
            {
                ApplyTypography();
            }
        }

        private void ApplyTypography()
        {
            foreach (var obj in targets)
            {
                var textComp = (AugmentusTextMeshPro)obj;
                textComp.ApplyTypography(textComp.Role, textComp.TypographyAsset);
                textComp.ApplyEmphasis(textComp.Emphasis, textComp.TypographyAsset);
                EditorUtility.SetDirty(textComp);
            }
        }
    }
}