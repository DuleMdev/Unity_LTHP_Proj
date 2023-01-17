using UnityEngine;
using UnityEditor;
using System.Collections;


[CustomPropertyDrawer(typeof(LanguageController.LanguageData))]
public class LanguageDataDrawer : PropertyDrawer
{
    // Kirajzoljuk a property-t a position által megadott területen belülre
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {

        //Debug.Log(position);

        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        Rect decrementPosition = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent(" "));



        SerializedProperty serializedProperty = property.FindPropertyRelative("languageID");

        EditorGUI.PropertyField(new Rect(position.x, position.y + 9, decrementPosition.x - position.x - 2, EditorGUIUtility.singleLineHeight), serializedProperty, new GUIContent(serializedProperty.tooltip));
        //Debug.Log("" + serializedProperty.tooltip);

        EditorGUI.indentLevel--;
        EditorGUI.PropertyField(new Rect(decrementPosition.x, decrementPosition.y + 9, decrementPosition.width, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("data"), GUIContent.none);

        EditorGUI.PropertyField(new Rect(decrementPosition.x, position.y + 11 + EditorGUIUtility.singleLineHeight, decrementPosition.width, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("dataPictures"), GUIContent.none);


        /*
                EditorGUI.PropertyField(new Rect(position.x, position.y, decrementPosition.x - position.x - 2, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("languageID"), GUIContent.none);
                EditorGUI.indentLevel--;
                EditorGUI.PropertyField(new Rect(decrementPosition.x, decrementPosition.y, decrementPosition.width, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("data"), GUIContent.none);
                */

        EditorGUI.EndProperty();
    }

    // A rendszer megkérdezi, hogy milyen magasságú helyre lenne szükségünk
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 2.5f;
    }

}
