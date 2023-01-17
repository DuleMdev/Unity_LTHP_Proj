using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(ClassYServerCommunication.ListOfRequestTest))]
class ListOfRequestTestDrawer : PropertyDrawer
{
    Rect actLine;

    void PrintProperties(SerializedProperty property)
    {
        property = property.Copy();

        SerializedProperty p = property.Copy();
        string s = "| " + p.CountInProperty();

        p = property.Copy();

        //property.Reset(); // Nem a property, hanem a Script első property-jére állítja a mutatót.

        do
        {
            s = s + "\n";
            for (int i = 0; i < property.depth; i++)
            {
                s = s + "\t";
            }

            s = s + property.name + ":" + property.type;
            s = s + ":" + property.propertyPath;
            s = s + ":" + property.depth;

            if (property.isExpanded)
                s = s + ":EXPANDED";

            if (property.name == "Array")
                s = s + ":" + property.arrayElementType;

            s = s + " | ";
        } while (property.Next(true));

        Debug.Log(s);
    }

    // Kirajzoljuk a property-t a position által megadott területen belülre
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //PrintProperties(property);

        // Kiszámoljuk az indent értékét
        float indentValue = position.width;
        EditorGUI.indentLevel++;
        indentValue -= EditorGUI.IndentedRect(position).width;
        EditorGUI.indentLevel--;

        // Készítünk egy 1 sor magas Rectangle-t
        actLine = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(actLine, property.isExpanded, label, true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            NextLine();
            SerializedProperty mainArrayProp = property.FindPropertyRelative("list.Array");
            SerializedProperty mainSizeProp = mainArrayProp.FindPropertyRelative("size");
            EditorGUI.PropertyField(actLine, mainSizeProp, true);

            int mainSize = mainSizeProp.intValue;

            GUIContent saveActiveIcon = EditorGUIUtility.IconContent("SaveActive");
            GUIContent savePassiveIcon = EditorGUIUtility.IconContent("SavePassive");
            GUIContent curvekeyframeweightedIcon = EditorGUIUtility.IconContent("curvekeyframeweighted");

            GUIStyle savebuttonStyle = new GUIStyle(GUI.skin.label);
            savebuttonStyle.padding = new RectOffset();

            GUIStyle littleSquareStyle = new GUIStyle(GUI.skin.label);
            littleSquareStyle.padding = new RectOffset(2, 0, 4, 0);

            for (int i = 0; i < mainSize; i++)
            {
                NextLine();

                SerializedProperty mainArrayItem = mainArrayProp.FindPropertyRelative("data[" + i + "]");

                SerializedProperty enabledProp = mainArrayItem.FindPropertyRelative("enabled");
                SerializedProperty commandProp = mainArrayItem.FindPropertyRelative("command");
                SerializedProperty saveToFileProp = mainArrayItem.FindPropertyRelative("saveToFile");
                SerializedProperty continuouslyProp = mainArrayItem.FindPropertyRelative("continuously");
                SerializedProperty nextIndexProp = mainArrayItem.FindPropertyRelative("nextIndex");
                SerializedProperty nextIndexIsValid = mainArrayItem.FindPropertyRelative("nextIndexIsValid");
                SerializedProperty answerDatas = mainArrayItem.FindPropertyRelative("answerDatas");

                Rect r = SetIndentAndWidth(actLine, 0, indentValue);
                answerDatas.isExpanded = EditorGUI.Foldout(r, answerDatas.isExpanded, "");
                r = SetIndentAndWidth(actLine, 0, 30);
                enabledProp.boolValue = EditorGUI.ToggleLeft(r, "", enabledProp.boolValue);
                r = SetIndentAndWidth(actLine, 1, (int)actLine.width - 32);
                commandProp.stringValue = EditorGUI.TextField(r, commandProp.stringValue);

                r = new Rect(actLine.x + actLine.width - 16, actLine.y, 16, actLine.height);
                if (GUI.Button(r, saveToFileProp.boolValue ? saveActiveIcon : savePassiveIcon, savebuttonStyle))
                {
                    saveToFileProp.boolValue = !saveToFileProp.boolValue;
                }

                if (answerDatas.isExpanded)
                {
                    SerializedProperty answerDatasArrayProp = answerDatas.FindPropertyRelative("Array");
                    SerializedProperty sizeProp = answerDatasArrayProp.FindPropertyRelative("size");

                    // Validáljuk a nextIndedx-et (Lehet, hogy az aktuális indexetről leveszik az engedélyezést, ekkor a következő engedélyezett indexre kellene ugornia a nextIndex-nek
                    List<bool> enabledList = new List<bool>();

                    for (int k = 0; k < sizeProp.intValue; k++)
                    {
                        SerializedProperty arrayItem = answerDatasArrayProp.FindPropertyRelative("data[" + k + "]");
                        SerializedProperty answerEnabledProp = arrayItem.FindPropertyRelative("enabled");
                        enabledList.Add(answerEnabledProp.boolValue);
                    }

                    int newIndex;
                    nextIndexIsValid.boolValue = ClassYServerCommunication.ListOfRequestTest.RequestTest.ValidateIndex(enabledList, nextIndexProp.intValue, out newIndex);
                    nextIndexProp.intValue = newIndex;

                    EditorGUI.indentLevel++;

                    NextLine();
                    EditorGUI.PropertyField(actLine, sizeProp);

                    for (int j = 0; j < sizeProp.intValue; j++)
                    {
                        SerializedProperty arrayItem = answerDatasArrayProp.FindPropertyRelative("data[" + j + "]");
                        SerializedProperty answerEnabledProp = arrayItem.FindPropertyRelative("enabled");
                        SerializedProperty answerProp = arrayItem.FindPropertyRelative("answer");

                        NextLine();

                        if (nextIndexIsValid.boolValue)
                        {
                            if (j == nextIndexProp.intValue)
                            {
                                // Nyilat kell rajzolni
                                r = new Rect(actLine.x - 3, actLine.y, EditorGUI.indentLevel * indentValue, actLine.height);
                                continuouslyProp.boolValue = EditorGUI.Foldout(r, continuouslyProp.boolValue, "");
                            }
                            else
                            {
                                // Gombot kell rajzolni rarja egy fekete kockával, ha engedélyezve van az elem
                                if (answerEnabledProp.boolValue)
                                {
                                    r = SetIndentAndWidth(actLine, EditorGUI.indentLevel - 1, 15);
                                    if (GUI.Button(r, curvekeyframeweightedIcon, littleSquareStyle)) //, savebuttonStyle))
                                    {
                                        nextIndexProp.intValue = j;
                                    }
                                }
                            }
                        }

                        r = SetIndentAndWidth(actLine, 0, (EditorGUI.indentLevel + 1) * indentValue);
                        answerEnabledProp.boolValue = EditorGUI.ToggleLeft(r, "", answerEnabledProp.boolValue);

                        EditorGUI.indentLevel++;
                        EditorGUI.ObjectField(actLine, answerProp, GUIContent.none);
                        EditorGUI.indentLevel--;
                    }

                    EditorGUI.indentLevel--;
                }
            }
        }
    }

    void NextLine()
    {
        actLine = new Rect(actLine.x, actLine.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, actLine.width, actLine.height);
    }

    Rect SetIndentAndWidth(Rect baseRect, int indentLevel, float width = float.MaxValue)
    {
        int indentSize = 15;

        return new Rect(baseRect.x + indentLevel * indentSize, baseRect.y, (width < baseRect.width) ? width : baseRect.width - indentLevel * indentSize, baseRect.height);
    }


    // A rendszer megkérdezi, hogy milyen magasságú helyre lenne szükségünk
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        //Debug.Log("GetPropertyHeight : " + EditorGUI.GetPropertyHeight(property));

        // Megszámoljuk, hogy hány sorra van szükség
        int count = 1;
        if (property.isExpanded)
        {
            SerializedProperty mainArrayProp = property.FindPropertyRelative("list.Array");
            SerializedProperty mainSizeProp = mainArrayProp.FindPropertyRelative("size");

            int mainSize = mainSizeProp.intValue;

            count += mainSize + 1;

            for (int i = 0; i < mainSize; i++)
            {
                SerializedProperty mainArrayItem = mainArrayProp.FindPropertyRelative("data[" + i + "]");
                SerializedProperty answerDatas = mainArrayItem.FindPropertyRelative("answerDatas");

                if (answerDatas.isExpanded)
                {
                    SerializedProperty answerDatasArrayProp = answerDatas.FindPropertyRelative("Array");
                    SerializedProperty sizeProp = answerDatasArrayProp.FindPropertyRelative("size");

                    count += sizeProp.intValue + 1;
                }
            }
        }

        //int count = property.CountInProperty();
        //Debug.Log("CountInProperty : " + count);
        //Debug.Log("PixelPerPoint : " + EditorGUIUtility.pixelsPerPoint);
        //Debug.Log("StandardVerticalSpacing : " + EditorGUIUtility.standardVerticalSpacing);

        float needHeight = EditorGUIUtility.singleLineHeight * count + EditorGUIUtility.standardVerticalSpacing * --count;
        //Debug.Log("NeedHeight : " + needHeight);

        return needHeight;
    }
    

    /*
    A Unity hogyan jegyzi meg a foldout állapotát?
    Tömböknél, illetve összetett típusoknál jelenik meg egy kis háromszög amit ki lehet nyitni, illetve bezárni.
    Ezt a Unity valahol megjegyzi, de hol?
    Nem a property adatokkal együtt, mivel ha kitörlöm a Library és az object mappákat, akkor elfelejti az állapotukat,
    viszont a beállított tulajdonágok értékeire emlékszik.

    property.isExpanded
    változóba lehet elmenteni a kinyitás állapotát és innen lehet kiolvasni is.

    */
}

