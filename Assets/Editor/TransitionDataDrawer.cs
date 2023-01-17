using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomPropertyDrawer(typeof(ScreenController.TransitionData))]
class TransitionDataDrawer : PropertyDrawer
{

    // Kirajzoljuk a property-t a position által megadott területen belülre
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);
        
        // Draw label
        Rect decrementPosition = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        EditorGUI.PropertyField(new Rect(decrementPosition.x, decrementPosition.y, decrementPosition.width, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("screenTransition"), GUIContent.none);

        // Kiszámoljuk a következő sor kezdetét
        Rect nextRow = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, position.height);

        EditorGUI.indentLevel++;
        decrementPosition = EditorGUI.PrefixLabel(nextRow, new GUIContent("Effect Speed"));
        EditorGUI.indentLevel--;
        EditorGUI.PropertyField(new Rect(decrementPosition.x, decrementPosition.y, decrementPosition.width, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("effectSpeed"), GUIContent.none);

        nextRow = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 2, position.width, position.height);

        // Lekérdezzük a beállított képernyő váltó effect-et
        SerializedProperty serializedPorperty = property.FindPropertyRelative("screenTransition");
        ScreenController.ScreenTransition screenTransition = (ScreenController.ScreenTransition)serializedPorperty.enumValueIndex;

        switch (screenTransition)
        {
            case ScreenController.ScreenTransition.FadeColor:
                EditorGUI.indentLevel++;
                decrementPosition = EditorGUI.PrefixLabel(nextRow, new GUIContent("Color"));
                EditorGUI.indentLevel--;
                EditorGUI.PropertyField(new Rect(decrementPosition.x, decrementPosition.y, decrementPosition.width, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("fadeColor"), GUIContent.none);
                break;
            case ScreenController.ScreenTransition.FadePicture:
                EditorGUI.indentLevel++;
                decrementPosition = EditorGUI.PrefixLabel(nextRow, new GUIContent("Picture Index"));
                EditorGUI.indentLevel--;
                EditorGUI.PropertyField(new Rect(decrementPosition.x, decrementPosition.y, decrementPosition.width, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("pictureIndex"), GUIContent.none);
                break;
            case ScreenController.ScreenTransition.Slide:
                EditorGUI.indentLevel++;
                decrementPosition = EditorGUI.PrefixLabel(nextRow, new GUIContent("Direction"));
                EditorGUI.indentLevel--;
                EditorGUI.PropertyField(new Rect(decrementPosition.x, decrementPosition.y, decrementPosition.width, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("direction"), GUIContent.none);

                nextRow = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 3, position.width, position.height);

                EditorGUI.indentLevel++;
                decrementPosition = EditorGUI.PrefixLabel(nextRow, new GUIContent("Ease Type"));
                EditorGUI.indentLevel--;
                EditorGUI.PropertyField(new Rect(decrementPosition.x, decrementPosition.y, decrementPosition.width, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("easeType"), GUIContent.none);
                break;
        }


        EditorGUI.EndProperty();
    }

    // A rendszer megkérdezi, hogy milyen magasságú helyre lenne szükségünk, ezt a visszatérő értékben meg kell mondanunk neki
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Lekérdezzük a beállított képernyő váltó effect-et
        SerializedProperty serializedPorperty = property.FindPropertyRelative("screenTransition");
        ScreenController.ScreenTransition screenTransition = (ScreenController.ScreenTransition)serializedPorperty.enumValueIndex;

        // Meghatározzuk, hogy a különböző beállításokhoz hány sorra van szükségünk
        int row = 0;
        switch (screenTransition)
        {
            case ScreenController.ScreenTransition.FadeColor:
                row = 3;
                break;
            case ScreenController.ScreenTransition.FadePicture:
                row = 3;
                break;
            case ScreenController.ScreenTransition.Scale:
                row = 2;
                break;
            case ScreenController.ScreenTransition.Slide:
                row = 4;
                break;
        }

        // Ha a képernyőváltó effect skálázás, akkor 1 sor kérünk, ha nem, akkor két sor helyet kérünk, mivel akkor további adatokat is meg kell jeleníteni
        return EditorGUIUtility.singleLineHeight * row;
    }
}
