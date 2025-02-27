using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;
using System;

namespace Ulko.Data.Abilities
{
    [CustomEditor(typeof(AbilityAsset))]
    public class AbilityEditor : Editor
    {
        private readonly Dictionary<string, ReorderableList> nodeLists = new();

        private float spacing = EditorGUIUtility.singleLineHeight / 2;

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("id"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("mainStat"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("target"));

            var nodesProperty = serializedObject.FindProperty("nodes");

            if (!nodeLists.ContainsKey(nodesProperty.propertyPath))
            {
                BuildReorderableList(nodesProperty);
            }

            nodeLists[nodesProperty.propertyPath].DoLayoutList();

            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        private float ElementHeightCallback(SerializedProperty property, int index)
        {
            //Gets the height of the element. This also accounts for properties that can be expanded, like structs.
            float propertyHeight = EditorGUI.GetPropertyHeight(nodeLists[property.propertyPath].serializedProperty.GetArrayElementAtIndex(index), true);
            return propertyHeight + spacing;
        }

        private void OnAddCallback(ReorderableList list)
        {
            var index = list.serializedProperty.arraySize;
            list.serializedProperty.arraySize++;
            list.index = index;

            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            element.managedReferenceValue = Activator.CreateInstance(typeof(AbilityNode));
        }

        private void BuildReorderableList(SerializedProperty property)
        {
            nodeLists.Add(property.propertyPath, new ReorderableList(property.serializedObject, property,
                true, true, true, true)
            {
                drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, "Nodes");
                }
            });

            nodeLists[property.propertyPath].elementHeightCallback += (int index) => { return ElementHeightCallback(property, index); };
            nodeLists[property.propertyPath].onAddCallback += OnAddCallback;

            nodeLists[property.propertyPath].drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var prop = nodeLists[property.propertyPath].serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(new Rect(rect.x + 15, rect.y, rect.width - 15, rect.height), prop, new GUIContent("Node "+(index+1)), true);
            };
        }
    }
}