using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

/*
Ez az objektum a fordításért felel.

Az alapértelmezett nyelv az első ami meg van adva az inspector ablakban



Készült egy SetLanguageText komponens amit ha egy text komponenst tartalmazó gameObjectre teszünk, akkor automatikusan
kifogja tölteni a text komponenst a fordítással.
Ezért ebben a SetLanguageText komponensben meg kell adni a fordítandó kulcsszót.
Lehetőség van több kulcsszót is lefordítani a következő képpen.

egyikKulcsszó|másikKulcsszó|*paraméter1|harmadikKulcsszó

egyikKulcsszó|-<b>|másikKulcsszó|-</b>|harmadikKulcsszó


Tehát a program feldarabolja a szövegekeet | jel mentén, majd minden darabot megpróbál lefordítani.
* Ha csillaggal kezdődik a kulcsszó, akkor azt paraméternek tekinti.
  Ezeket vagy a dataProvidertől próbálja megszereezni, vagy a szótárba nézi meg.
- Ha kötöjellel kezdődik egy kulcsszó, akkor azt nem fordítja le



*/

public class LanguageController : MonoBehaviour {

    [System.Serializable]
    public class LanguageData {

        [Tooltip("A nyelv azonosítója")]
        public string languageID;   // A nyelv azonosítója pl. "English", "Hungarian", "Romanian", "Slovak"
        [Tooltip("A fordításokat tartalmazó JSON fájl")]
        public TextAsset data;      // 
        [Tooltip("A képeket tartalmazó JSON fájl")]
        public TextAsset dataPictures;      // 
    }

    string dirName = "Languages";

    public string defaultLanguage;

    JSONNode jsonLanguagesData;
    public List<LanguageData> listOfLanguagesData;
    JSONNode actLanguageData;   // A beállított nyelv adatait tartalmazza, a default az első
    JSONNode actLanguagePictureData;

    //JSONNode newLanguagesDataFromServer;

    [HideInInspector]
    public string actLanguageID;

    void Awake() {
        Debug.Log("LanguageController.Awake");

        Common.languageController = this;

        // Ha még a ConfigurationController nem hívta meg ennek a scriptnek a Load metódusát, akkor létrehozunk egy üres osztályt a nyelvi adatoknak
        if (jsonLanguagesData == null)
            jsonLanguagesData = new JSONClass();

        LanguageDetermination(false);
    }

    /// <summary>
    /// Meghatározza a használandó nyelvet
    /// </summary>
    public void LanguageDetermination(bool refresh = true)
    {
        bool languageDatasIsLoaded = false;

        // Ha a felhasználó meghatározta, hogy milyen legyen az app nyelve
        if (!string.IsNullOrEmpty(Common.configurationController.applicationLangName))
            languageDatasIsLoaded = SetLanguage(Common.configurationController.applicationLangName);

        // Ha konkrétan meg van határozva a használandó nyelv
        if (!languageDatasIsLoaded && !string.IsNullOrWhiteSpace(Common.configurationController.setupData.defaultLanguage))
            languageDatasIsLoaded = SetLanguage(Common.configurationController.setupData.defaultLanguage);
        //if (!string.IsNullOrWhiteSpace(defaultLanguage))
        //    languageDatasIsLoaded = SetLanguage(defaultLanguage);

        // A tablet nyelvét próbáljuk meg használni
        if (!languageDatasIsLoaded)
            languageDatasIsLoaded = SetLanguage(Application.systemLanguage.ToString());

        // A listában szereplő első nyelvet használjuk
        if (!languageDatasIsLoaded && listOfLanguagesData.Count > 0)
            languageDatasIsLoaded = LoadLanguageFromInspector(listOfLanguagesData[0].languageID);

        // nincs fordítás
        if (!languageDatasIsLoaded)
        {
            actLanguageData = JSON.Parse("{}");
            actLanguagePictureData = JSON.Parse("{}");
            actLanguageID = "";
        }

        if (refresh)
            Refresh();
    }

    /// <summary>
    /// Frissíti a szövegeket az aktuális képernyőn.
    /// </summary>
    public void Refresh()
    {
        Debug.Log("LanguageController . Refresh");

        List<SetLanguageText> listOfSetLanguageText = new List<SetLanguageText>(Common.screenController.GetComponentsInChildren<SetLanguageText>());

        if (Common.screenController.actScreen && !Common.IsDescendant(Common.screenController.transform, Common.screenController.actScreen.transform))
        {
            Debug.Log("actScreen : " + Common.screenController.actScreen.name);

            SetLanguageText[] actScreensListOfSetLanguageText = Common.screenController.actScreen.GetComponentsInChildren<SetLanguageText>();

            foreach (SetLanguageText item in actScreensListOfSetLanguageText)
            {
                if (!listOfSetLanguageText.Contains(item))
                    listOfSetLanguageText.Add(item);
            } 
        }

        Debug.Log("SetLanguageText component count : " + listOfSetLanguageText.Count);

        // Mindegyiknek meghívjuk a SetText metóduát, ami újra beállítja a szöveget a kiválasztott nyelvnek megfelelően
        for (int i = 0; i < listOfSetLanguageText.Count; i++)
        {
            listOfSetLanguageText[i].Refresh();
        }
    }

    bool SetLanguage(string languageName)
    {
        // Betöltjük az inspectorból ha ott megtalálható
        bool loaded = LoadLanguageFromInspector(languageName);

        // Megkiséreljük betölteni a háttértárról a megadott nevű nyelvet
        string fileName = System.IO.Path.Combine(Common.GetDocumentsDir(), dirName, languageName + ".txt");

        try
        {
            JSONNode loadedLanguageData = JSON.Parse(System.IO.File.ReadAllText(fileName));
            if (loadedLanguageData != null)
            {
                actLanguageData = loadedLanguageData;
                loaded = true;
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e);

            // Hiba csak akkor lehetséges, ha nem létezik a fájl vagy hibás a benne található json
            // Ha hiba volt, akkor kitöröljük a nyelv adatait
            JSONNode json = GetLanguageDatas(languageName, jsonLanguagesData);
            if (json != null)
            {
                jsonLanguagesData.Remove(json);
                Common.configurationController.Save();
            }
        }

        return loaded;
    }

    IEnumerator Start()
    {
        Debug.Log("LanguageController.Start");
        // Feltehetőleg már be vannak töltve a config adatok
        LoadLanguageData(Common.configurationController.loadNode != null ? Common.configurationController.loadNode[C.JSONKeys.appLanguages] : null) ;

        bool success = false;
        JSONNode response = null;
        bool ready = false;

        // Megkérdezzük a szervertől a nyelvi fájlok állapotát
        ClassYServerCommunication.instance.GetSupportedLanguages(
            false,
            (bool success2, JSONNode response2) =>
            {
                success = success2;
                response = response2;
                ready = true;
            }
            );

        while (!ready) yield return null; // Várunk amíg az előző lekérdezés válasza meg nem érkezik

        // Ha sikeresen letöltöttük a támogatott nyelvek listáját, akkor megkiséreljük feldolgozni a választ
        if (success)
        {
            bool error = false; // Volt-e valamilyen hiba a letöltések közben
            bool modified = false;  // Volt-e sikeres letöltés


            if (Common.configurationController.isServer2020)
            {
                #region Ez az új cucc ami egy lekérdezésbe tölti le az összes nyelv adatát
                // Ha valamelyik nyelvi fájl módosult, akkor letöltjük a nyelveket (mindet)
                modified = response[C.JSONKeys.answer].ToString() != jsonLanguagesData.ToString();
                if (modified)
                {
                    ready = false;

                    ClassYServerCommunication.instance.getAllSupportedLanguagesData(
                        false,
                        (bool success2, JSONNode response2) =>
                        {
                            // Ha sikeres volt a letöltés, akkor elmentjük a háttértárra
                            if (success2)
                            {
                                Debug.Log("A nyelvi fájlok letöltése sikeres");

                                for (int i = 0; i < response[C.JSONKeys.answer].Count; i++)
                                {
                                    try
                                    {
                                        string dirPath = System.IO.Path.Combine(Common.GetDocumentsDir(), dirName);
                                        System.IO.Directory.CreateDirectory(dirPath);

                                        string fileName = System.IO.Path.Combine(dirPath, response[C.JSONKeys.answer][i][C.JSONKeys.name] + ".txt");

                                        Debug.Log("A nyelvi fájl mentése : " + fileName);

                                        System.IO.File.WriteAllText(fileName, response2[response[C.JSONKeys.answer][i][C.JSONKeys.code]].ToString(" "));
                                    }
                                    catch (System.Exception e)
                                    {
                                        Debug.Log(e.ToString());
                                        error = true;
                                    }
                                }
                            }
                            else
                            {
                                Debug.Log("A nyelvi fájlok letöltése sikertelen");
                            }

                            ready = true;
                        }
                        );
                }

                while (!ready) yield return null;
                #endregion
            }
            else
            {
                #region Ez a régi cucc ami nyelvenként tölti le a nyelvi adatokat
                // Összehasonlítjuk a letöltött adatokat a mentett adatokkal
                for (int i = 0; i < response[C.JSONKeys.answer].Count; i++)
                {
                    JSONNode json = GetLanguageDatas(response[C.JSONKeys.answer][i][C.JSONKeys.name], jsonLanguagesData);
                
                    // Ha valamelyik nyelvi fájl módosult vagy nem létezik, akkor letöltjük az adatokat
                    if (json == null || json[C.JSONKeys.translate_change].Value != response[C.JSONKeys.answer][i][C.JSONKeys.translate_change].Value)
                    {
                        ready = false;
                
                        Debug.Log("Megkiséreljük letölteni a " + response[C.JSONKeys.answer][i][C.JSONKeys.name] + " nyelvi fájlt");
                        
                        ClassYServerCommunication.instance.GetLanguageData(
                            response[C.JSONKeys.answer][i][C.JSONKeys.code],
                            false,
                            (bool success2, JSONNode response2) =>
                            {
                                // Ha sikeres volt a letöltés, akkor elmentjük a háttértárra
                                if (success2)
                                {
                                    Debug.Log("A " + response[C.JSONKeys.answer][i][C.JSONKeys.name] + " nyelvi fájl letöltése sikeres");
                
                                    try
                                    {
                                        string dirPath = System.IO.Path.Combine(Common.GetDocumentsDir(), dirName);
                                        System.IO.Directory.CreateDirectory(dirPath);
                
                                        string fileName = System.IO.Path.Combine(dirPath, response[C.JSONKeys.answer][i][C.JSONKeys.name] + ".txt");
                
                                        Debug.Log("A nyelvi fájl mentése : " + fileName);
                
                                        System.IO.File.WriteAllText(fileName, response2.ToString(" "));
                
                                        modified = true; // Volt sikeres letöltés
                                    }
                                    catch (System.Exception e)
                                    {
                                        Debug.Log(e.ToString());        
                                        error = true;
                                    }
                                }
                                else
                                {
                                    Debug.Log("A " + response[C.JSONKeys.answer][i][C.JSONKeys.name] + " nyelvi fájl letöltése sikertelen");
                                }
                
                                ready = true;
                            }
                            );
                
                        // Várunk amíg a nyelvi adatok nem töltődnek le az internetről és nem dolgozzuk fel
                        while (!ready) yield return null;
                    }
                }
                #endregion
            }

            #region Ez a rész közös
            // Ha nem volt hiba a letöltések közben és módosult valami
            if (modified)
            {
                if (!error)
                {
                    // Frissítjük a nyelvi adatokat
                    jsonLanguagesData = response[C.JSONKeys.answer];

                    // Elmentjük az új nyelvi adatokat
                    Common.configurationController.Save();
                }

                LanguageDetermination(); // Újra meghívjuk a nyelv meghatározását, hátha éppen a használatos nyelvi fájl módosult
            }
            #endregion
        }

        yield return null;
    }

    JSONNode GetLanguageDatas(string languageName, JSONNode json)
    {
        for (int i = 0; i < json.Count; i++)
        {
            if (json[i][C.JSONKeys.name].Value == languageName)
                return json[i];
        }

        return null;
    }

    // Vissza adja a rendelkezésre álló nyelveket
    public List<string> GetLanguages()
    {
        List<string> listOfLanguagesID = new List<string>();

        foreach (LanguageData languageData in listOfLanguagesData)
            listOfLanguagesID.Add(languageData.languageID);

        for (int i = 0; i < jsonLanguagesData.Count; i++)
        {
            string langName = jsonLanguagesData[i][C.JSONKeys.name].Value;

            if (!listOfLanguagesID.Contains(langName))
                listOfLanguagesID.Add(langName);
        }

        return listOfLanguagesID;
    }

    // A cél nyelvet beállítja a megadottra, ha nincs olyan nyelvi azonosító, akkor marad a korábbi
    public bool LoadLanguageFromInspector(string languageID)
    {
        try
        {
            foreach (LanguageData languageData in listOfLanguagesData)
            {
                if (languageData.languageID == languageID)
                {
                    if (languageData.data)
                        actLanguageData = JSON.Parse(languageData.data.text);

                    if (languageData.dataPictures)
                        actLanguagePictureData = JSON.Parse(languageData.dataPictures.text);

                    actLanguageID = languageID;
                    return true; // Ha megvan a keresett nyelv, akkor kilépünk
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }

        return false;
    }


    public string PreTranslate(string text, char translateMaker = '#', Dictionary<string, string> dicParams = null, Common.CallBack_In_String_Out_String dataProvider = null)
    {
        // feldaraboljuk a fordítandó szöveget
        string[] textArray = text.Split('|');

        string result = "";
        // darabonként lefordítjuk
        for (int i = 0; i < textArray.Length; i++)
        {
            if (!string.IsNullOrEmpty(textArray[i]))
            {
                switch (textArray[i][0])
                {
                    case '*': // paraméter
                        result += GetParams(textArray[i].Substring(1), dicParams, dataProvider, true);
                        break;

                    case '-': // Békén kell hagyni
                        result += textArray[i].Substring(1);
                        break;

                    default: // Fordítandó szöveg
                        string translatedText = actLanguageData[textArray[i]];
                        result += string.IsNullOrEmpty(translatedText) ? "$" + textArray[i] : translatedText;

                        break;
                }
            }

            /*
            if (textArray[i][0] != '*')
            {
                string translatedText = actLanguageData[textArray[i]];
                result += string.IsNullOrEmpty(translatedText) ? "$" + textArray[i] : translatedText;
            }
            else
            {
                result += GetParams(textArray[i].Substring(1), dicParams, dataProvider);
            }
            */
        }

        // Ha paramétereket kell a szövegbe illeszteni, akkor azt megtesszük
        // Szögletes zárójelek között vannak a paraméterek pl. [value]
        if (text == "levelInfo")
            Debug.Log("Break");

        int indexOpen = 0;
        do
        {
            indexOpen = result.IndexOf('[', indexOpen);

            if (indexOpen > -1)
            {
                int indexClose = result.IndexOf(']', indexOpen);

                if (indexClose > -1)
                {
                    string paramsToken = result.Substring(indexOpen + 1, indexClose - indexOpen - 1);

                    string paramValue = GetParams(paramsToken, dicParams, dataProvider, false);

                    if (paramValue != null)
                    {
                        result = result.SubstringSafe(0, indexOpen) + paramValue + result.SubstringSafe(indexClose + 1);
                    }
                    else
                        indexOpen++;
                }
                else break;
            }
        } while (indexOpen != -1);


        /*
        if (dicParams != null)
        {
            foreach (string paramName in dicParams.Keys)
            {
                //result = result.Replace("[" + paramName + "]", dicParams[paramName][0] == translateMaker ? Translate(dicParams[paramName].Substring(1)) : dicParams[paramName]);
                result = result.Replace("[" + paramName + "]", dicParams[paramName][0] == translateMaker ? Translate(paramName.Substring(1)) : dicParams[paramName]);
            }
        }
        */
        return result;
    }

    public string GetParams(string paramName, Dictionary<string, string> dicParams, Common.CallBack_In_String_Out_String dataProvider, bool mustBe)
    {
        // Ha a megadott szótárban meg van a paraméter akkor onnan olvassuk ki az értékét
        if (dicParams != null && dicParams.ContainsKey(paramName))
            return dicParams[paramName];

        // Ha nincs a szótárban és van dataProvider, akkor megpróbáljuk onnan kiolvasni
        if (dataProvider != null)
            return dataProvider(paramName);

        // Ha egyik sincs, akkor egy hiba stringet ad vissza
        return mustBe ? "?missing param<" + paramName + ">?" : null;
    }

    public string Translate(string text, char translateMaker = '#', Dictionary<string, string> dicParams = null, Common.CallBack_In_String_Out_String dataProvider = null)
    {
        return PreTranslate(text, translateMaker, dicParams, dataProvider);

        /*
        string result = actLanguageData[text];

        // Ha nem találtuk meg a fordítandó szöveget, akkor az eredeti szöveget adja vissza két # között, ami figyelmeztett, hogy nem jó valami
        if (string.IsNullOrEmpty(result)) {
            //result = "#" + text + "#";
            result = text;
        }

        // Ha paramétereket kell a szövegbe illeszteni, akkor azt megtesszük
        if (dicParams != null)
        {
            foreach (string paramName in dicParams.Keys)
            {
                //result = result.Replace("[" + paramName + "]", dicParams[paramName][0] == translateMaker ? Translate(dicParams[paramName].Substring(1)) : dicParams[paramName]);
                result = result.Replace("[" + paramName + "]", dicParams[paramName][0] == translateMaker ? Translate(paramName) : dicParams[paramName]);
            }
        }

        //Debug.Log("LanguageController.Translate : " + text + " -> " + result);

        // Ha nem tudta lefordítani, akkor ír elé egy $ jelet, jelezve ezt a tényt
        return (result == text) ? "$" + text : result;
        */
    }

    public string Translate(string text, char translateMaker = '#', params string[] keyValuePairs)
    {
        return Translate(text, translateMaker, stringArrayToDictionary(keyValuePairs));
    }

    public Dictionary<string, string> stringArrayToDictionary(string[] keyValuePairs)
    {
        Dictionary<string, string> dicParams = null;
        if (keyValuePairs != null && keyValuePairs.Length > 1)
        {
            dicParams = new Dictionary<string, string>();

            for (int i = 0; i < keyValuePairs.Length - 1; i = i + 2)
            {
                if (!dicParams.ContainsKey(keyValuePairs[i]))
                    dicParams.Add(keyValuePairs[i], keyValuePairs[i + 1]);
                else
                    dicParams[keyValuePairs[i]] = keyValuePairs[i + 1];
            }
        }

        return dicParams;
    }

    public Sprite GetSprite(string text)
    {
        try
        {
            return Common.MakeSpriteFromByteArray(System.Convert.FromBase64String(actLanguagePictureData[text].Value));
        }
        catch (System.Exception)
        {

        }

        return null;
    }

    public void LoadLanguageData(JSONNode json)
    {
        if (json != null)
            jsonLanguagesData = json;

        LanguageDetermination();
    }

    public JSONNode GetLanguageDataToSave()
    {
        return jsonLanguagesData;
    }

}
