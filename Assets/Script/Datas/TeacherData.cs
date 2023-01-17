using UnityEngine;
using System.Collections;
using SimpleJSON;

public class TeacherData {

    public int userID;         // A tanár azonosítója
    public string userName;    // A tanár neve
    public string passMD5;     // A tanár password-jének MD5 változata
    public string email;       // A tanár email-je
    public string realName;    // A tanár valódi neve
    public int userType;       // A tanár típusa

    public TeacherData()
    {

    }

    public TeacherData(int? userID, string userName, string passMD5, string email, string realName, int? userType)
    {
        FillData(userID, userName, passMD5, email, realName, userType);
    }

    public TeacherData(JSONNode jsonData)
    {
        FillData(jsonData);
    }

    public void FillData(JSONNode jsonData)
    {
        if (jsonData.ContainsKey(C.JSONKeys.userID))
            userID = jsonData[C.JSONKeys.userID].AsInt;

        if (jsonData.ContainsKey(C.JSONKeys.username))
            userName = jsonData[C.JSONKeys.username].Value;

        if (jsonData.ContainsKey(C.JSONKeys.passMD5))
            passMD5 = jsonData[C.JSONKeys.passMD5].Value;

        if (jsonData.ContainsKey(C.JSONKeys.email))
            email = jsonData[C.JSONKeys.email].Value;

        if (jsonData.ContainsKey(C.JSONKeys.realName))
            realName = jsonData[C.JSONKeys.realName].Value;

        if (jsonData.ContainsKey(C.JSONKeys.usertype))
            userType = jsonData[C.JSONKeys.usertype].AsInt;
    }

    public void FillData(int? userID = null, string userName = null, string passMD5 = null, string email = null, string realName = null, int? userType = null)
    {
        if (userID != null)
            this.userID = userID.Value;

        if (userName != null)
            this.userName = userName;

        if (passMD5 != null)
            this.passMD5 = passMD5;

        if (email != null)
            this.email = email;

        if (realName != null)
            this.realName = realName;

        if (userType != null)
            this.userType = userType.Value;
    }

    //public bool 

    /// <summary>
    /// A TeacherData adatait a JSON osztályba menti.
    /// </summary>
    /// <returns>A visszaadott érték a JSON osztály.</returns>
    public JSONNode SaveDataToJSON()
    {
        JSONClass jsonData = new JSONClass();

        jsonData[C.JSONKeys.userID].AsInt = userID;
        jsonData[C.JSONKeys.username] = userName ?? "";
        jsonData[C.JSONKeys.passMD5] = passMD5 ?? "";
        jsonData[C.JSONKeys.email] = email ?? "";
        jsonData[C.JSONKeys.realName] = realName ?? "";
        jsonData[C.JSONKeys.usertype].AsInt = userType;

        return jsonData;
    }
}
