using UnityEngine;
using UnityEditor;

namespace Ulko.Data.Abilities
{
    [CustomPropertyDrawer(typeof(AbilityNode))]
    public class AbilityNodeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = 0;

            var forceValidTargetProp = property.FindPropertyRelative("forceValidTarget");
            var sequenceProperty = property.FindPropertyRelative("applySequence");
            var effectsProperty = property.FindPropertyRelative("effects");

            height += EditorGUIUtility.singleLineHeight;
            height += EditorGUI.GetPropertyHeight(forceValidTargetProp);
            height += EditorGUIUtility.singleLineHeight;

            height += EditorGUI.GetPropertyHeight(sequenceProperty);
            height += EditorGUI.GetPropertyHeight(effectsProperty);

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var forceValidTargetProp = property.FindPropertyRelative("forceValidTarget");
            var sequenceProperty = property.FindPropertyRelative("applySequence");
            var effectsProperty = property.FindPropertyRelative("effects");

            EditorGUI.BeginProperty(position, GUIContent.none, property);

            position = new Rect(position.x, position.y + 5, position.width, position.height);

            float totalHeight = GetPropertyHeight(property, label);
            EditorGUI.DrawRect(new Rect(position.x - 5, position.y, position.width + 8, totalHeight), new Color(0.2f, 0.2f, 0.2f, 1f));

            float height = 0;
            EditorGUI.LabelField(new Rect(position.x, position.y + height, position.width, EditorGUIUtility.singleLineHeight), label);
            height += EditorGUIUtility.singleLineHeight;

            EditorGUI.PropertyField(new Rect(position.x, position.y + height, position.width, EditorGUI.GetPropertyHeight(forceValidTargetProp)), forceValidTargetProp, new GUIContent("Force Valid Target"), false);
            height += EditorGUI.GetPropertyHeight(forceValidTargetProp);
            height += EditorGUIUtility.singleLineHeight;

            EditorGUI.PropertyField(new Rect(position.x, position.y + height, position.width, EditorGUI.GetPropertyHeight(sequenceProperty)), sequenceProperty, label, true);
            height += EditorGUI.GetPropertyHeight(sequenceProperty);

            EditorGUI.PropertyField(new Rect(position.x, position.y + height, position.width, EditorGUI.GetPropertyHeight(effectsProperty)), effectsProperty, label, true);
            height += EditorGUI.GetPropertyHeight(effectsProperty);

            EditorGUI.EndProperty();

            property.serializedObject.ApplyModifiedProperties();
        }
    }
}