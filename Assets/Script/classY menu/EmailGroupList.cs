using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Egy email csoport listát tartalmaz
/// </summary>
public class EmailGroupList
{
    public string requestName;
    public string color;
    public bool bigItems;
    public bool languageFilter; // Szürve lett-e a lista nyelvre

    public List<EmailGroup> list;

    public EmailGroupList(JSONNode json)
    {
        requestName =  json[C.JSONKeys.requestName];
        color = json[C.JSONKeys.color];
        bigItems = json[C.JSONKeys.bigItems].AsBool;
        languageFilter = json[C.JSONKeys.languageFilter].AsBool;

        list = new List<EmailGroup>();

        for (int i = 0; i < json[C.JSONKeys.answer].Count; i++)
            list.Add(new EmailGroup(json[C.JSONKeys.answer][i]));
    }

    public EmailGroup GetEmailGroupByID(string id)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].id == id)
                return list[i];
        }

        return null;
    }
}


