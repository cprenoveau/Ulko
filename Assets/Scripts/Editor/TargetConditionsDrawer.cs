using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using HotChocolate.Utils;

namespace Ulko.Data.Abilities
{
    [CustomPropertyDrawer(typeof(CompositeTargetCondition))]
    public class TargetConditionsDrawer : PropertyDrawer
    {
        private readonly Dictionary<string, ReorderableList> conditionLists = new();
        private IEnumerable<Instantiator> conditionInstantiators;
        private int conditionTypeIndex;

        private float height = 0;
        private float spacing = EditorGUIUtility.singleLineHeight / 2;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float listHeight = 0;
            var conditionsProperty = property.FindPropertyRelative("conditions");

            if (conditionLists.ContainsKey(conditionsProperty.propertyPath))
            {
                listHeight = conditionLists[conditionsProperty.propertyPath].GetHeight();
            }

            return height + spacing + listHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            height = 0;

            conditionInstantiators = AbilityAsset.ConditionInstantiators();

            EditorGUI.BeginProperty(position, GUIContent.none, property);

            EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("union"), new GUIContent("Condition Union"));
            height += EditorGUIUtility.singleLineHeight;

            var conditionsProperty = property.FindPropertyRelative("conditions");

            if (!conditionLists.ContainsKey(conditionsProperty.propertyPath))
            {
                BuildReorderableList(conditionsProperty);
            }

            conditionLists[conditionsProperty.propertyPath].DoList(new Rect(position.x, position.y + height, position.width, TotalListHeight(conditionsProperty)));
            float listHeight = conditionLists[conditionsProperty.propertyPath].GetHeight();

            float buttonSize = 200f;
            conditionTypeIndex = EditorGUI.Popup(new Rect(position.x, position.y + height + listHeight, position.width - buttonSize - spacing, EditorGUIUtility.singleLineHeight), conditionTypeIndex, conditionInstantiators.Select(s => s.displayName).ToArray());

            if (GUI.Button(new Rect(position.x + position.width - buttonSize, position.y + height + listHeight, buttonSize, EditorGUIUtility.singleLineHeight), "Add"))
            {
                OnAddCallback(conditionLists[conditionsProperty.propertyPath]);
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
            float propertyHeight = EditorGUI.GetPropertyHeight(conditionLists[property.propertyPath].serializedProperty.GetArrayElementAtIndex(index), true);
            return propertyHeight + spacing;
        }

        private void OnAddCallback(ReorderableList list)
        {
            var index = list.serializedProperty.arraySize;
            list.serializedProperty.arraySize++;
            list.index = index;
            var element = list.serializedProperty.GetArrayElementAtIndex(index);

            var instantiator = conditionInstantiators.ElementAt(conditionTypeIndex);
            element.managedReferenceValue = instantiator.Instantiate();
        }

        private void BuildReorderableList(SerializedProperty property)
        {
            conditionLists.Add(property.propertyPath, new ReorderableList(property.serializedObject, property,
                true, true, true, true)
            {
                drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, "Target Conditions");
                }
            });

            conditionLists[property.propertyPath].elementHeightCallback += (int index) => { return ElementHeightCallback(property, index); };
            conditionLists[property.propertyPath].onAddCallback += OnAddCallback;

            conditionLists[property.propertyPath].drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var prop = conditionLists[property.propertyPath].serializedProperty.GetArrayElementAtIndex(index);
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