using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ToggleLeftAttribute))]
public class ToggleLeftDrawer : PropertyDrawer
{
    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Now draw the property as a Slider or an IntSlider based on whether it's a float or integer.
        if (property.propertyType == SerializedPropertyType.Boolean)
            property.boolValue = EditorGUI.ToggleLeft(position, label.text, property.boolValue);
        else
            EditorGUI.PropertyField(position, property, label); // EditorGUI.LabelField(position, label.text, "Use ToogleLeft with boolean values.");
    }
}