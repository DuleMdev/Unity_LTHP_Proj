using UnityEngine;
using System.Collections;

using System.Collections.Generic;
using SimpleJSON;

public class ABC_Table : MonoBehaviour
{
    public TextAsset keySets;

    [Tooltip("A betűket automatikusan osztja el a négy sor között a rendelkezésre álló hely függvényében")]
    public bool autoLayout;

    GameObject keyButtonPrefab;

    Transform[] rowStart;
    Transform[] rowEnd;

    GameObject move;

    SpriteRenderer tablePicture;

    List<KeyButton> listOfKeyButtons;

    [HideInInspector]
    public string allLetters;  // Az összes betű amit ki lehet választani

    const float ANIM_SPEED = 1;

    float screenEdge;   // Képernyő széle

	// Use this for initialization
	void Awake () {

        keyButtonPrefab = Common.SearchGameObject(gameObject.transform.parent.gameObject, "KeyButton").gameObject;

        move = Common.SearchGameObject(gameObject, "move").gameObject;
        tablePicture = Common.SearchGameObject(gameObject, "TablePicture").GetComponent<SpriteRenderer>();

        // Összeszedjük a sorok elejét és a végét
        rowStart = new Transform[4];
        rowEnd = new Transform[4];

        Transform[] transforms = GetComponentsInChildren<Transform>(true);

        foreach (Transform item in transforms)
        {
            if (item.name.StartsWith("RowStart")) 
                rowStart[int.Parse(item.name.Substring(8)) - 1] = item;

            if (item.name.StartsWith("RowEnd"))
                rowEnd[int.Parse(item.name.Substring(6)) - 1] = item;
        }

        // Meghatározzuk, hogy hol van a képernyő széle
        screenEdge = Camera.main.aspect;
    }

    /// <summary>
    /// Létrehozzuk a beállított nyelvnek megfelelő betűgombokat.
    /// A button Click paraméterben megadhatunk egy metódust, amit majd a betűk lenyomásakor meg fog hívni.
    /// </summary>
    /// <param name="buttonClick">Betűk lenyomásakor a meghívandó metódus.</param>
    public void Init(KeyButton.ButtonClick buttonClick, string language) {
        // Korábbi betű gombokat töröljük
        if (listOfKeyButtons != null)
            foreach (KeyButton keyButton in listOfKeyButtons)
                Destroy(keyButton.gameObject);

        // Létrehozzuk a betű gombokat
        JSONNode node = JSON.Parse(keySets.text);
        if (node.ContainsKey(language))
        {
            node = GetAutoLayout(node[language]); // Common.languageController.actLanguageID];
        }
        else
        {
            JSONArray jsonArray = new JSONArray();
            jsonArray.Add(language);
            autoLayout = true;
            node = GetAutoLayout(jsonArray);
        }

        allLetters = "";
        listOfKeyButtons = new List<KeyButton>();
        for (int i = 0; i < node.Count; i++)
        {
            string rowLetters = node[i];
            allLetters += rowLetters;

            float distanceBetweenLetters = 0;
            if (rowLetters.Length > 1)
                distanceBetweenLetters = (rowEnd[i].position.x - rowStart[i].position.x) / (rowLetters.Length - 1);

            for (int j = 0; j < rowLetters.Length; j++)
            {
                KeyButton newKeyButton = Instantiate(keyButtonPrefab, tablePicture.transform, true).GetComponent<KeyButton>();
                newKeyButton.Init(rowLetters[j].ToString());
                newKeyButton.transform.position =  new Vector3(rowStart[i].position.x + j * distanceBetweenLetters, rowStart[i].position.y) ;
                newKeyButton.buttonClick = buttonClick;

                listOfKeyButtons.Add(newKeyButton);
            }
        }

        // A betű gombokat tartalmazó táblát kipozícionáljuk a képernyőből
        move.transform.position = new Vector3(Camera.main.aspect * 2, move.transform.position.y);
    }

    /// <summary>
    /// A rendelkezésre álló helyre automatikusan elosztja a betűket
    /// </summary>
    /// <returns></returns>
    JSONNode GetAutoLayout(JSONNode node)
    {
        if (autoLayout)
        {
            // összegyűjtjük az összes betüt
            string allLetters = "";
            for (int i = 0; i < node.Count; i++)
                allLetters += node[i];

            // Elosztjuk a sorok között
            // Meghatározzuk, hogy melyik sorba hány betű legyen
            int[] rowCount = new int[rowStart.Length];

            for (int i = 0; i < allLetters.Length; i++)
            {
                int theBestIndex = 0;
                float theBestValue = float.MinValue;

                // Megkeressük azt a sort, ahol legritkábban találhatóak a betük
                for (int j = 0; j < rowCount.Length; j++)
                {
                    float actValue = (rowEnd[j].position.x - rowStart[j].position.x) / rowCount[j] + 1;
                    if (actValue > theBestValue)
                    {
                        theBestValue = actValue;
                        theBestIndex = j;
                    }
                }

                // A legritkább sorba eggyel több betüt teszünk
                rowCount[theBestIndex] += 1;
            }

            // Létrehozzuk az új node-ot
            int letterStart = 0;
            for (int i = 0; i < rowCount.Length; i++)
            {
                node[i] = allLetters.Substring(letterStart, rowCount[i]);
                letterStart += rowCount[i];
            }
        }

        return node;
    }

    public KeyButton GetKeyButton(string key) {
        foreach (KeyButton keyButton in listOfKeyButtons)
            if (keyButton.key == key)
                return keyButton;

        return null;
    }

    // Megjelenítjük a betűket tartalmazó táblát
    public void Show(float animTime = ANIM_SPEED) {
        if (animTime > 0)
            iTween.MoveTo(move, iTween.Hash("islocal", true, "position", Vector3.zero, "time", animTime, "easeType", iTween.EaseType.easeOutCirc));
        else
            move.transform.localPosition = Vector3.zero;
    }

    public float Hide() {
        iTween.MoveTo(move, iTween.Hash("position", new Vector3(Camera.main.aspect * 2, move.transform.position.y), "time", ANIM_SPEED, "easeType", iTween.EaseType.easeInCirc));

        return ANIM_SPEED;
    }

    public void SetPictures(Sprite abcTable) {
        tablePicture.sprite = abcTable;
    }
}
