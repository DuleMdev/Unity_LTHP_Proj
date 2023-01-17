using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/*
Szöveget és dragTarget-et létrehozó szkript
A szkriptnek meg kell adni egy szöveget amit részekre bont.
Létrehozz egy listItems nevű listát, amiben a szövegek és a dragTarget-ek MonoBehaviour objektumát teszi.
A létrehozott objektumoknak implementálni kell az IWidthHeight interfészt, hogy a méretüket le lehessen kérdezni.
*/

public class TextMeshWithDragTarget : MonoBehaviour, IWidthHeight {

    GameObject textPrefab;                      // A kérdés szöveg része
    GameObject dragTargetPrefab;                // Ahová a megfogott elemet helyezni lehet

    [HideInInspector]
    public int subQuestionNumber;               // A szövegben található kérdések száma
    [HideInInspector]
    public List<MonoBehaviour> listItems;       // A szöveg feldarabolva textMeshPrefab és dragTargetPrafab objektumokra

    [HideInInspector]
    public List<DragTarget> listOfDragTarget;   // A DragTarget-eket külön egy listába gyűjtjük

    float fullWidth;                            // A szöveg teljes szélességét tárolja létrehozás után

    void Awake()
    {
        textPrefab = gameObject.SearchChild("Text");
        dragTargetPrefab = gameObject.SearchChild("QuestionMark");
    }

    public void Initialize(TextMeshWithDragTargetData questionData, int questionNumber, Sprite picture = null)
    {
        subQuestionNumber = 0;

        listItems = new List<MonoBehaviour>();
        listOfDragTarget = new List<DragTarget>();

        // Felbontjuk a szöveget részekre és létrehozzuk a részeket
        foreach (string questionPart in questionData.listOfQuestionParts)
        {
            // Ha a találat első karaktere kérdőjel
            if (questionPart.StartsWith("??"))
            {   // Létrehozunk egy kérdőjel objektumot
                GameObject dragTargetClone = Instantiate(dragTargetPrefab);
                dragTargetClone.transform.parent = transform;
                dragTargetClone.transform.localScale = Vector3.one;

                DragTarget dragTarget = dragTargetClone.GetComponent<MonsterDragTarget>();
                dragTarget.Initialize(questionData.listOfAnswerGroups[subQuestionNumber]);
                dragTarget.maxItem = 1;
                dragTarget.questionIndex = questionNumber;
                dragTarget.subQuestionIndex = subQuestionNumber;
                if (picture != null) dragTarget.SetPicture(picture);

                listItems.Add((MonoBehaviour)dragTarget);

                listOfDragTarget.Add(dragTarget);

                subQuestionNumber++;
            }
            else {
                // Létrehozzuk a szöveg objektumot
                GameObject textMesh = Instantiate(textPrefab);
                textMesh.transform.parent = transform;
                textMesh.transform.localScale = Vector3.one;

                TextMeshPrefab textMeshScript = textMesh.GetComponent<TextMeshPrefab>();
                textMeshScript.Initialize(questionPart);

                listItems.Add((MonoBehaviour)textMesh.GetComponent<TextMeshPrefab>());
            }
        }

        // A létrehozott részeket pozícionáljuk
        fullWidth = Common.PositionToBase(gameObject, listItems, 0.05f);
    }

    public float GetHeight()
    {
        throw new NotImplementedException();
    }

    public float GetWidth()
    {
        return fullWidth;
    }

    public void SetPictures(Sprite picture) {
        foreach (DragTarget dragTarget in listOfDragTarget)
        {
            dragTarget.SetPicture(picture);
        }
    }
}
