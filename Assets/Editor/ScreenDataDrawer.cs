using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomPropertyDrawer(typeof(ScreenController.ScreenData))]
class ScreenDataDrawer : PropertyDrawer
{

    // Kirajzoljuk a property-t a position által megadott területen belülre
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {

        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        Rect decrementPosition = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent(" "));

        EditorGUI.PropertyField(new Rect(position.x, position.y, decrementPosition.x - position.x - 2, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("name"), GUIContent.none);
        EditorGUI.indentLevel--;
        EditorGUI.PropertyField(new Rect(decrementPosition.x, decrementPosition.y, decrementPosition.width, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("screen"), GUIContent.none);

        EditorGUI.EndProperty();
    }

    /*
    // A rendszer megkérdezi, hogy milyen magasságú helyre lenne szükségünk, ezt a visszatérő értékben meg kell mondanunk neki
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Lekérdezzük a beállított képernyő váltó effect-et
        SerializedProperty serializedPorperty = property.FindPropertyRelative("ScreenTransition");
        ScreenController.ScreenTransition screenTransition = (ScreenController.ScreenTransition)serializedPorperty.enumValueIndex;

        // Ha a képernyőváltó effect skálázás, akkor 1 sor kérünk, ha nem, akkor két sor helyet kérünk, mivel akkor további adatokat is meg kell jeleníteni
        return (screenTransition == ScreenController.ScreenTransition.Scale) ? EditorGUIUtility.singleLineHeight * 2 : EditorGUIUtility.singleLineHeight * 3;
    }
    */
}

