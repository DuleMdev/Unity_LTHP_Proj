using UnityEngine;
using System.Collections;
using SimpleJSON;
using System.Collections.Generic;

/*
ClassY játékhoz.

    */
public class SubFolderDatas {
    public int ID { get; private set; }
    public string name { get; private set; }
    public int notice { get; private set; }

    // Ha hiba történt a JsonArrayToList metódusban, akkor ezekkel a változókkal tudomást szerezhetünk róla
    public static bool isError;
    public static string errorMessage;

    public SubFolderDatas(JSONNode json) {
        ID = json[C.JSONKeys.id].AsInt;
        name = json[C.JSONKeys.name].Value;
        notice = json[C.JSONKeys.incompleteCurriculumNumber].AsInt;
    }

    public SubFolderDatas(int ID, string name, int notice) {
        this.ID = ID;
        this.name = name;
        this.notice = notice;
    }

    /*
    [
        {
            "id":"1",
            "name":"testCourse",
            "incompleteCurriculumNumber":"1"
        },
    ]
    */
    public static List<SubFolderDatas> JsonArrayToList(JSONNode json)
    {
        List<SubFolderDatas> list = new List<SubFolderDatas>();

        isError = false;

        try
        {
            for (int i = 0; i < json.Count; i++)
                list.Add(new SubFolderDatas(json[i]));
        }
        catch (System.Exception e)
        {
            isError = true;
            errorMessage = e.Message;

            // Hibát megmutatjuk a felhasználónak
            //throw;
        }

        return list;
    }
}
