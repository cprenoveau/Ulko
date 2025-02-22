using UnityEditor;
using UnityEditor.UI;

namespace Ulko.UI
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(PointedButton))]
    public class PointedButtonEditor : ButtonEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("pointer"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("flash"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("background"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bgDefaultColor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bgSelectedColor"));

            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }
    }
}