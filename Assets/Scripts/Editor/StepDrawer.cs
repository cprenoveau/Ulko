using UnityEngine;
using UnityEditor;

namespace Ulko.Data.Cutscenes
{
    [CustomPropertyDrawer(typeof(Step))]
    public class StepDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + 2;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, GUIContent.none, property);

            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 60;
            EditorGUI.PropertyField(new Rect(position.x, position.y, 100, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("blocking"));

            EditorGUIUtility.labelWidth = 50;
            EditorGUI.PropertyField(new Rect(position.x + 100, position.y, 100, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("delay"));
            EditorGUI.PropertyField(new Rect(position.x + 220, position.y, position.width - 220, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("action"));

            EditorGUIUtility.labelWidth = labelWidth;

            EditorGUI.EndProperty();

            property.serializedObject.ApplyModifiedProperties();
        }
    }
}
