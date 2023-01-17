using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/*
{
    "LastDay" : 
    "EmailGroupPictureFiles" : [
        {
            "fileName" : "BASE64CODEDFILENAME",
            "unusedDays" : 3
        },
        {
            "fileName" : "BASE64CODEDFILENAME",
            "unusedDays" : 3
        },
        {
            "fileName" : "BASE64CODEDFILENAME",
            "unusedDays" : 3
        }
    ]
}
*/

public class EmailGroupPictureController : MonoBehaviour
{
    public static EmailGroupPictureController instance;

    [Tooltip("Hány másodpercenkéntt mentse a kép használati adatokat")]
    public int seconds;
    [Tooltip("Ha egy képre nem volt szükség a megadott különböző napig, akkor törölve lesz.")]
    public int days;

    string fileName = "DownloadedPictures.json";
    string dirName = "DownloadedPictures";

    //string downloadDir;

    bool needSave;

    string lastDay;
    JSONNode jsonDatas;

    float saveCounter;

    Dictionary<string, int> datas = new Dictionary<string, int>();

    HashSet<string> fileDownloads = new HashSet<string>(); // Azok a fájlok, amiket éppen most töltöttünk le.

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;

        try
        {
            string filePath = Path.Combine(Common.GetDocumentsDir(), fileName);

            // betöltjük a file tartalmát
            jsonDatas = JSON.Parse(File.ReadAllText(filePath));

            // Megnézzük, hogy léteznek-e a szükséges kulcs nevek, mert ha nem, akkor ez a json nem olyan amilyennek lennie kellene
            if (!jsonDatas.ContainsKey(C.JSONKeys.EmailGroupPictureFiles) || !jsonDatas.ContainsKey(C.JSONKeys.LastDay))
                throw new Exception("Nem létező kulcs nevek : " + C.JSONKeys.EmailGroupPictureFiles + " vagy " + C.JSONKeys.LastDay);

            // Beolvassuk, hogy mikor használtuk utoljára az appot
            lastDay = jsonDatas[C.JSONKeys.LastDay];

            for (int i = 0; i < jsonDatas[C.JSONKeys.EmailGroupPictureFiles].Count; i++)
            {
                JSONNode item = jsonDatas[C.JSONKeys.EmailGroupPictureFiles][i] ;
                // Megnézzük, hogy léteznek-e a szükséges kulcs nevek, mert csak akkor rögzítjük
                if (item.ContainsKey(C.JSONKeys.fileName) && item.ContainsKey(C.JSONKeys.unusedDays))
                {
                    string fileName = item[C.JSONKeys.fileName];
                    int unusedDays = item[C.JSONKeys.unusedDays].AsInt;

                    // Ha még nemm tartalmazza a szótár a filenevet, akkor rögzítjük
                    if (!datas.ContainsKey(fileName))
                        datas.Add(fileName, unusedDays);
                }
            }

            // Beolvassuk a directoryban található képek neveit
            HashSet<string> fileNames = new HashSet<string>();
            string[] fileEntries = Directory.GetFiles(Path.Combine(Common.GetDocumentsDir(), dirName));
            foreach (string fileEntry in fileEntries)
                fileNames.Add(Path.GetFileName(fileEntry));

            // Ha valamelyik képfile nincs a szótárban azt a file-t töröljük.
            foreach (var fileName in fileNames)
                if (!datas.ContainsKey(fileName))
                    FileDelete(fileName);

            // Ha a szótárban van kulcs amihez nincs fájl, akkor azt a kulcsot töröljük
            List<string> keyList = new List<string>(datas.Keys);
            foreach (string key in keyList)
            {
                if (!fileNames.Contains(key))
                {
                    datas.Remove(key);
                    needSave = true;
                }
            }

            // Meghatározzuk a mai napot
            string today = ToDay();

            // Ha az utolsó indítási dátum nem egyezik a jelenlegivel, akkor minden file nap számlálóját megnöveljük eggyel
            if (today != lastDay)
            {
                List<string> keys = new List<string>(datas.Keys);

                for (int i = 0; i < keys.Count; i++)
                {
                    datas[keys[i]] = datas[keys[i]] + 1;

                    // Ha valamelyik file-t már a megadott alkalomnál régebben használtuk, akkor törölve lesz
                    if (datas[keys[i]] > days)
                    {
                        FileDelete(keys[i]);
                        datas.Remove(keys[i]);
                    }

                    needSave = true;
                }

                //foreach (var kvp in datas)
                //{
                //    datas[kvp.Key] = datas[kvp.Key] + 1;
                //
                //    // Ha valamelyik file-t már a megadott alkalomnál régebben használtuk, akkor törölve lesz
                //    if (datas[kvp.Key] > days)
                //    {
                //        FileDelete(kvp.Key);
                //        datas.Remove(kvp.Key);
                //    }
                //
                //    needSave = true;
                //}
                lastDay = today;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(e.ToString());

            lastDay = DateTime.Now.ToString("yyyy MM dd");
            datas = new Dictionary<string, int>();
            needSave = true;
        }
    }

    void FileDelete(string fileName)
    {
        File.Delete(Path.Combine(Path.Combine(Common.GetDocumentsDir(), dirName), fileName));
    }

    string ToDay()
    {
        return DateTime.Now.ToString("yyyy MM dd");
    }

    public void SaveIfNeed()
    {
        Debug.Log("Kép adatok mentésének kezdeményezése");
        if (needSave)
            Save();
        else
            Debug.Log("A kép adataiban nem történt változás");
    }

    public void Save()
    {
        string filePath = Path.Combine(Common.GetDocumentsDir(), fileName);

        JSONClass saveJson = new JSONClass();

        saveJson.Add(C.JSONKeys.LastDay, lastDay);

        foreach (var kvp in datas)
        {
            JSONClass jsonClass = new JSONClass();
            jsonClass[C.JSONKeys.fileName] = kvp.Key;
            jsonClass[C.JSONKeys.unusedDays].AsInt = kvp.Value;

            saveJson[C.JSONKeys.EmailGroupPictureFiles].Add(jsonClass);
        }

        File.WriteAllText(filePath, saveJson.ToString(" "));

        needSave = false;

        Debug.Log("Kép adatok elmentve");
    }

    public void GetPictureFromUploadsDir(string pictureName, Common.CallBack_In_Sprite callBack)
    {
        StartCoroutine(GetPictureCoroutine("uploads/", pictureName, callBack));
    }

    public void GetPictureFromMarketBadgesDir(string pictureName, Common.CallBack_In_Sprite callBack)
    {
        StartCoroutine(GetPictureCoroutine("marketBadges/", pictureName, callBack));
    }

    public void GetPictureFromMarketWebshopItemsDir(string pictureName, Common.CallBack_In_Sprite callBack)
    {
        StartCoroutine(GetPictureCoroutine("marketWebshopItems/", pictureName, callBack));
    }

    IEnumerator GetPictureCoroutine(string downloadDir, string pictureName, Common.CallBack_In_Sprite callBack)
    {
        //string downloadDir = this.downloadDir;

        // Ha a fájlt már töltjük a szerverről, akkor nem kezdjük el újra inkább megvárjuk, hogy mi történik
        if (fileDownloads.Contains(pictureName))
        {
            Debug.Log("A képet már megpróbálták letölteni : " + pictureName);

            yield return new WaitWhile(() => fileDownloads.Contains(pictureName));

            Debug.Log("Várakozásnak vége : " + pictureName);
        }

        Sprite resultSprite = null;

        string fileExtension = Path.GetExtension(pictureName);
        string base64pictureName = Common.Base64Encode(downloadDir + Path.GetFileNameWithoutExtension(pictureName)) + fileExtension;
        string fulldirPath = Path.Combine(Common.GetDocumentsDir(), dirName);
        string fullFileName = Path.Combine(fulldirPath, base64pictureName);

        Debug.Log("A " + pictureName + ":" + fullFileName + " nevű kép megszerzése");

        bool error = false;

        try
        {
            // Megnézzük, hogy a képneve benne van-e a szótárban, illetve megtalálható-e a háttértáron
            if (!datas.ContainsKey(base64pictureName))
                throw new Exception("A kép nincs a szótárban");

            if (!File.Exists(fullFileName))
                throw new Exception("A fájl nem létezik");

            // Ha létezik a fájl, akkor megkiséreljük a betöltését
            resultSprite = Common.MakeSpriteFromByteArray(File.ReadAllBytes(fullFileName));
            if (datas[base64pictureName] != 0)
            {
                datas[base64pictureName] = 0;
                needSave = true;
            }

            Debug.Log("Kép betöltve a háttértárról");

            //Texture2D texture = new Texture2D(2, 2);
            //texture.LoadImage(File.ReadAllBytes(fileName));
            //resultSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
        catch (Exception e)
        {
            Debug.Log("Háttértárról való kép betöltés sikertelen : " + e.Message);
            error = true;
        }

        if (error)
        {
            fileDownloads.Add(pictureName);
            // Ha nem sikerült a háttértárról betölteni, akkor a szerverről töltjük le
            // Elkészítjük a letöltési linket
            string link = Common.configurationController.getServerCommunicationLink();

            // Eltávolítjuk az utolsó / jel utáni tartalmat
            link = link.Substring(0, link.LastIndexOf('/') + 1);

            link += downloadDir + pictureName;

            Debug.Log(link);

            WWW www = new WWW(link);

            yield return www; // Várunk amíg befejeződik a letöltés

            string downloadStatus = "sikertelen";
            if (www.error == null)
            {
                resultSprite = Common.MakeSpriteFromByteArray(www.bytes);

                // Elmentjük a háttértárra is a képet
                Directory.CreateDirectory(fulldirPath);

                Debug.Log("Kép elmentése a háttértárra : " + www.bytes.Length);
                File.WriteAllBytes(fullFileName, www.bytes);
                if (datas.ContainsKey(base64pictureName))
                    datas[base64pictureName] = 0;
                else
                    datas.Add(base64pictureName, 0);

                needSave = true;

                downloadStatus = "sikeres";
            }

            Debug.Log("Kép letöltése a szerverről " + downloadStatus);

            fileDownloads.Remove(pictureName);
        }

        callBack(resultSprite);
    }

    // Update is called once per frame
    void Update()
    {
        // Ha letelt a számláló, akkor mentsük az adatokat
        saveCounter += Time.deltaTime;

        if (saveCounter > seconds)
        {
            saveCounter -= seconds;
            SaveIfNeed();
        }
    }

    void OnApplicationQuit()
    {
        SaveIfNeed();
    }
}