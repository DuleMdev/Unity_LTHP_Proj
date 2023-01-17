using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanguageData
{
    public string langID { get; private set; }
    public string langCode { get; private set; }
    public string langName { get; private set; }
    public Sprite langFlag { get; private set; }

    string langFlagRaw;

    public LanguageData()
    {
        langID = "";
        langCode = "";
        langName = "";
        langFlag = null;

        langFlagRaw = "";
    }

    public LanguageData(JSONNode node) {
        langID = node[C.JSONKeys.langID].Value;
        langCode = node[C.JSONKeys.langCode].Value;
        langName = node[C.JSONKeys.langName].Value;
        langFlag = Common.MakeSpriteFromByteArray(System.Convert.FromBase64String(node[C.JSONKeys.langFlag].Value));

        langFlagRaw = node[C.JSONKeys.langFlag].Value;
    }

    public LanguageData(string langID, string langCode, string langName)
    {
        this.langID = langID;
        this.langCode = langCode;
        this.langName = langName;
    }

    /// <summary>
    /// A json-ban megkapott nyelvi adatokat feldolgozza és egy listában vissza adja
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public static List<LanguageData> GetListOfLanguageData(JSONNode node)
    {
        List<LanguageData> list = new List<LanguageData>();

        for (int i = 0; i < node.Count; i++)
            list.Add(new LanguageData(node[i]));

        return list;
    }

    /// <summary>
    /// Vissza adja a nyelv adatait ha megtalálta a megadott szempont szerint. Elég egy paramétert megadni.
    /// </summary>
    /// <param name="langID"></param>
    /// <param name="langCode"></param>
    /// <param name="langName"></param>
    /// <returns></returns>
    public static LanguageData GetLanguageData(List<LanguageData> list, string langID = null, string langCode = null, string langName = null)
    {
        for (int i = 0; i < list.Count; i++)
        {
            bool ok = false;

            if (langID != null)
                if (!(ok = list[i].langID == langID))
                    continue; // Ha nem egyezik, jöhet a következő

            if (langCode != null)
                if (!(ok = list[i].langCode == langCode))
                    continue; // Ha nem egyezik, jöhet a következő

            if (langName != null)
                if (!(ok = list[i].langName == langName))
                    continue; // Ha nem egyezik, jöhet a következő

            // Ha legalább egy, de lehet, hogy több egyezett akkor kész vagyunk
            if (ok)
                return list[i];
        }

        return null;
    }

    /// <summary>
    /// Vissza adja a keresett nyelv adatait. A Search string lehet langID, langCode vagy langName. Ha nem talál egyezést, akkor null-t ad vissza.
    /// </summary>
    /// <param name="list">LanguageData lista amiben keresni kell.</param>
    /// <param name="search">Melyik nyelv adatait keressük.</param>
    /// <returns></returns>
    public static LanguageData GetLanguageData(List<LanguageData> list, string search)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].langID == search ||
                list[i].langCode == search || 
                list[i].langName == search
                )
                return list[i];
        }

        return null;
    }

    public JSONNode SaveDataToJson()
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.langID] = langID;
        jsonClass[C.JSONKeys.langCode] = langCode;
        jsonClass[C.JSONKeys.langName] = langName;
        jsonClass[C.JSONKeys.langFlag] = langFlagRaw;

        return jsonClass;
    }
}
