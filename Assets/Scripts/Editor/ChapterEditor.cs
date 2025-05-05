using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using HotChocolate.Utils;

namespace Ulko.Data.Timeline
{
    [CustomEditor(typeof(Chapter))]
    public class ChapterEditor : Editor
    {
        private Dictionary<string, ReorderableList> milestoneLists = new Dictionary<string, ReorderableList>();
        private IEnumerable<Instantiator> milestoneInstantiators;
        private int milestoneTypeIndex;

        private float spacing = EditorGUIUtility.singleLineHeight / 2;

        public override void OnInspectorGUI()
        {
            var chapter = serializedObject.targetObject as Chapter;
            milestoneInstantiators = chapter.MilestoneInstantiators();

            EditorGUILayout.BeginVertical();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("chapterId"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("chapterName"));

            var milestonesProperty = serializedObject.FindProperty("milestones");

            if (!milestoneLists.ContainsKey(milestonesProperty.propertyPath))
            {
                BuildReorderableList(milestonesProperty);
            }

            milestoneLists[milestonesProperty.propertyPath].DoLayoutList();

            milestoneTypeIndex = EditorGUILayout.Popup(milestoneTypeIndex, milestoneInstantiators.Select(s => s.displayName).ToArray());

            if (GUILayout.Button("Add", GUILayout.Width(200)))
            {
                OnAddCallback(milestoneLists[milestonesProperty.propertyPath]);
            }

            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        private float ElementHeightCallback(SerializedProperty property, int index)
        {
            //Gets the height of the element. This also accounts for properties that can be expanded, like structs.
            float propertyHeight = EditorGUI.GetPropertyHeight(milestoneLists[property.propertyPath].serializedProperty.GetArrayElementAtIndex(index), true);
            return propertyHeight + spacing;
        }

        private void OnAddCallback(ReorderableList list)
        {
            var index = list.serializedProperty.arraySize;
            list.serializedProperty.arraySize++;
            list.index = index;
            var element = list.serializedProperty.GetArrayElementAtIndex(index);

            var instantiator = milestoneInstantiators.ElementAt(milestoneTypeIndex);
            element.managedReferenceValue = instantiator.Instantiate();
        }

        private void BuildReorderableList(SerializedProperty property)
        {
            milestoneLists.Add(property.propertyPath, new ReorderableList(property.serializedObject, property,
                true, true, true, true)
            {
                drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, "Milestones");
                }
            });

            milestoneLists[property.propertyPath].elementHeightCallback += (int index) => { return ElementHeightCallback(property, index); };
            milestoneLists[property.propertyPath].onAddCallback += OnAddCallback;

            milestoneLists[property.propertyPath].drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var prop = milestoneLists[property.propertyPath].serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(new Rect(rect.x + 15, rect.y, rect.width - 15, rect.height), prop, new GUIContent(prop.FindPropertyRelative("name").stringValue + " (" +GetManagedTypeName(prop) + ")"), true);
            };
        }

        private string GetManagedTypeName(SerializedProperty property)
        {
            var tokens = property.managedReferenceFullTypename.Split('.');
            if (tokens.Length > 0)
                return tokens[tokens.Length - 1];

            return property.managedReferenceFullTypename;
        }
    }
}