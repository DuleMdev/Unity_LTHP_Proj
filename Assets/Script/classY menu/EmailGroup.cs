// Egy emaail csoport adatait tartalmazza, ahová aztán meg lehet hívni tagokat a web-es felületen

using SimpleJSON;
using System.Collections.Generic;
using UnityEngine;

public class EmailGroup
{
    public string id;
    public string name;             // A csoport neve
    public bool isPublic;             // A csoport publikus
    public string joinLevel;        // Csatlakozás típusa
    public bool autoAddUser;        // A felhasználót automatikusan adtuk a csoporthoz (nem tudja törölni magát a csoportból)
    public string appID;
    public string pictureName;
    public string description;
    public string languageID;
    public string cssClass;
    public UserStatus userStatus;
    public string invitationID;

    public bool isOwn;              // Az email csoportnak a felhasználó a létrehozója, tehát nem iratkozhat le róla

    public string ownerID;          // A csoport létrehozójáról információ
    public string ownerName;
    public string ownerEmail;
    public string ownerJoinedDate;
    public string ownerLanguageID;  // A csoport nyelve
    public string ownerProfilePicture;
    public string ownerDescription;


    Dictionary<string, bool> listOfPrivilege;

    public EmailGroup(JSONNode json)
    {
        id = json[C.JSONKeys.id].Value;
        name = json[C.JSONKeys.name].Value;
        isPublic = json[C.JSONKeys.isPublic].AsBool;
        joinLevel = json[C.JSONKeys.joinLevel].Value;
        autoAddUser = json[C.JSONKeys.autoAddUser].AsBool;
        appID = json[C.JSONKeys.appID].Value;
        pictureName = json[C.JSONKeys.picture].Value;
        description = json[C.JSONKeys.description].Value;
        languageID = json[C.JSONKeys.languageID].Value;
        cssClass = json[C.JSONKeys.cssClass].Value;
        userStatus = (UserStatus)Common.configurationController.ConvertTextToEnumValue<UserStatus>(json[C.JSONKeys.userStatus].Value, UserStatus.unknown);
        invitationID = json[C.JSONKeys.invitationID];

        isOwn = json[C.JSONKeys.isOwn].AsBool;

        ownerID = json[C.JSONKeys.owner][C.JSONKeys.userID].Value;
        ownerName = json[C.JSONKeys.owner][C.JSONKeys.userName].Value;
        ownerEmail = json[C.JSONKeys.owner][C.JSONKeys.userEmail].Value;
        ownerJoinedDate = json[C.JSONKeys.owner][C.JSONKeys.joinedDate].Value;
        ownerLanguageID = json[C.JSONKeys.owner][C.JSONKeys.languageID].Value;
        ownerProfilePicture = json[C.JSONKeys.owner][C.JSONKeys.profilePicture].Value;
        ownerDescription = json[C.JSONKeys.owner][C.JSONKeys.description].Value;

        listOfPrivilege = new Dictionary<string, bool>();

        for (int i = 0; i < json[C.JSONKeys.currentPrivileges].Count; i++)
        {
            Privileges privilege = (Privileges)Common.configurationController.ConvertTextToEnumValue<Privileges>(json[C.JSONKeys.currentPrivileges][i][C.JSONKeys.privilege], Privileges.unknown);

            if (privilege != Privileges.unknown)
                listOfPrivilege.Add(privilege.ToString(), json[C.JSONKeys.currentPrivileges][i][C.JSONKeys.hasPrivilege].AsBool);
        }
    }

    /// <summary>
    /// Vissza adja, hogy a megadott privilege milyen állapotban van
    /// </summary>
    /// <param name="privilege"></param>
    /// <returns></returns>
    public bool? getPrivilege(Privileges privilege)
    {
        if (listOfPrivilege.ContainsKey(privilege.ToString()))
            return listOfPrivilege[privilege.ToString()];

        return null;
    }

    public bool IsPlayEnabled()
    {
        bool? playEnabled = getPrivilege(Privileges.playContent);

        return playEnabled != null && playEnabled.Value && userStatus == UserStatus.subscribed;
    }

    public bool IsSubscribePossible() // Lehetséges a felíratkozás
    {
        // Feliratkozhatunk a csoportra ha nem vagyunk felíratkozva és publikus a csoport
        return userStatus == UserStatus.unsubscribed && isPublic;
    }

    public bool IsAcceptancePossible() // Elfogadás és Elutasítás válasz is lehetséges
    {
        // Ha meg van hívva
        return userStatus == UserStatus.invited;
    }

    public bool IsUnsubscribePossible() // Lehetéges a leiratkozás
    {
        // Leíratkozhatunk ha nem a miénk a csoport, ha nem automatikus a felíratkozás és feliratkoztunk vagy függőben van a feliratkozás elfogadása
        return !isOwn && !autoAddUser && (userStatus == UserStatus.subscribed);
    }

    public bool IsWaitForConfirmation() // Jelentkezés után a pozítiv vissza igazolásra várunk
    {
        return userStatus == UserStatus.pending;
    }

    public ButtonState GetButtonState()
    {
        if (IsSubscribePossible())
            return ButtonState.subscribe;

        if (IsUnsubscribePossible())
            return ButtonState.unsubscribe;

        if (IsAcceptancePossible())
            return ButtonState.acceptance;

        return ButtonState.nothing;
    }

    public enum Privileges
    {
        unknown,
        accessToSharedDesktop,
        editContent,
        editMailListData,
        manageHomeworks,
        manageUsers,
        playContent,
        useContent,
        copyContent,
        readReportings,
    }

    /*
    class PrivilegeData
    {
        public string name;
        public bool enabled;

        public PrivilegeData(JSONNode json)
        {
            name = json[C.JSONKeys.privilege].Value;
            enabled = json[C.JSONKeys.hasPrivilege].AsBool;
        }
    }
    */

    public enum UserStatus
    {
        unknown,        // Ismeretlen status
        unsubscribed,   // Nincs feliratkozva a csoportra           => Felíratkozás
        subscribed,     // Fel van íratkozva a csoportra            => Leíratkozás (ha nem saját csoport)
        pending,        // Feliratkozott és vár az engedélyezésre   => Leíratkozás (ha nem saját csoport)
        invited         // Meghívták a csoportba                    => Elfogadás / Elutasítás
    }

    public enum ButtonState
    {
        nothing,        // Nincs gomb
        subscribe,      // Felíratkozás
        unsubscribe,    // Leíratkozás
        acceptance      // Elfogadás / Elutasítás
    }

}