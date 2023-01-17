using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterData
{
    public string id { get; private set; }
    public string userID { get; private set; }
    public string name { get; private set; }
    public string image { get; private set; }
    public bool isDefault { get; private set; }

    public CharacterData(string id, string userID, string name, string image, bool isDefault)
    {
        this.id = id;
        this.userID = userID;
        this.name = name;
        this.image = image;
        this.isDefault = isDefault;
    }

    public CharacterData(JSONNode json)
    {
        id = json[C.JSONKeys.id];
        userID = json[C.JSONKeys.userID];
        name = json[C.JSONKeys.name];
        image = json[C.JSONKeys.image];
        isDefault = json[C.JSONKeys.isDefault].AsBool;
    }

    static public List<CharacterData> GetListOFCharacterData(JSONNode json)
    {
        List<CharacterData> list = new List<CharacterData>();
        for (int i = 0; i < json.Count; i++)
            list.Add(new CharacterData(json[i]));

        return list;
    }
}
