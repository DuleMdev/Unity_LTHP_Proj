using UnityEngine;
using System.Collections;
using SimpleJSON;
using System.Collections.Generic;

public class CurriculumItemDriveData {

    public string learnRoutePathID { get; private set; }
    public string learnRoutePathStart { get; private set; }
    public string subjectID { get; private set; }
    public string topicID { get; private set; }
    public string courseID { get; private set; }
    public string curriculumID { get; private set; }
    public string name { get; private set; }
    public float maxCurriculumProgress { get; private set; }
    public float scorePercent { get; private set; }
    public string notice { get; private set; }
    public string madeBy { get; private set; }
    public bool sync { get; private set; }
    public bool check { get; private set; }
    public string date { get; private set; }
    public string searchWords { get; private set; }
    public int points { get; private set; }
    public int stars { get; private set; }

    // Server2020
    public string newCurriculumIsolation { get; private set; }
    public string lastCurriculumIsolation { get; private set; }
    public string lastLogID { get; private set; }
    public string cssClass { get; private set; }

    // Ha hiba történt a JsonArrayToList metódusban, akkor ezekkel a változókkal tudomást szerezhetünk róla
    public static bool isError;
    public static string errorMessage;

    public CurriculumItemDriveData(JSONNode jsonNode)
    {
        learnRoutePathID = jsonNode[C.JSONKeys.learnRoutePathID];
        learnRoutePathStart = jsonNode[C.JSONKeys.learnRoutePathStart];

        subjectID = jsonNode[C.JSONKeys.subjectID];
        topicID = jsonNode[C.JSONKeys.topicID];
        courseID = jsonNode[C.JSONKeys.courseID];
        curriculumID = jsonNode[C.JSONKeys.curriculumID];

        name = jsonNode[C.JSONKeys.name];
        maxCurriculumProgress = jsonNode[C.JSONKeys.maxCurriculumProgress].AsFloat;
        scorePercent = jsonNode[C.JSONKeys.scorePercent].AsFloat;

        notice = "1";
        madeBy = "Gipsz Jakab";
        sync = false;
        check = false;
        date = "date is here";
        searchWords = "search words are here";
        points = 0;
        stars = 3;

        // Server2020
        newCurriculumIsolation = jsonNode[C.JSONKeys.newCurriculumIsolation];
        lastCurriculumIsolation = jsonNode[C.JSONKeys.lastCurriculumIsolation];
        lastLogID = jsonNode[C.JSONKeys.lastLogID];
        cssClass = jsonNode[C.JSONKeys.cssClass];

        // Az új szerveren más néven jön ez az érték
        if (jsonNode.ContainsKey(C.JSONKeys.progress))
            maxCurriculumProgress = jsonNode[C.JSONKeys.progress].AsFloat;
    }

    /*
    public CurriculumItemDriveData(string id, string notice, string text, string madeBy, bool sync, bool check) {
        this.curriculumID = id;
        this.notice = notice;
        this.name = text;
        this.madeBy = madeBy;
        this.sync = sync;
        this.check = check;
        this.date = "date is here";
        this.searchWords = "search words is here";
        this.points = 0;
        this.stars = 3;
    }
    */

    /*
    [
        {
            "id":"1",
            "name":"testCurriculum",
            "courseID":"1"
        },
    ]
    */
    public static List<CurriculumItemDriveData> JsonArrayToList(JSONNode json)
    {
        List<CurriculumItemDriveData> listOfCurriculums = new List<CurriculumItemDriveData>();

        try
        {
            for (int i = 0; i < json.Count; i++)
                listOfCurriculums.Add(new CurriculumItemDriveData(json[i]));
        }
        catch (System.Exception e)
        {
            isError = true;
            errorMessage = e.Message;
            // Hibát megmutatjuk a felhasználónak
            //throw;
        }

        return listOfCurriculums;
    }

    static public CurriculumItemDriveData GetItemByCurriculumID(List<CurriculumItemDriveData> list, string ID)
    {
        foreach (CurriculumItemDriveData item in list)
            if (item.curriculumID == ID)
                return item;

        return null;
    }
}
