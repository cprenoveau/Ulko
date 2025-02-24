using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using HotChocolate.Utils;

namespace Ulko.Data.Abilities
{
    [CustomPropertyDrawer(typeof(AbilitySequence))]
    public class AbilitySequenceDrawer : PropertyDrawer
    {
        private readonly Dictionary<string, ReorderableList> stepLists = new();
        private IEnumerable<Instantiator> stepInstantiators;
        private int stepTypeIndex;

        private float height = 0;
        private float spacing = EditorGUIUtility.singleLineHeight / 2;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float listHeight = 0;
            var stepsProperty = property.FindPropertyRelative("steps");

            if (stepLists.ContainsKey(stepsProperty.propertyPath))
            {
                listHeight = stepLists[stepsProperty.propertyPath].GetHeight();
            }

            return height + spacing + listHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            height = 0;

            stepInstantiators = AbilityAsset.StepInstantiators();

            EditorGUI.BeginProperty(position, GUIContent.none, property);

            var stepsProperty = property.FindPropertyRelative("steps");

            if (!stepLists.ContainsKey(stepsProperty.propertyPath))
            {
                BuildReorderableList(stepsProperty, property.displayName);
            }

            stepLists[stepsProperty.propertyPath].DoList(new Rect(position.x, position.y + height, position.width, TotalListHeight(stepsProperty)));
            float listHeight = stepLists[stepsProperty.propertyPath].GetHeight();

            float buttonSize = 200f;
            stepTypeIndex = EditorGUI.Popup(new Rect(position.x, position.y + height + listHeight, position.width - buttonSize - spacing, EditorGUIUtility.singleLineHeight), stepTypeIndex, stepInstantiators.Select(s => s.displayName).ToArray());

            if (GUI.Button(new Rect(position.x + position.width - buttonSize, position.y + height + listHeight, buttonSize, EditorGUIUtility.singleLineHeight), "Add"))
            {
                OnAddCallback(stepLists[stepsProperty.propertyPath]);
            }

            height += EditorGUIUtility.singleLineHeight;

            EditorGUI.EndProperty();

            property.serializedObject.ApplyModifiedProperties();
        }

        private float TotalListHeight(SerializedProperty property)
        {
            float height = 0;
            for (int i = 0; i < property.arraySize; ++i)
            {
                height += ElementHeightCallback(property, i);
            }

            return height;
        }

        private float ElementHeightCallback(SerializedProperty property, int index)
        {
            //Gets the height of the element. This also accounts for properties that can be expanded, like structs.
            float propertyHeight = EditorGUI.GetPropertyHeight(stepLists[property.propertyPath].serializedProperty.GetArrayElementAtIndex(index), true);
            return propertyHeight + spacing;
        }

        private void OnAddCallback(ReorderableList list)
        {
            var index = list.serializedProperty.arraySize;
            list.serializedProperty.arraySize++;
            list.index = index;
            var element = list.serializedProperty.GetArrayElementAtIndex(index);

            var instantiator = stepInstantiators.ElementAt(stepTypeIndex);
            element.managedReferenceValue = instantiator.Instantiate();
        }

        private void BuildReorderableList(SerializedProperty property, string label)
        {
            stepLists.Add(property.propertyPath, new ReorderableList(property.serializedObject, property,
                true, true, true, true)
            {
                drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, label);
                }
            });

            stepLists[property.propertyPath].elementHeightCallback += (int index) => { return ElementHeightCallback(property, index); };
            stepLists[property.propertyPath].onAddCallback += OnAddCallback;

            stepLists[property.propertyPath].drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var prop = stepLists[property.propertyPath].serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(new Rect(rect.x + 15, rect.y, rect.width - 15, rect.height), prop, new GUIContent(GetManagedTypeName(prop)), true);
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