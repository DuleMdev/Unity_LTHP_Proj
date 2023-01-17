using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using SimpleJSON;

public class LessonPlanData {


    public int id;                  // Az óraterv azonosítója
    public string name;             // A óraterv neve
    public string labels;           // Az óraterv cimkéi
    public int subject;             // Az óra tárgya
    public int language;            // Az óra nyelve
    public string lessonSynchronizeTime; // Mikor volt a lecketerv szinkronizálva

    public float progressPercent;   // Hogy áll az óraterv képeinek feldolgozása
    public bool finish;             // Igaz értéke van ha az objektum befejezte a feldolgozást
    public bool error;              // A feldolgozás során hiba történt, nem lehet használni a lecke tervet
    public string errorMessage;     // Ha hiba történt, akkor a hiba üzenet

    public List<LessonMosaicData> lessonMosaicsList = new List<LessonMosaicData>(); // Az óraterben található óra mozaikok

    //string jsonText;

    Dictionary<string, Sprite> pictures = new Dictionary<string, Sprite>();

    public void LoadDataFromString(string jsonText, bool full = true) {
        //this.jsonText = jsonText;

        //Thread thread = new Thread(InitDatas);

        //thread.Start();

        error = false;

        try
        {
            LoadDataFromJSONNode(JSON.Parse(jsonText), true);
        }
        catch (System.Exception e)
        {
            errorMessage = e.Message;
            error = true;
        }
    }

    /*
    public LessonPlanData(JSONNode node, bool full = true) {

        InitDatas(node, full);
    }*/

    public void LoadDataFromJSONNode(JSONNode jsonData, bool full = true) {
        error = false;

        if (jsonData != null)
        {
            finish = false;
            progressPercent = 0;

            //JSONNode node = JSON.Parse(jsonText);

            id = jsonData[C.JSONKeys.lessonid].AsInt;
            name = jsonData[C.JSONKeys.lessonName].Value;
            labels = jsonData[C.JSONKeys.lessonLabels].Value;
            subject = jsonData[C.JSONKeys.lessonSubject].AsInt;
            language = jsonData[C.JSONKeys.lessonLanguage].AsInt;

            lessonSynchronizeTime = jsonData[C.JSONKeys.lessonSynchronizeTime].Value;

            if (full)
            {
                lessonMosaicsList = new List<LessonMosaicData>();

                //int lessonMosaicIndex = 0;
                for (int i = 0; i < jsonData["lessonMosaics"].Count; i++)
                {
                    LessonMosaicData mosaicData = new LessonMosaicData(jsonData["lessonMosaics"][i]);

                    if (mosaicData.listOfGames.Count != 0)
                    {
                        // Beállítjuk a feldolgozott játékoknak a mozaik és a játék indexét
                        for (int j = 0; j < mosaicData.listOfGames.Count; j++)
                        {
                            mosaicData.listOfGames[j].lessonMosaicIndex = lessonMosaicsList.Count; // lessonMosaicIndex;
                            mosaicData.listOfGames[j].gameIndex = j;
                        }

                        lessonMosaicsList.Add(mosaicData);
                    }
                }

                // Ha egy óramozaikot sem sikerült létrehozni, akkor hibát jelzünk
                if (lessonMosaicsList.Count == 0)
                {
                    errorMessage = "Egyetlen óramozaikot sem sikerült létrehozni.";
                    error = true;
                }

                /*
                //Debug.Log(node["pictures"].Count);
                int allPictures = jsonData["pictures"].Count;
                int picturesCount = 0;

                foreach (string item in jsonData["pictures"].Keys)
                {
                    try
                    {
                        Texture2D texture = new Texture2D(2, 2);

                        texture.LoadImage(System.Convert.FromBase64String(jsonData["pictures"][item].Value));
                        pictures.Add(item, Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f)));

                        // Kiszámoljuk hány százaléknál tart a program a képek feldolgozásában
                        picturesCount++;
                        progressPercent = 1.0f / allPictures * picturesCount;
                    }
                    catch (System.Exception e)
                    {
                        // Debug.Log("json pictures error : " + e.Message);
                        errorMessage = "Picture error : " + e.Message;
                        error = true;
                        throw;
                    }

                    //yield return null;
                }
                */

                //yield return null;
            }

            finish = true;
        }
    }

    public Sprite GetPicture(string pictureName) {
        return (pictures.ContainsKey(pictureName)) ? pictures[pictureName] : null;
    }

    /// <summary>
    /// json-ba menti az osztály tartalmát.
    /// </summary>
    /// <returns>Egy json, ami tartalmazza az osztály adatait.</returns>
    public JSONNode SaveDataToJSON()
    {
        JSONClass jsonData = new JSONClass();

        jsonData[C.JSONKeys.lessonid].AsInt = id;
        jsonData[C.JSONKeys.lessonName] = name ?? "";
        jsonData[C.JSONKeys.lessonLabels] = labels ?? "";
        jsonData[C.JSONKeys.lessonSubject].AsInt = subject;
        jsonData[C.JSONKeys.lessonLanguage].AsInt = language;

        jsonData[C.JSONKeys.lessonSynchronizeTime].Value = lessonSynchronizeTime;

        return jsonData;
    }
}
